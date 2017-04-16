namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.ChromeDevTools;
    using BaristaLabs.Skrapr.Definitions;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    /// Represents a class that processes a Skrapr Definition.
    /// </summary>
    public sealed class SkraprWorker : ISkraprWorker
    {
        private readonly ActionBlock<string> m_urlQueue;

        private readonly SkraprDefinition m_definition;
        private readonly SkraprDevTools m_devTools;
        private readonly ChromeSession m_session;

        public SkraprWorker(SkraprDefinition definition, ChromeSession session, SkraprDevTools devTools)
        {
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

        private async Task ProcessUrlQueue(string url)
        {
            //Navigate to the URL.
            await DevTools.Navigate(url);
            await DevTools.WaitForCurrentNavigation();
            var matchingRules = Definition.Rules.Where(r => Regex.IsMatch(url, r.UrlPattern));
            foreach (var rule in matchingRules)
            {
                await ProcessSkraprRule(rule);
            }
        }

        private async Task ProcessSkraprRule(SkraprRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            foreach(var task in rule.Tasks)
            {
                await task.PerformTask(this);
            }
        }

        public static SkraprWorker Create(string pathToSkraprDefinition, ChromeSession session, SkraprDevTools devTools)
        {
            if (!File.Exists(pathToSkraprDefinition))
                throw new FileNotFoundException($"The specified skrapr definition ({pathToSkraprDefinition}) could not be found. Please check that the skrapr definition exists.");

            var skraprDefinitionJson = File.ReadAllText(pathToSkraprDefinition);

            var skraprDefinition = JsonConvert.DeserializeObject<SkraprDefinition>(skraprDefinitionJson);
            return new SkraprWorker(skraprDefinition, session, devTools);
        }
    }
}
