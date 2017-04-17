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
    using Emulation = ChromeDevTools.Emulation;
    using Page = ChromeDevTools.Page;
    using Runtime = ChromeDevTools.Runtime;

    /// <summary>
    /// Represents a controller, or facade, over a ChromeSession to abstract away the intrincies of common operations.
    /// </summary>
    public class SkraprDevTools
    {
        #region Fields
        private readonly ILogger m_logger;
        private readonly string m_targetId;
        private readonly ChromeSession m_session;
        private string m_currentFrameId;
        private Runtime.ExecutionContextDescription m_currentFrameContext;

        private readonly ManualResetEventSlim m_pageStoppedLoading = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim m_childNodeEvent = new ManualResetEventSlim(false);
        private ConcurrentDictionary<long, Dom.Node> m_nodeDictionary = new ConcurrentDictionary<long, Dom.Node>();
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
            get { return !m_pageStoppedLoading.IsSet; }

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

        public async Task<JObject> GetPageDimensions()
        {
            var result = await Session.Runtime.Evaluate(@"
(function() {
    'use strict';

    var max = function (nums) {
        return Math.max.apply(Math, nums.filter(function(x) { return x; }));
    };

    var body = document.body;

    var originalX = window.scrollX,
        originalY = window.scrollY,
        originalOverflowStyle = document.documentElement.style.overflow;

    document.documentElement.style.overflow = 'hidden';

    var widths = [
        document.documentElement.clientWidth,
        body.scrollWidth,
        document.documentElement.scrollWidth,
        body.offsetWidth,
        document.documentElement.offsetWidth
    ];
    var heights = [
        document.documentElement.clientHeight,
        body.scrollHeight,
        document.documentElement.scrollHeight,
        body.offsetHeight,
        document.documentElement.offsetHeight
    ];

    var result = {
        fullWidth: max(widths),
        fullHeight: max(heights),
        windowWidth: window.innerWidth,
        windowHeight: window.innerHeight,
        devicePixelRatio: window.devicePixelRatio,
        originalOverflowStyle: originalOverflowStyle
    };

    document.documentElement.style.overflow = originalOverflowStyle;
    return JSON.stringify(result);
})();
", contextId: CurrentFrameContext.Id);

            var resultObject = JObject.Parse(result.Value as string);
            return resultObject;
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

        /// <summary>
        /// Injects a script element to the current page that contains inline script or points to an external script.
        /// </summary>
        /// <param name="scriptUrl"></param>
        /// <returns>The nodeId of the injected element.</returns>
        public async Task<long> InjectScriptElement(string scriptUrl, string contents = null, string type = "text/javascript", bool async = true, string condition = null)
        {
            if (!String.IsNullOrWhiteSpace(condition))
            {
                m_logger.LogDebug("{functionName} Condition parameter has been specified - determining if script should be injected.", nameof(InjectScriptElement));

                var shouldInjectJsResponse = await Session.Runtime.EvaluateCondition(condition, contextId: CurrentFrameContext.Id);
                if (!shouldInjectJsResponse)
                {
                    m_logger.LogDebug("{functionName} condition result was false - skipping script injection.", nameof(InjectScriptElement));
                    return -1;
                }
            }

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
        public async Task Navigate(string url, bool forceNavigate = false)
        {
            m_logger.LogDebug("{functionName} Navigating to {url}", nameof(Navigate), url);
            if (!forceNavigate)
            {
                var targetInfo = await Session.Target.GetTargetInfo(m_targetId);

                if (targetInfo.Url == url)
                {
                    m_logger.LogDebug("{functionName} No navigation needed - Current session currently at current page ({pageUrl})", nameof(Navigate), targetInfo.Url);
                    return;
                }
            }

            m_pageStoppedLoading.Reset();
            var navigateResponse = await m_session.Page.Navigate(new Page.NavigateCommand
            {
                Url = url
            });

            m_currentFrameId = navigateResponse.FrameId;
            m_logger.LogDebug("{functionName} Completed navigation to {url} (New frame id: {frameId})", nameof(Navigate), url, m_currentFrameId);
        }

        /// <summary>
        /// Waits until the current navigation to stop loading.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitForCurrentNavigation(int millisecondsTimeout = 15000)
        {
            m_logger.LogDebug("{functionName} Waiting for current navigation to complete.", nameof(WaitForCurrentNavigation));
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
            m_logger.LogDebug("{functionName} Waiting for next navigation.", nameof(WaitForNextNavigation));

            if (!IsLoading)
                m_pageStoppedLoading.Reset();

            await Task.Run(() => m_pageStoppedLoading.Wait(millisecondsTimeout));

            return IsLoading;
        }

        #region Private Methods

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
