namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.ChromeDevTools;
    using BaristaLabs.Skrapr.Definitions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    /// Represents a class that processes a Skrapr Definition.
    /// </summary>
    public sealed class SkraprWorker : ISkraprWorker, IDisposable
    {
        private readonly ILogger m_logger;
        private readonly ActionBlock<SkraprTarget> m_mainFlow;

        private readonly SkraprDefinition m_definition;
        private readonly SkraprDevTools m_devTools;
        private readonly ChromeSession m_session;
        private readonly bool m_isDebugEnabled;

        public SkraprWorker(ILogger logger, SkraprDefinition definition, ChromeSession session, SkraprDevTools devTools, bool isDebugEnabled = false)
        {
            m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_logger = logger;
            m_mainFlow = new ActionBlock<SkraprTarget>(ProcessSkraprTarget, new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1
            });

            m_devTools = devTools ?? throw new ArgumentNullException(nameof(devTools));
            m_session = session;
            m_definition = definition;

            m_isDebugEnabled = isDebugEnabled;
        }

        public SkraprDefinition Definition
        {
            get { return m_definition; }
        }

        public SkraprDevTools DevTools
        {
            get { return m_devTools; }
        }

        public bool IsDebugEnabled
        {
            get { return m_isDebugEnabled; }
        }

        public ILogger Logger
        {
            get { return m_logger; }
        }

        public ChromeSession Session
        {
            get { return m_session; }
        }

        /// <summary>
        /// Start processing the definition.
        /// </summary>
        public void AddStartUrls()
        {
            //Enqueue the start urls associated with the definition.
            foreach (var url in m_definition.StartUrls)
            {
                m_mainFlow.Post(new SkraprTarget(url, null));
            }
        }

        /// <summary>
        /// Adds the specified target to the queue.
        /// </summary>
        /// <param name="target"></param>
        public void AddTarget(SkraprTarget target)
        {
            m_mainFlow.Post(target);
        }

        /// <summary>
        /// Gets the rules that match the specified target.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public IEnumerable<SkraprRule> GetMatchingRules(SkraprTarget target)
        {
            if (!String.IsNullOrWhiteSpace(target.Rule))
                return Definition.Rules.Where(r => r.Name == target.Rule && r.Isolated == false);
            return Definition.Rules.Where(r => r.Isolated == false && Regex.IsMatch(target.Url, r.UrlPattern, RegexOptions.IgnoreCase));
        }

        private async Task ProcessSkraprTarget(SkraprTarget target)
        {
            m_logger.LogDebug("{functionName} Started Processing {url}", nameof(ProcessSkraprTarget), target.Url);
            //Navigate to the URL.
            await DevTools.Navigate(target.Url);
            var matchingRules = GetMatchingRules(target);
            foreach (var rule in matchingRules)
            {
                m_logger.LogDebug("{functionName} Found rule that matches current url: {url} ({ruleName} - {urlPattern})", nameof(ProcessSkraprTarget), target.Url, rule.Name, rule.UrlPattern);
                await ProcessSkraprRule(rule);
            }
            m_logger.LogDebug("{functionName} Completed Processing {url}", nameof(ProcessSkraprTarget), target.Url);
        }

        private async Task ProcessSkraprRule(SkraprRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            foreach(var task in rule.Tasks)
            {
                m_logger.LogDebug("{functionName} Performing task {taskName}", nameof(ProcessSkraprRule), task.Name);
                try
                {
                    await task.PerformTask(this);
                }
                catch(Exception ex)
                {
                    m_logger.LogError("{functionName} An error occurred while performing task {taskName}: {exceptionMessage} FrameId: {currentFrameId}", nameof(ProcessSkraprRule), task.Name, ex.Message, DevTools.CurrentFrameId);
                }
            }
        }

        #region IDisposable
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_mainFlow.Complete();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public static SkraprWorker Create(IServiceProvider serviceProvider, string pathToSkraprDefinition, ChromeSession session, SkraprDevTools devTools, bool debugMode = false)
        {
            if (!File.Exists(pathToSkraprDefinition))
                throw new FileNotFoundException($"The specified skrapr definition ({pathToSkraprDefinition}) could not be found. Please check that the skrapr definition exists.");

            var skraprDefinitionJson = File.ReadAllText(pathToSkraprDefinition);

            var skraprLogger = serviceProvider
                .GetService<ILoggerFactory>()
                .CreateLogger<SkraprWorker>();

            var skraprDefinition = JsonConvert.DeserializeObject<SkraprDefinition>(skraprDefinitionJson);
            return new SkraprWorker(skraprLogger, skraprDefinition, session, devTools, debugMode);
        }
    }
}
