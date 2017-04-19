namespace BaristaLabs.Skrapr.Rules
{
    using BaristaLabs.Skrapr.Converters;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a rule that contains tasks that are run when the current frame matches a url pattern.
    /// </summary>
    public class UrlPatternRule : ISkraprRule
    {
        [JsonProperty("type")]
        public string Type
        {
            get { return "UrlPattern"; }
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
        public IList<ISkraprTask> Tasks
        {
            get;
            set;
        }

        public Task<bool> IsMatch(SkraprFrameState frameState)
        {
            return Task.FromResult(Regex.IsMatch(frameState.Url, UrlPattern, RegexOptions.IgnoreCase));
        }

        public override string ToString()
        {
            return UrlPattern;
        }
    }
}
