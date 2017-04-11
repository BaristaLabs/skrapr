namespace BaristaLabs.Skrapr.Definitions
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public abstract class Schedule : ISchedule
    {
        /// <summary>
        /// Gets the type of the schedule
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        public abstract string Type
        {
            get;
        }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a schedule defined as a Cron Expression.
    /// </summary>
    public class CronSchedule : Schedule
    {
        public override string Type
        {
            get { return "cron"; }
        }

        [JsonProperty("cronExpression", Required = Required.Always)]
        public string CronExpression
        {
            get;
            set;
        }
    }
}
