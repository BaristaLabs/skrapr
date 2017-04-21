namespace BaristaLabs.Skrapr.Definitions
{
    using BaristaLabs.Skrapr.Converters;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class SkraprDefinition
    {
        /// <summary>
        /// Gets or sets the name of the skrapr.
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the Skrapr.
        /// </summary>
        [JsonProperty("description")]
        [DefaultValue(null)]
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a collection of urls that will be prevented from loading by the browser.
        /// </summary>
        /// <remarks>
        /// Use this to prevent tracking or heavy resources from loading.
        /// </remarks>
        [JsonProperty("blockedUrls")]
        public IList<string> BlockedUrls
        {
            get;
            set;
        }

        // <summary>
        /// Gets or sets a collection of urls that will be used as the start point for skraping.
        /// </summary>
        [JsonProperty("startUrls")]
        public IList<string> StartUrls
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a collection of rules.
        /// </summary>
        [JsonProperty("rules", ItemConverterType = typeof(RuleConverter), DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ISkraprRule> Rules
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a collection of interceptors that can save downloaded resources.
        /// </summary>
        [JsonProperty("resourceRules", ItemConverterType = typeof(RuleConverter), DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<string> ResourceRules
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a collection of white-listed urls for crawling.
        /// </summary>
        [JsonProperty("includeFilter", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public IList<string> IncludeFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a collection of black-listed urls for crawling.
        /// </summary>
        [JsonProperty("excludeFilter", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public IList<string> ExcludeFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates the maximum number of results to retrieve.
        /// </summary>
        /// <remarks>Once this value is reached, the skrapr will stop.</remarks>
        [JsonProperty("maxResults", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public int? MaxResults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of schedules. If no schedules are defined, the Skrapr will only execute manually.
        /// </summary>
        [JsonProperty("schedule", ItemConverterType = typeof(ScheduleConverter), DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public IList<ISchedule> Schedule
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates when the Skrapr was created.
        /// </summary>
        [JsonProperty("createdOn")]
        public DateTime? CreatedOn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates when the Skrapr Definition was last updated.
        /// </summary>
        [JsonProperty("lastUpdated")]
        public DateTime? LastUpdated
        {
            get;
            set;
        }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData
        {
            get;
            set;
        }
    }
}
