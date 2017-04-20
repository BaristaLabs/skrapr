namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    /// <summary>
    /// Highlights a rectangle shaped area.
    /// </summary>
    public class HighlightRectTask : SkraprTask
    {
        public override string Name
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

        public override async Task PerformTask(ISkraprWorker worker)
        {
            await worker.Session.SendCommand(new Dom.HighlightRectCommand
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
