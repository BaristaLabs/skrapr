namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    using Network = ChromeDevTools.Network;

    public class NavigateTask : ITask
    {
        public string Name
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

        public async Task PerformTask(SkraprContext context)
        {
            //If a useragent is specified, override the default user agent
            if (!String.IsNullOrWhiteSpace(UserAgent))
            {
                await context.DevTools.Session.SendCommand(new Network.SetUserAgentOverrideCommand
                {
                    UserAgent = UserAgent
                });
            }

            Console.WriteLine($"Navigating to {Url}");
            await context.DevTools.Navigate(Url);
        }
    }
}
