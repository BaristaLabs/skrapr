namespace BaristaLabs.Skrapr.Schedules
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a base implementation of a SkraprSchedule.
    /// </summary>
    public abstract class SkraprSchedule : ISkraprSchedule
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
}
