namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.ChromeDevTools;
    using BaristaLabs.Skrapr.Definitions;
    using BaristaLabs.Skrapr.Extensions;
    using BaristaLabs.Skrapr.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    /// Represents a concrete implementation of a local worker that processes a Skrapr Definition.
    /// </summary>
    public sealed class SkraprWorker : ISkraprWorker, IDisposable
    {
        private readonly ILogger m_logger;
        private readonly ActionBlock<ISkraprTask> m_mainFlow;

        private readonly SkraprDefinition m_definition;
        private readonly SkraprDevTools m_devTools;
        private readonly ChromeSession m_session;
        private readonly bool m_isDebugEnabled;

        public SkraprWorker(ILogger logger, SkraprDefinition definition, ChromeSession session, SkraprDevTools devTools, bool isDebugEnabled = false)
        {
            m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_logger = logger;
            m_mainFlow = new ActionBlock<ISkraprTask>(ProcessMainSkraprTask, new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1
            });

            m_devTools = devTools ?? throw new ArgumentNullException(nameof(devTools));
            m_session = session;
            m_definition = definition;

            m_isDebugEnabled = isDebugEnabled;
        }

        /// <summary>
        /// Gets a task that represents the asynchronous operation and completion of the Worker.
        /// </summary>
        public Task Completion
        {
            get { return m_mainFlow.Completion; }
        }

        /// <summary>
        /// Gets the Skrapr Definition that the worker is processing.
        /// </summary>
        public SkraprDefinition Definition
        {
            get { return m_definition; }
        }

        /// <summary>
        /// Gets the SkraprDevTools that are associated with the worker.
        /// </summary>
        public SkraprDevTools DevTools
        {
            get { return m_devTools; }
        }

        /// <summary>
        /// Gets a value that indicates if debug is enabled on the worker.
        /// </summary>
        public bool IsDebugEnabled
        {
            get { return m_isDebugEnabled; }
        }

        /// <summary>
        /// Gets the logger associated with the worker.
        /// </summary>
        public ILogger Logger
        {
            get { return m_logger; }
        }

        /// <summary>
        /// Gets the Chrome Session associated with the worker.
        /// </summary>
        public ChromeSession Session
        {
            get { return m_session; }
        }

        /// <summary>
        /// Gets the number of tasks waiting to be processed by the worker.
        /// </summary>
        public int TaskCount
        {
            get { return m_mainFlow.InputCount; }
        }

        /// <summary>
        /// Start processing the definition.
        /// </summary>
        public void AddStartUrls()
        {
            //Enqueue the start urls associated with the definition.
            foreach (var url in m_definition.StartUrls)
            {
                m_mainFlow.Post(new NavigateTask
                {
                    Url = url
                });
            }
        }

        /// <summary>
        /// Adds the specified target to the queue.
        /// </summary>
        /// <param name="target"></param>
        public void AddTask(ISkraprTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (m_disposed)
                return;

            Logger.LogDebug("{functionName} Added task {taskName} to the main flow. ({details})", nameof(AddTask), task.Name, task.ToString());
            m_mainFlow.Post(task);
        }

        /// <summary>
        /// Gets the rules that match the current state of the session.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ISkraprRule>> GetMatchingRules()
        {
            var frameState = await DevTools.GetCurrentFrameState();
            var matchingRules = new List<ISkraprRule>();

            foreach(var rule in Definition.Rules)
            {
                var ruleResult = await rule.IsMatch(frameState);
                if (ruleResult == true)
                    matchingRules.Add(rule);
            }

            return matchingRules;
        }

        /// <summary>
        /// Immediately processes the specified task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task ProcessSkraprTask(ISkraprTask task)
        {
            await ProcessSkraprTask(task, false);
        }

        /// <summary>
        /// Processes the specified task on the main skrapr flow.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async Task ProcessMainSkraprTask(ISkraprTask task)
        {
            await ProcessSkraprTask(task, true);

            //If there are no more tasks in the main flow, mark as complete.
            if (m_mainFlow.InputCount == 0)
            {
                m_logger.LogDebug("{functionName} Completed processing all tasks in the main flow. Completing.", nameof(ProcessSkraprTask));
                m_mainFlow.Complete();
            }
        }

        private async Task ProcessSkraprRule(ISkraprRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            foreach(var task in rule.Tasks)
            {
                await ProcessSkraprTask(task);
            }
        }

        /// <summary>
        /// Processes the specified ISkraprTask
        /// </summary>
        /// <param name="task"></param>
        /// <param name="processRules"></param>
        /// <returns></returns>
        private async Task ProcessSkraprTask(ISkraprTask task, bool processRules = false)
        {
            m_logger.LogDebug("{functionName} performing task {taskName}", nameof(ProcessSkraprTask), task.Name);

            if (task.Disabled)
            {
                m_logger.LogDebug("{functionName} Task {taskName} was marked as disabled. Skipping.", nameof(ProcessSkraprTask), task.Name);
                return;
            }

            if (task is IConditionalExpressionTask conditionalExpressionTask && !String.IsNullOrWhiteSpace(conditionalExpressionTask.Condition))
            {
                m_logger.LogDebug("{functionName} Task {taskName} is a conditional task. Evaluating expression {expression}.", nameof(ProcessSkraprTask), task.Name, conditionalExpressionTask.Condition);
                var conditionResponse = await Session.Runtime.EvaluateCondition(conditionalExpressionTask.Condition, contextId: DevTools.CurrentFrameContext.Id);
                if (conditionResponse == false)
                {
                    m_logger.LogDebug("{functionName} Condition result was false - skipping task {taskName}.", nameof(ProcessSkraprTask), task.Name);
                    return;
                }
                m_logger.LogDebug("{functionName} Condition result was true - processing task {taskName}.", nameof(ProcessSkraprTask), task.Name);
            }

            //Perform the task
            try
            {
                await task.PerformTask(this);
            }
            catch (Exception ex)
            {
                m_logger.LogError("{functionName} An error occurred while performing task {taskName}: {exceptionMessage} FrameId: {currentFrameId}", nameof(ProcessSkraprRule), task.Name, ex.Message, DevTools.CurrentFrameId);
                throw;
            }

            if (processRules == true)
            {
                //Get matching rules for the state of the session.
                var matchingRules = await GetMatchingRules();
                foreach (var rule in matchingRules)
                {
                    m_logger.LogDebug("{functionName} Found rule that matches current frame state: ({ruleType} - {details})", nameof(ProcessSkraprTask), rule.Type, rule.ToString());
                    await ProcessSkraprRule(rule);
                }

                if (matchingRules.Count() == 0)
                {
                    m_logger.LogError("{functionName} A rule was not found that matches the current frame state for task {taskName}: ({details})", nameof(ProcessSkraprTask), task.Name, task.ToString());
                }
            }

            m_logger.LogDebug("{functionName} Completed task {url}", nameof(ProcessSkraprTask), task.Name);
        }

        #region IDisposable
        private bool m_disposed;
        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    m_mainFlow.Complete();
                    Session.Dispose();
                }

                m_disposed = true;
            }
        }

        /// <summary>
        /// Disposes of the worker, freeing resources and marking it as complete.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        /// <summary>
        /// Creates a new SkraprWorker that can be processes the specified definition through tasks.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="pathToSkraprDefinition"></param>
        /// <param name="session"></param>
        /// <param name="devTools"></param>
        /// <param name="debugMode"></param>
        /// <returns></returns>
        public static SkraprWorker Create(IServiceProvider serviceProvider, string pathToSkraprDefinition, ChromeSession session, SkraprDevTools devTools, bool debugMode = false)
        {
            if (!File.Exists(pathToSkraprDefinition))
                throw new FileNotFoundException($"The specified skrapr definition ({pathToSkraprDefinition}) could not be found. Please check that the skrapr definition exists.");

            var skraprLogger = serviceProvider
                .GetService<ILoggerFactory>()
                .CreateLogger<SkraprWorker>();

            JToken skraprDefinitionJson;

            //The pyramid of json reading.
            using (var fs = File.OpenRead(pathToSkraprDefinition))
            {
                using (var textReader = new StreamReader(fs))
                {
                    using (var jsonReader = new JsonTextReader(textReader))
                    {
                        skraprDefinitionJson = JToken.ReadFrom(jsonReader, new JsonLoadSettings
                        {
                            CommentHandling = CommentHandling.Ignore,
                            LineInfoHandling = LineInfoHandling.Load
                        });
                    }
                }
            }

            var skraprDefinition = skraprDefinitionJson.ToObject<SkraprDefinition>();
            return new SkraprWorker(skraprLogger, skraprDefinition, session, devTools, debugMode);
        }
    }
}
