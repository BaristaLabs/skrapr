namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Converters;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that contains a number of sub-tasks that will be performed.
    /// </summary>
    public class SubFlowTask : SkraprTask, IConditionalExpressionTask
    {
        public override string Name
        {
            get { return "SubFlow"; }
        }

        /// <summary>
        /// Gets or sets an optional expression that will be evaluated to determine if the sub-tasks will be processed.
        /// </summary>
        /// <remarks>
        /// If the condition is truthy, the sub-tasks will be processed.
        /// </remarks>
        public string Condition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of sub-tasks associated with this task that will be processed when this task is processed.
        /// </summary>
        [JsonProperty("tasks", ItemConverterType = typeof(TaskConverter), DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ISkraprTask> Tasks
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            worker.Logger.LogDebug("{taskName} Started processing subtasks.", Name);

            foreach (var task in Tasks)
            {
                await worker.ProcessSkraprTask(task);
            }

            worker.Logger.LogDebug("{taskName} Completed processing subtasks.", Name);
        }
    }
}
