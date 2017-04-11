namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.ChromeDevTools;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
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

        private SkraprDevTools(ChromeSession session)
        {
            m_session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <summary>
        /// Gets the Chrome session associated with this dev tools instance.
        /// </summary>
        public ChromeSession Session
        {
            get { return m_session; }
        }

        /// <summary>
        /// Gets a value that indicates if the current page is loading.
        /// </summary>
        public bool IsLoading
        {
            get { return !m_pageStoppedLoading.IsSet; }
        }

        /// <summary>
        /// Returns the present frame / resource tree structure.
        /// </summary>
        /// <returns></returns>
        public async Task<Page.FrameResourceTree> GetResourceTree()
        {
            var getFramesResponse = await m_session.SendCommand<Page.GetResourceTreeCommand, Page.GetResourceTreeCommandResponse>(new Page.GetResourceTreeCommand());
            return getFramesResponse.FrameTree;
        }

        public async Task Navigate(string url)
        {
            m_pageStoppedLoading.Reset();
            var navigateResponse = await m_session.SendCommand<Page.NavigateCommand, Page.NavigateCommandResponse>(new Page.NavigateCommand
            {
                Url = url
            });

            m_currentFrameId = navigateResponse.FrameId;
        }

        /// <summary>
        /// Waits until the next page stopped loading event
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitForPageToStopLoading(int millisecondsTimeout = 15000)
        {
            if (IsLoading)
                await Task.Run(() => m_pageStoppedLoading.Wait(millisecondsTimeout));

            return IsLoading;
        }

        /// <summary>
        /// Evaluates the specified script expression on global object.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="isPromise"></param>
        /// <returns></returns>
        public async Task<Runtime.RemoteObject> EvaluateScript(string script, bool isPromise = false)
        {
            var evaluateResponse = await m_session.SendCommand<Runtime.EvaluateCommand, Runtime.EvaluateCommandResponse>(new Runtime.EvaluateCommand
            {
                AwaitPromise = isPromise,
                ContextId = m_currentFrameContext.Id,
                Expression = script,
                GeneratePreview = false,
                IncludeCommandLineAPI = true,
                ObjectGroup = "console",
                ReturnByValue = false,
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

            //TODO: Don't sequentially await these.
            await Session.SendCommand(new Page.EnableCommand());
            var resourceTree = await GetResourceTree();
            m_currentFrameId = resourceTree.Frame.Id;
            await Session.SendCommand(new Runtime.EnableCommand());
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

        public static async Task<SkraprDevTools> Connect(ChromeSessionInfo sessionInfo)
        {
            if (sessionInfo == null)
                throw new ArgumentNullException(nameof(sessionInfo));

            //Create a new session using the information in the session info.
            var session = new ChromeSession(sessionInfo.WebSocketDebuggerUrl);
            var devTools = new SkraprDevTools(session);
            await devTools.Initialize();

            return devTools;
        }
    }
}
