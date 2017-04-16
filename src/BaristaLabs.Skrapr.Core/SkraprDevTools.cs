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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Css = ChromeDevTools.CSS;
    using Dom = ChromeDevTools.DOM;
    using Input = ChromeDevTools.Input;
    using Page = ChromeDevTools.Page;
    using Runtime = ChromeDevTools.Runtime;

    /// <summary>
    /// Represents a controller, or facade, over a ChromeSession to abstract away the intrincies of common operations.
    /// </summary>
    public class SkraprDevTools
    {
        private readonly ChromeSession m_session;
        private string m_currentFrameId;
        private Runtime.ExecutionContextDescription m_currentFrameContext;

        private readonly ManualResetEventSlim m_pageStoppedLoading = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim m_childNodeEvent = new ManualResetEventSlim(false);
        private ConcurrentDictionary<long, Dom.Node> m_nodeDictionary = new ConcurrentDictionary<long, Dom.Node>();


        #region Properties

        /// <summary>
        /// Gets an IDictionary of the child nodes currently seen by the dev tools -- use the GetChildNodeData to obtain information about a specific node.
        /// </summary>
        public IDictionary<long, Dom.Node> ChildNodes
        {
            get { return m_nodeDictionary; }
        }

        /// <summary>
        /// Gets a value that indicates if the current page is loading.
        /// </summary>
        public bool IsLoading
        {
            get { return !m_pageStoppedLoading.IsSet; }

        }
        /// <summary>
        /// Gets the Chrome session associated with this dev tools instance.
        /// </summary>
        public ChromeSession Session
        {
            get { return m_session; }
        }
        #endregion

        /// <summary>
        /// Creates a new instance of the SkraprDevTools using the provided sesion.
        /// </summary>
        /// <param name="session"></param>
        private SkraprDevTools(ChromeSession session)
        {
            m_session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <summary>
        /// Instructs the current session to navigate to the specified Url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="forceNavigate"></param>
        /// <returns></returns>
        public async Task Navigate(string url, bool forceNavigate = false)
        {
            if (!forceNavigate)
            {
                var tree = await Session.Page.GetResourceTree();
                if (tree.Frame.Url == url)
                    return;
            }

            m_pageStoppedLoading.Reset();
            var navigateResponse = await m_session.SendCommand<Page.NavigateCommand, Page.NavigateCommandResponse>(new Page.NavigateCommand
            {
                Url = url
            });

            m_currentFrameId = navigateResponse.FrameId;
        }

        /// <summary>
        /// Determine the scale factor for the current page by comparing the BoxModel of the document to the ViewPort Client Info
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<double, double>> GetPageScaleFactor()
        {
            var documentNode = await Session.DOM.GetDocument();
            var documentBoxModel = await Session.DOM.GetBoxModel(new Dom.GetBoxModelCommand
            {
                NodeId = documentNode.NodeId
            });

            var documentLayoutMetrics = await Session.Page.GetLayoutMetrics();

            double scaleX = documentLayoutMetrics.LayoutViewport.ClientWidth / documentBoxModel.Model.Width;
            double scaleY = documentLayoutMetrics.LayoutViewport.ClientHeight / documentBoxModel.Model.Height;
            return new Tuple<double, double>(scaleX, scaleY);
        }

        public async Task<bool> GetChildNodeData(long nodeId, long depth = 1, bool pierce = false)
        {
            //TODO: Implement this.
            throw new NotImplementedException();

            //m_childNodeEvent.Reset();
            //await m_session.SendCommand(new Dom.RequestChildNodesCommand
            //{
            //    NodeId = nodeId,
            //    Depth = depth,
            //    Pierce = pierce
            //});
            //await Task.Run(() => m_childNodeEvent.Wait());
            //return false;
        }

        /// <summary>
        /// Waits until the current navigation to stop loading.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitForCurrentNavigation(int millisecondsTimeout = 15000)
        {
            await Task.Run(() => m_pageStoppedLoading.Wait(millisecondsTimeout));

            return IsLoading;
        }

        /// <summary>
        /// Waits for the next navigation to stop loading.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public async Task<bool> WaitForNextNavigation(int millisecondsTimeout = 15000)
        {
            if (!IsLoading)
                m_pageStoppedLoading.Reset();

            await Task.Run(() => m_pageStoppedLoading.Wait(millisecondsTimeout));

            return IsLoading;
        }

        /// <summary>
        /// Gets the layout tree node for the dom element corresponding to the specified selector.
        /// </summary>
        /// <remarks>
        /// If the selector doesn't correspond to a dom element or the dom element is not visible, null will be returned.
        /// </remarks>
        /// <param name="cssSelector"></param>
        /// <returns></returns>
        public async Task<Css.LayoutTreeNode> GetLayoutTreeNodeForDomElement(string cssSelector)
        {
            var document = await Session.DOM.GetDocument();
            var domElement = await m_session.SendCommand<Dom.QuerySelectorCommand, Dom.QuerySelectorCommandResponse>(new Dom.QuerySelectorCommand
            {
                NodeId = document.NodeId, //Document node id is probably most likely always 1.
                Selector = cssSelector
            });

            if (domElement.NodeId <= 0)
                return null;

            var elementCssComputedStyle = await m_session.SendCommand<Css.GetLayoutTreeAndStylesCommand, Css.GetLayoutTreeAndStylesCommandResponse>(new Css.GetLayoutTreeAndStylesCommand
            {
                ComputedStyleWhitelist = new string[0]
            });

            return elementCssComputedStyle.LayoutTreeNodes.FirstOrDefault(e => e.NodeId == domElement.NodeId);
        }

        public async Task<bool> ClickDomElement(string cssSelector)
        {
            var layoutTreeNode = await GetLayoutTreeNodeForDomElement(cssSelector);

            if (layoutTreeNode == null)
                return false;

            var toClick = layoutTreeNode.BoundingBox.GetMiddleOfRect();

            await m_session.SendCommand(new Input.DispatchMouseEventCommand
            {
                Button = "left",
                Type = "mousePressed",
                ClickCount = 1,
                Modifiers = 0,
                X = (long)toClick.X,
                Y = (long)toClick.Y,
                Timestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds()
            });

            return true;
        }

        /// <summary>
        /// Evaluates the specified script expression on global object.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="isPromise"></param>
        /// <returns></returns>
        public async Task<Runtime.RemoteObject> EvaluateScript(string script, bool byValue = false, bool isPromise = false)
        {
            var evaluateResponse = await m_session.SendCommand<Runtime.EvaluateCommand, Runtime.EvaluateCommandResponse>(new Runtime.EvaluateCommand
            {
                AwaitPromise = isPromise,
                ContextId = m_currentFrameContext.Id,
                Expression = script,
                GeneratePreview = false,
                IncludeCommandLineAPI = true,
                ObjectGroup = "Skrapr",
                ReturnByValue = byValue,
                Silent = false,
                UserGesture = true
            });

            return evaluateResponse.Result;
        }

        private async Task Initialize()
        {
            // Subscribe to and enable various events - This mimics the chrome dev tools initialization
            //Note: View the DevTools interaction with chrome by popping open a new
            //Chrome Debug session, browsing to http://localhost:<port>/ in another
            //browser instance, and looking at the WS traffic in devtools.

            Session.Subscribe<Page.FrameNavigatedEvent>(ProcessFrameNavigatedEvent);
            Session.Subscribe<Page.FrameStoppedLoadingEvent>(ProcessFrameStoppedLoading);

            Session.Subscribe<Runtime.ExecutionContextCreatedEvent>(ProcessExecutionContextCreated);
            Session.Subscribe<Dom.DocumentUpdatedEvent>(ProcessDocumentUpdatedEvent);
            Session.Subscribe<Dom.SetChildNodesEvent>(ProcessSetChildNodesEvent);

            //TODO: Don't sequentially await these.
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

        private void ProcessFrameNavigatedEvent(Page.FrameNavigatedEvent e)
        {
            if (m_currentFrameId == e.Frame.Id)
            {
                m_pageStoppedLoading.Reset();
            }
        }

        private void ProcessFrameStoppedLoading(Page.FrameStoppedLoadingEvent e)
        {
            if (m_currentFrameId == e.FrameId)
            {
                m_pageStoppedLoading.Set();
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

        public static async Task<SkraprDevTools> Connect(IServiceProvider serviceProvider, ChromeSessionInfo sessionInfo)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (sessionInfo == null)
                throw new ArgumentNullException(nameof(sessionInfo));

            var logger = serviceProvider
                .GetService<ILoggerFactory>()
                .CreateLogger<ChromeSession>();

            //Create a new session using the information in the session info.
            var session = new ChromeSession(logger, sessionInfo.WebSocketDebuggerUrl);
            var devTools = new SkraprDevTools(session);
            await devTools.Initialize();

            return devTools;
        }
    }
}
