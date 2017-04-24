namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.Converters;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a Skrapr rule
    /// </summary>
    public interface ISkraprRule
    {
        // <summary>
        /// Gets the type of the rule
        /// </summary>
        [JsonProperty("type")]
        string Type
        {
            get;
        }

        /// <summary>
        /// Gets a textual description of the purpose of intent of the rule.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// Indicates the maximum number of rule hits that should be allowed. (Optional. Default: 10)
        /// </summary>
        [JsonProperty("max", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(10)]
        int? Max
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of tasks that correspond to the rule.
        /// </summary>
        [JsonProperty("tasks", ItemConverterType = typeof(TaskConverter), DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        IList<ISkraprTask> Tasks
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a value that indicates if the rule matches the current state of the session.
        /// </summary>
        /// <param name="sessionInfo"></param>
        /// <returns></returns>
        Task<bool> IsMatch(SkraprFrameState sessionState);
    }
}
