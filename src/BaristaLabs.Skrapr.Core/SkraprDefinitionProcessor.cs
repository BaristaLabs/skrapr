namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.ChromeDevTools;
    using BaristaLabs.Skrapr.Definitions;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a class that processes a Skrapr Definition.
    /// </summary>
    public sealed class SkraprDefinitionProcessor
    {
        private readonly BlockingCollection<string> m_queue;

        private readonly SkraprDefinition m_definition;
        private readonly SkraprDevTools m_devTools;
        private readonly ChromeSession m_session;

        public SkraprDefinitionProcessor(SkraprDefinition definition, ChromeSession session, SkraprDevTools devTools)
        {
            m_queue = new BlockingCollection<string>();

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
        public void Begin()
        {
            //Enqueue the start urls associated with the definition.
            foreach (var url in m_definition.StartUrls)
            {
                m_queue.Add(url);
            }

            //Start processing of the queue.
            ProcessQueue().GetAwaiter().GetResult();
        }

        private async Task ProcessQueue()
        {
            while (m_queue.TryTake(out string url, Timeout.Infinite))
            {
                //Navigate to the URL.
                DevTools.Navigate(url).Wait();
                DevTools.WaitForCurrentNavigation().Wait();
                var matchingRules = Definition.Rules.Where(r => Regex.IsMatch(url, r.UrlPattern));
                foreach(var rule in matchingRules)
                {
                    await ProcessSkraprRule(rule);
                }

                if (m_queue.Count == 0)
                    m_queue.CompleteAdding();
            }
        }

        private async Task ProcessSkraprRule(SkraprRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            var context = new SkraprContext
            {
                DevTools = DevTools,
                Session = Session
            };

            foreach(var task in rule.Tasks)
            {
                await task.PerformTask(context);
            }
        }

        public static SkraprDefinitionProcessor Create(string pathToSkraprDefinition, ChromeSession session, SkraprDevTools devTools)
        {
            if (!File.Exists(pathToSkraprDefinition))
                throw new FileNotFoundException($"The specified skrapr definition ({pathToSkraprDefinition}) could not be found. Please check that the skrapr definition exists.");

            var skraprDefinitionJson = File.ReadAllText(pathToSkraprDefinition);

            var skraprDefinition = JsonConvert.DeserializeObject<SkraprDefinition>(skraprDefinitionJson);
            return new SkraprDefinitionProcessor(skraprDefinition, session, devTools);
        }
    }
}
