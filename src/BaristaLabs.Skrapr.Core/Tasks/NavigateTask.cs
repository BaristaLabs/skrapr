namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    public class NavigateTask : ITask
    {
        public string TaskName
        {
            get { return "Navigate"; }
        }

        [JsonProperty("userAgent")]
        [DefaultValue("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.102 Safari/537.36")]
        public string UserAgent
        {
            get;
            set;
        }

        [JsonProperty("url")]
        public string Url
        {
            get;
            set;
        }
    }
}
