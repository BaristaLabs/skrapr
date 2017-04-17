namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that can be used to inject a style tag to inline CSS.
    /// </summary>
    public class InjectStyleElementTask : SkraprTask
    {
        public override string Name
        {
            get { return "InjectStyleElement"; }
        }

        /// <summary>
        /// Gets or sets the content of the style tag.
        /// </summary>
        /// <remarks>
        /// If the condition is truthy, the script tag will NOT be injected.
        /// </remarks>
        public string Styles
        {
            get;
            set;
        }
        public override async Task PerformTask(ISkraprWorker worker)
        {
            await worker.DevTools.InjectStyleElement(Styles);
        }
    }
}
