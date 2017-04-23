namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel;
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
        /// Forces the page to reload if the current page is already at the specified url.
        /// </summary>
        public bool Force
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before the navigation times out. (Optional, default: 15000)
        /// </summary>
        [JsonProperty("millisecondsTimeout", NullValueHandling = NullValueHandling.Ignore)]
        [DefaultValue(15000)]
        public int? MillisecondsTimeout
        {
            get;
            set;
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

            if (MillisecondsTimeout.HasValue == false)
                MillisecondsTimeout = 15000;

            await worker.DevTools.Navigate(Url, referrer: Referrer, forceNavigate: Force, cancellationToken: worker.CancellationToken, millisecondsTimeout: MillisecondsTimeout.Value);
        }

        public override string ToString()
        {
            return Url;
        }
    }
}
