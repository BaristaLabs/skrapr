namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.ChromeDevTools;
    using BaristaLabs.Skrapr.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Troschuetz.Random;
    using Css = ChromeDevTools.CSS;
    using Dom = ChromeDevTools.DOM;
    using Emulation = ChromeDevTools.Emulation;
    using Input = ChromeDevTools.Input;
    using Page = ChromeDevTools.Page;
    using Runtime = ChromeDevTools.Runtime;

    /// <summary>
    /// Represents a controller, or facade, over a ChromeSession to abstract away the intrincies of common operations.
    /// </summary>
    public class SkraprDevTools : IDisposable
    {
        #region Fields
        private readonly ILogger m_logger;
        private readonly string m_targetId;
        private readonly ChromeSession m_session;
        private string m_currentFrameId;
        private Runtime.ExecutionContextDescription m_currentFrameContext;

        private readonly ManualResetEventSlim m_frameStoppedLoading = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim m_childNodeEvent = new ManualResetEventSlim(false);
        private ConcurrentDictionary<long, Dom.Node> m_nodeDictionary = new ConcurrentDictionary<long, Dom.Node>();
        private static TRandom s_random = TRandom.New(new Troschuetz.Random.Generators.NR3Generator());
        #endregion

        #region Properties

        /// <summary>
        /// Gets an IDictionary of the child nodes currently seen by the dev tools -- use the GetChildNodeData to obtain information about a specific node.
        /// </summary>
        public IDictionary<long, Dom.Node> ChildNodes
        {
            get { return m_nodeDictionary; }
        }

        /// <summary>
        /// Gets the frame id associated with the current session.
        /// </summary>
        public string CurrentFrameId
        {
            get { return m_currentFrameId; }
        }

        /// <summary>
        /// Gets the frame context associated with the current session.
        /// </summary>
        public Runtime.ExecutionContextDescription CurrentFrameContext
        {
            get { return m_currentFrameContext; }
        }

        /// <summary>
        /// Gets a value that indicates if the current page is loading.
        /// </summary>
        public bool IsLoading
        {
            get { return !m_frameStoppedLoading.IsSet; }

        }
        /// <summary>
        /// Gets the Chrome session associated with this dev tools instance.
        /// </summary>
        public ChromeSession Session
        {
            get { return m_session; }
        }

        /// <summary>
        /// Gets the id of the target associated with the dev tools.
        /// </summary>
        public string TargetId
        {
            get { return m_targetId; }
        }
        #endregion

        /// <summary>
        /// Creates a new instance of the SkraprDevTools using the provided sesion.
        /// </summary>
        /// <param name="session"></param>
        private SkraprDevTools(ILogger logger, ChromeSession session, string targetId)
        {
            m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_session = session ?? throw new ArgumentNullException(nameof(session));
            m_targetId = targetId;
        }

        /// <summary>
        /// Gets information about the current frame.
        /// </summary>
        /// <returns></returns>
        public async Task<SkraprFrameState> GetCurrentFrameState()
        {
            var targetInfo = await Session.Target.GetTargetInfo(TargetId);
            var frameResourceTree = await Session.Page.GetResourceTree(new Page.GetResourceTreeCommand());

            //TODO: add things like current javascript global vars.

            return new SkraprFrameState
            {
                Title = targetInfo.Title,
                Url = targetInfo.Url,
                FrameTree = frameResourceTree.FrameTree
            };
        }

        public async Task<BoundingClientRect> GetBoundingClientRect(long nodeId)
        {
            var resolveNodeResponse = await Session.DOM.ResolveNode(new Dom.ResolveNodeCommand
            {
                NodeId = nodeId,
                ObjectGroup = "Skrapr"
            });

            var boundingClientRectResponse = await m_session.Runtime.CallFunctionOn(new Runtime.CallFunctionOnCommand
            {
                ObjectId = resolveNodeResponse.Object.ObjectId,
                FunctionDeclaration = "function() { return this.getBoundingClientRect(); }",
                Silent = true,
                UserGesture = false
            });

            if (boundingClientRectResponse.Result.Subtype == "error")
                return null;

            var propertiesResponse = await m_session.Runtime.GetProperties(new Runtime.GetPropertiesCommand
            {
                ObjectId = boundingClientRectResponse.Result.ObjectId
            });

            var properties = propertiesResponse.Result;
            var result = new BoundingClientRect()
            {
                Top = double.Parse(properties.First(p => p.Name == "top").Value.Description),
                Right = double.Parse(properties.First(p => p.Name == "right").Value.Description),
                Bottom = double.Parse(properties.First(p => p.Name == "bottom").Value.Description),
                Left = double.Parse(properties.First(p => p.Name == "left").Value.Description),
                Width = double.Parse(properties.First(p => p.Name == "width").Value.Description),
                Height = double.Parse(properties.First(p => p.Name == "height").Value.Description)
            };

            //Cleanup.
            var releaseResponse = await m_session.Runtime.ReleaseObject(new Runtime.ReleaseObjectCommand
            {
                ObjectId = boundingClientRectResponse.Result.ObjectId
            });
            return result;
        }

        /// <summary>
        /// Gets the Css bounding box for the specified node.
        /// </summary>
        /// <remarks>
        /// If the nodeId doesn't correspond to a dom element or the dom element is not visible, null will be returned.
        /// 
        /// Um, not sure what determines if the node is within the returned objects. Document(-1) doesn't seems to get all nodes either.
        /// </remarks>
        /// <param name="cssSelector"></param>
        /// <returns></returns>
        public async Task<Dom.Rect> GetBoundingBoxForNode(long nodeId, double devicePixelRatio = 2)
        {
            //Ensure the node is pushed.
            await m_session.DOM.GetDocument(-1);
            await m_session.DOM.RequestChildNodes(new Dom.RequestChildNodesCommand
            {
                NodeId = nodeId
            });

            var layoutTree = await m_session.CSS.GetLayoutTreeAndStyles(new Css.GetLayoutTreeAndStylesCommand
            {
                ComputedStyleWhitelist = new string[0]
            });

            var layoutTreeNode = layoutTree.LayoutTreeNodes.FirstOrDefault(e => e.NodeId == nodeId);
            if (layoutTreeNode == null)
                return null;

            return new Dom.Rect
            {
                Height = layoutTreeNode.BoundingBox.Height / devicePixelRatio,
                Width = layoutTreeNode.BoundingBox.Width / devicePixelRatio,
                X = layoutTreeNode.BoundingBox.X / devicePixelRatio,
                Y = layoutTreeNode.BoundingBox.Y / devicePixelRatio
            };
        }


        /// <summary>
        /// Retrieves the position of the element relative to the document.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public async Task<BoundingClientRect> GetOffset(long nodeId)
        {
            var resolveNodeResponse = await Session.DOM.ResolveNode(new Dom.ResolveNodeCommand
            {
                NodeId = nodeId,
                ObjectGroup = "Skrapr"
            });

            var boundingClientRectResponse = await m_session.Runtime.CallFunctionOn(new Runtime.CallFunctionOnCommand
            {
                ObjectId = resolveNodeResponse.Object.ObjectId,
                FunctionDeclaration = @"function() {
    var doc, docElem, rect, win, elem = this;

    if ( !elem.getClientRects().length ) {
	    return { top: 0, bottom: 0, left: 0, right: 0, height: 0, width: 0 };
    }

    rect = elem.getBoundingClientRect();

    doc = elem.ownerDocument;
    docElem = doc.documentElement;
    win = doc.defaultView;

    return {
	    top: rect.top + win.pageYOffset - docElem.clientTop,
        bottom: rect.bottom + win.pageYOffset - docElem.clientTop,
	    left: rect.left + win.pageXOffset - docElem.clientLeft,
        right: rect.right + win.pageXOffset - docElem.clientLeft,
        height: rect.height,
        width: rect.width
    };
}",
                Silent = true,
                UserGesture = false
            });

            if (boundingClientRectResponse.Result.Subtype == "error")
                return null;

            var propertiesResponse = await m_session.Runtime.GetProperties(new Runtime.GetPropertiesCommand
            {
                ObjectId = boundingClientRectResponse.Result.ObjectId
            });

            var properties = propertiesResponse.Result;
            var result = new BoundingClientRect()
            {
                Top = double.Parse(properties.First(p => p.Name == "top").Value.Description),
                Right = double.Parse(properties.First(p => p.Name == "right").Value.Description),
                Bottom = double.Parse(properties.First(p => p.Name == "bottom").Value.Description),
                Left = double.Parse(properties.First(p => p.Name == "left").Value.Description),
                Width = double.Parse(properties.First(p => p.Name == "width").Value.Description),
                Height = double.Parse(properties.First(p => p.Name == "height").Value.Description)
            };

            //Cleanup.
            var releaseResponse = await m_session.Runtime.ReleaseObject(new Runtime.ReleaseObjectCommand
            {
                ObjectId = boundingClientRectResponse.Result.ObjectId
            });

            return result;
        }

        /// <summary>
        /// Injects a script element to the current page that contains inline script or points to an external script.
        /// </summary>
        /// <param name="scriptUrl"></param>
        /// <returns>The nodeId of the injected element.</returns>
        public async Task<long> InjectScriptElement(string scriptUrl, string contents = null, string type = "text/javascript", bool async = true)
        {
            m_logger.LogDebug("{functionName} injecting script tag with src={scriptUrl} type={type}", nameof(InjectScriptElement), scriptUrl, type);

            var result = await Session.Runtime.Evaluate($@"
new Promise(function (resolve, reject) {{
    'use strict';
    var r = false;
    var s = document.createElement('script');
    s.type = {type.GetJSValue()};
    s.src = {scriptUrl.GetJSValue()};
    s.async = {async.GetJSValue()};
    s.text = {contents.GetJSValue()};
    s.onload = s.onreadystatechange = function() {{
        if (!r && (!this.readyState || this.readyState == 'complete')) {{
            r = true;
            resolve(this);
        }}
    }};
    s.onerror = s.onabort = reject;
    document.body.appendChild(s);
}});
            ", contextId: CurrentFrameContext.Id, awaitPromise: true);

            var nodeResponse = await Session.DOM.RequestNode(new Dom.RequestNodeCommand
            {
                ObjectId = result.ObjectId
            });

            return nodeResponse.NodeId;
        }

        /// <summary>
        /// Injects a style element into the current page.
        /// </summary>
        /// <param name="scriptUrl"></param>
        /// <returns>The nodeId of the injected element.</returns>
        public async Task<long> InjectStyleElement(string styles, string type = "text/css")
        {
            m_logger.LogDebug("{functionName} injecting style tag.", nameof(InjectStyleElement));

            var result = await Session.Runtime.Evaluate($@"
(function() {{
    'use strict';
    var s = document.createElement('style');
    s.type = {type.GetJSValue()};
    s.innerText = {styles.GetJSValue()};
    document.head.appendChild(s);
    return s;
}})();
            ", contextId: CurrentFrameContext.Id, awaitPromise: false);

            var nodeResponse = await Session.DOM.RequestNode(new Dom.RequestNodeCommand
            {
                ObjectId = result.ObjectId
            });

            return nodeResponse.NodeId;
        }

        /// <summary>
        /// Instructs the current session to navigate to the specified Url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="forceNavigate"></param>
        /// <returns></returns>
        public async Task Navigate(string url, string referrer = null, bool forceNavigate = false, int millisecondsTimeout = 15000)
        {
            m_logger.LogDebug("{functionName} Navigating to {url}", nameof(Navigate), url);
            if (!forceNavigate)
            {
                var targetInfo = await Session.Target.GetTargetInfo(m_targetId);

                if (targetInfo.Url == url)
                {
                    m_logger.LogDebug("{functionName} No navigation needed - Current session currently at target page ({pageUrl})", nameof(Navigate), targetInfo.Url);
                    return;
                }
            }

            var navigateResponse = await m_session.Page.Navigate(new Page.NavigateCommand
            {
                Referrer = referrer,
                Url = url
            });
            m_currentFrameId = navigateResponse.FrameId;

            await WaitForCurrentNavigation(millisecondsTimeout: millisecondsTimeout);
            m_logger.LogDebug("{functionName} Completed navigation to {url} (New frame id: {frameId})", nameof(Navigate), url, m_currentFrameId);
        }

        /// <summary>
        /// Scrolls to the specified selector
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="isHuman"></param>
        /// <param name="maxScrolls"></param>
        /// <param name="postScrollDelayMs">Occassionally on pages there will be post-scroll pop-in (usually of images) which will reposition the elements on screen. This delay waits for the specified number of sections after a scroll.</param>
        /// <returns></returns>
        public async Task ScrollTo(string selector, bool isHuman = true, int maxScrolls = 10, int postScrollDelayMs = 1500)
        {
            var documentNode = await Session.DOM.GetDocument(1);

            //Obtaining the page dimensions prior to getting the selector
            var pageDimensions = await Session.Runtime.GetReportedPageDimensions(contextId: m_currentFrameContext.Id);

            //Get the node to scroll to.
            var nodeIds = await Session.DOM.QuerySelectorAll(new Dom.QuerySelectorAllCommand
            {
                NodeId = documentNode.NodeId,
                Selector = selector
            });

            if (nodeIds.NodeIds.Length == 0)
            {
                m_logger.LogError("{functionName} Did not find any nodes corresponding to selector {selector}, skipping.", nameof(ScrollTo), selector);
                return;
            }

            var nodeId = nodeIds.NodeIds.First();
            if (nodeId < 1)
                return;

            await Session.DOM.HighlightNode(new Dom.HighlightNodeCommand
            {
                NodeId = nodeId,
                HighlightConfig = new Dom.HighlightConfig
                {
                    ContentColor = new Dom.RGBA
                    {
                        R = 0,
                        G = 0,
                        B = 255,
                        A = 0.7
                    }
                }
            });

            var scrollsLeft = maxScrolls;
            Tuple<double, double> delta;
            do
            {
                //Get the scroll delta.)
                pageDimensions = await Session.Runtime.GetReportedPageDimensions(contextId: m_currentFrameContext.Id);
                var targetObject = await GetOffset(nodeId);
                delta = targetObject.GetOnscreenDelta(pageDimensions);

                //If the element is already on screen, well, we're done.
                if (delta.Item1 >= -1 && delta.Item1 <= 1 &&
                    delta.Item2 >= -1 && delta.Item2 <= 1)
                {
                    m_logger.LogDebug("{functionName} Target element is already within the boundries of the current viewport.", nameof(ScrollTo));
                    return;
                }

                if (isHuman)
                {
                    var scrollPoint = new Point(
                        s_random.NextUInt(10, (uint)pageDimensions.WindowWidth - 20),
                        s_random.NextUInt(10, (uint)pageDimensions.WindowHeight - 20)
                        );

                    var targetInfoResponse = await Session.Input.SynthesizeScrollGesture(new Input.SynthesizeScrollGestureCommand
                    {
                        X = (long)scrollPoint.X,
                        Y = (long)scrollPoint.Y,
                        XDistance = (long)Math.Ceiling(delta.Item1),
                        YDistance = (long)Math.Ceiling(delta.Item2),
                        Speed = s_random.Next(400, 1200)
                    }, millisecondsTimeout: 120000);
                }
                else
                {
                    throw new NotImplementedException();
                }

                scrollsLeft--;
                Thread.Sleep(postScrollDelayMs);
            } while (scrollsLeft > 0);

            m_logger.LogError("{functionName} exceeded the maximum number of scrolls: {maxScrolls}", nameof(ScrollTo), maxScrolls);
        }
        /// <summary>
        /// Keeps scrolling to the bottom of the page until the scroll position stablizes
        /// </summary>
        /// <param name="maxScrolls"></param>
        /// <returns></returns>
        public async Task ScrollToAbsoluteBottom(int maxScrolls = 10, int iterateDelayMS = 1000)
        {
            long lastScrollY = -1, scrollY = -1, yPos;
            var scrollsLeft = maxScrolls;
            do
            {
                var pageDimensions = await Session.Page.GetPageDimensions();
                lastScrollY = scrollY;
                scrollY = (long)pageDimensions.ScrollY;

                if (lastScrollY == scrollY)
                    return;

                yPos = (long)pageDimensions.FullHeight;
                await Session.Runtime.Evaluate($"window.scrollTo(0, {yPos});", m_currentFrameContext.Id);
                scrollsLeft--;
                await Task.Delay(iterateDelayMS);
            } while (scrollsLeft > 0);

            m_logger.LogError("{functionName} exceeded the maximum number of scrolls: {maxScrolls}", nameof(ScrollToAbsoluteBottom), maxScrolls);
        }


        /// <summary>
        /// Waits until the current navigation to stop loading.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitForCurrentNavigation(int millisecondsTimeout = 15000)
        {
            m_logger.LogDebug("{functionName} Waiting for current navigation to complete. FrameId: {frameId}", nameof(WaitForCurrentNavigation), m_currentFrameId);
            var completed = await Task.Run(() => m_frameStoppedLoading.Wait(millisecondsTimeout));
            if (!completed)
                throw new NavigationFailedException("Navigation timed out while waiting for current navigation to complete.");

            return IsLoading;
        }

        /// <summary>
        /// Waits for the next navigation to stop loading.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public async Task<bool> WaitForNextNavigation(int millisecondsTimeout = 15000)
        {
            m_logger.LogDebug("{functionName} Waiting for next navigation.", nameof(WaitForNextNavigation));

            if (!IsLoading)
                m_frameStoppedLoading.Reset();

            var completed = await Task.Run(() => m_frameStoppedLoading.Wait(millisecondsTimeout));
            if (!completed)
                throw new NavigationFailedException("Navigation timed out while waiting for next navigation to complete.");

            return IsLoading;
        }

        #region Private Methods

        private async Task Initialize()
        {
            // Subscribe to and enable various events - This mimics the chrome dev tools initialization
            //Note: View the DevTools interaction with chrome by popping open a new
            //Chrome Debug session, browsing to http://localhost:<port>/ in another
            //browser instance, and looking at the WS traffic in devtools.

            Session.Subscribe<Page.FrameStartedLoadingEvent>(ProcessFrameStartedLoading);
            Session.Subscribe<Page.FrameStoppedLoadingEvent>(ProcessFrameStoppedLoading);

            Session.Subscribe<Runtime.ExecutionContextCreatedEvent>(ProcessExecutionContextCreated);
            Session.Subscribe<Dom.DocumentUpdatedEvent>(ProcessDocumentUpdatedEvent);
            Session.Subscribe<Dom.SetChildNodesEvent>(ProcessSetChildNodesEvent);

            //TODO: Don't sequentially await these.
            await Session.Emulation.ResetViewport(new Emulation.ResetViewportCommand());
            await Session.Emulation.ResetPageScaleFactor(new Emulation.ResetPageScaleFactorCommand());
            await Session.SendCommand(new Page.EnableCommand());
            var resourceTree = await Session.Page.GetResourceTree();
            m_currentFrameId = resourceTree.Frame.Id;
            await Session.SendCommand(new Runtime.EnableCommand());
            await Session.SendCommand(new Dom.EnableCommand());
        }

        private void ProcessExecutionContextCreated(Runtime.ExecutionContextCreatedEvent e)
        {
            var auxData = e.Context.AuxData as JObject;
            var frameId = auxData["frameId"].Value<string>();

            if (m_currentFrameId == frameId)
            {
                m_currentFrameContext = e.Context;
            }
        }

        private void ProcessFrameStartedLoading(Page.FrameStartedLoadingEvent e)
        {
            if (m_currentFrameId == e.FrameId)
            {
                m_frameStoppedLoading.Reset();
            }
        }

        private void ProcessFrameStoppedLoading(Page.FrameStoppedLoadingEvent e)
        {
            if (m_currentFrameId == e.FrameId)
            {
                m_frameStoppedLoading.Set();
            }
        }

        private void ProcessDocumentUpdatedEvent(Dom.DocumentUpdatedEvent e)
        {
            m_nodeDictionary.Clear();
        }

        private void ProcessSetChildNodesEvent(Dom.SetChildNodesEvent e)
        {
            foreach(var node in e.Nodes)
            {
                m_nodeDictionary.AddOrUpdate(node.NodeId, node, (id, previousNode) => node);
            }
        }
        #endregion

        #region IDisposable
        private bool m_disposed;

        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    m_session.Dispose();

                    m_frameStoppedLoading.Dispose();
                    m_childNodeEvent.Dispose();
                    m_nodeDictionary.Clear();
                }

                m_disposed = true;
            }
        }

        /// <summary>
        /// Disposes of the DevTools, freeing resources and marking it as complete.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Static Members

        /// <summary>
        /// Connects to the specified chrome session.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="sessionInfo"></param>
        /// <returns></returns>
        public static async Task<SkraprDevTools> Connect(IServiceProvider serviceProvider, ChromeSessionInfo sessionInfo)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (sessionInfo == null)
                throw new ArgumentNullException(nameof(sessionInfo));

            var chromeSessionLogger = serviceProvider
                .GetService<ILoggerFactory>()
                .CreateLogger<ChromeSession>();

            var devToolsLogger = serviceProvider
                .GetService<ILoggerFactory>()
                .CreateLogger<SkraprDevTools>();

            //Create a new session using the information in the session info.
            var session = new ChromeSession(chromeSessionLogger, sessionInfo.WebSocketDebuggerUrl);
            var devTools = new SkraprDevTools(devToolsLogger, session, sessionInfo.Id);
            await devTools.Initialize();

            return devTools;
        }
        #endregion
    }
}
