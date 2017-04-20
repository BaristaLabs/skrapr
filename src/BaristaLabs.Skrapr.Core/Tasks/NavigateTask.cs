namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    using Network = ChromeDevTools.Network;

    /// <summary>
    /// Represents a task that navigates the current page to a specified url.
    /// </summary>
    public class NavigateTask : SkraprTask
    {
        public override string Name
        {
            get { return "Navigate"; }
        }

        /// <summary>
        /// Gets or sets the referrer URL
        /// </summary>
        [JsonProperty("referrer")]
        public string Referrer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URL to navigate to.
        /// </summary>
        [JsonProperty("url")]
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the useragent that will be used.
        /// </summary>
        [JsonProperty("userAgent")]
        public string UserAgent
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            //If a useragent is specified, override the default user agent
            if (!String.IsNullOrWhiteSpace(UserAgent))
            {
                await worker.DevTools.Session.Network.SetUserAgentOverride(new Network.SetUserAgentOverrideCommand
                {
                    UserAgent = UserAgent
                });
            }

            Console.WriteLine($"Navigating to {Url}");
            await worker.DevTools.Navigate(Url, referrer: Referrer);
        }

        public override string ToString()
        {
            return Url;
        }
    }
}
