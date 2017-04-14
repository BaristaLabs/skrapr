namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    public class HighlightRectTask : ITask
    {
        public string Name
        {
            get { return "HighlightRect"; }
        }

        /// <summary>
        /// X coordinate
        /// </summary>
        [JsonProperty("x")]
        public long X
        {
            get;
            set;
        }


        /// <summary>
        /// Y coordinate
        /// </summary>
        [JsonProperty("y")]
        public long Y
        {
            get;
            set;
        }


        /// <summary>
        /// Rectangle width
        /// </summary>
        [JsonProperty("width")]
        public long Width
        {
            get;
            set;
        }


        /// <summary>
        /// Rectangle height
        /// </summary>
        [JsonProperty("height")]
        public long Height
        {
            get;
            set;
        }


        /// <summary>
        /// The highlight fill color (default: transparent).
        /// </summary>
        [JsonProperty("color", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dom.RGBA Color
        {
            get;
            set;
        }


        /// <summary>
        /// The highlight outline color (default: transparent).
        /// </summary>
        [JsonProperty("outlineColor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dom.RGBA OutlineColor
        {
            get;
            set;
        }

        public async Task PerformTask(SkraprContext context)
        {
            await context.Session.SendCommand(new Dom.HighlightRectCommand
            {
                X = X,
                Y = Y,
                Height = Height,
                Width = Width,
                Color = Color,
                OutlineColor = OutlineColor
            });
        }
    }
}
