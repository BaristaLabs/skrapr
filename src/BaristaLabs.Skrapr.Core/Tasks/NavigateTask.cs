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

        [JsonProperty("userAgent")]
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
            await worker.DevTools.Navigate(Url);
        }

        public override string ToString()
        {
            return Url;
        }
    }
}
