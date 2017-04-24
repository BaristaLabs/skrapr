namespace BaristaLabs.Skrapr.Rules
{
    using Newtonsoft.Json;
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a rule that contains tasks that are run when the current frame matches a url pattern.
    /// </summary>
    public class UrlPatternRule : SkraprRule
    {
        [JsonProperty("type")]
        public override string Type
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

        public override Task<bool> IsMatch(SkraprFrameState frameState)
        {
            return Task.FromResult(Regex.IsMatch(frameState.Url, UrlPattern, RegexOptions.IgnoreCase));
        }

        public override string ToString()
        {
            return UrlPattern;
        }
    }
}
