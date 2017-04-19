namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Converters;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that contains a number of tasks that will be performed.
    /// </summary>
    public class SubFlowTask : SkraprTask
    {
        public override string Name
        {
            get { return "SubFlow"; }
        }

        public string Condition
        {
            get;
            set;
        }

        [JsonProperty("tasks", ItemConverterType = typeof(TaskConverter), DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ISkraprTask> Tasks
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            throw new NotImplementedException();
        }
    }
}
