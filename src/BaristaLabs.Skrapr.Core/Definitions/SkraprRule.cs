namespace BaristaLabs.Skrapr.Definitions
{
    using BaristaLabs.Skrapr.Converters;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Represents a rule that contains tasks that is run when a set of preconditions apply.
    /// </summary>
    public class SkraprRule
    {
        /// <summary>
        /// Gets or sets a name for the rule
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the url pattern to use. This is a regular expression.
        /// </summary>
        [JsonProperty("urlPattern")]
        public string UrlPattern
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates if the rule is isolated. That is, the rule must be used by name from a task in order for the rule to be applied.
        /// </summary>
        [JsonProperty("isolated")]
        [DefaultValue(false)]
        public bool Isolated
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the mime type to use. This is a regular expression.
        /// </summary>
        [JsonProperty("mimeTypePattern")]
        [DefaultValue("text/html")]
        public string MimeType
        {
            get;
            set;
        }

        [JsonProperty("tasks", ItemConverterType = typeof(TaskConverter), DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public ICollection<ISkraprTask> Tasks
        {
            get;
            set;
        }
    }
}
