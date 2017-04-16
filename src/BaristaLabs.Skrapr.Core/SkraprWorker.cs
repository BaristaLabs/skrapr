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
        private readonly ActionBlock<string> m_urlQueue;

        private readonly SkraprDefinition m_definition;
        private readonly SkraprDevTools m_devTools;
        private readonly ChromeSession m_session;

        public SkraprWorker(ILogger logger, SkraprDefinition definition, ChromeSession session, SkraprDevTools devTools)
        {
            m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_logger = logger;
            m_urlQueue = new ActionBlock<string>(ProcessUrlQueue, new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1
            });

            m_devTools = devTools ?? throw new ArgumentNullException(nameof(devTools));
            m_session = session;
            m_definition = definition;
        }

        public SkraprDefinition Definition
        {
            get { return m_definition; }
        }

        public SkraprDevTools DevTools
        {
            get { return m_devTools; }
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
                m_urlQueue.Post(url);
            }
        }

        /// <summary>
        /// Adds the specified url to the queue.
        /// </summary>
        /// <param name="url"></param>
        public void AddUrl(string url)
        {
            m_urlQueue.Post(url);
        }

        /// <summary>
        /// Gets a count of the number of rules that match the specified url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public IEnumerable<SkraprRule> GetMatchingRules(string url)
        {
            return Definition.Rules.Where(r => Regex.IsMatch(url, r.UrlPattern));
        }

        private async Task ProcessUrlQueue(string url)
        {
            m_logger.LogDebug("{functionName} Started Processing {url}", nameof(ProcessUrlQueue), url);
            //Navigate to the URL.
            await DevTools.Navigate(url);
            var matchingRules = GetMatchingRules(url);
            foreach (var rule in matchingRules)
            {
                m_logger.LogDebug("{functionName} Found rule that matches current url: {url} ({ruleName} - {urlPattern})", nameof(ProcessUrlQueue), url, rule.Name, rule.UrlPattern);
                await ProcessSkraprRule(rule);
            }
            m_logger.LogDebug("{functionName} Completed Processing {url}", nameof(ProcessUrlQueue), url);
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
                    m_logger.LogError("{functionName} An error occurred while performing task {taskName}: {{exceptionMessage}}", nameof(ProcessSkraprRule), task.Name, ex.Message);
                }
            }
        }

        #region IDisposable
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_urlQueue.Complete();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public static SkraprWorker Create(IServiceProvider serviceProvider, string pathToSkraprDefinition, ChromeSession session, SkraprDevTools devTools)
        {
            if (!File.Exists(pathToSkraprDefinition))
                throw new FileNotFoundException($"The specified skrapr definition ({pathToSkraprDefinition}) could not be found. Please check that the skrapr definition exists.");

            var skraprDefinitionJson = File.ReadAllText(pathToSkraprDefinition);

            var skraprLogger = serviceProvider
                .GetService<ILoggerFactory>()
                .CreateLogger<SkraprWorker>();

            var skraprDefinition = JsonConvert.DeserializeObject<SkraprDefinition>(skraprDefinitionJson);
            return new SkraprWorker(skraprLogger, skraprDefinition, session, devTools);
        }
    }
}
