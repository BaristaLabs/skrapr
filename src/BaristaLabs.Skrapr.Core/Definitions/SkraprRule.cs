namespace BaristaLabs.Skrapr.Definitions
{
    using BaristaLabs.Skrapr.Converters;
    using BaristaLabs.Skrapr.Tasks;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class SkraprRule
    {
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
        public ICollection<ITask> Tasks
        {
            get;
            set;
        }
    }
}
