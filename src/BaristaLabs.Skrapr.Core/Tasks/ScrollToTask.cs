namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that positions the viewport to the first element that corresponds to the specified selector.
    /// </summary>
    public class ScrollTo : SkraprTask
    {
        public override string Name
        {
            get { return "ScrollTo"; }
        }

        /// <summary>
        /// Gets or sets the selector to scroll to.
        /// </summary>
        public string Selector
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            await worker.DevTools.ScrollTo(Selector, cancellationToken: worker.CancellationToken);
        }
    }
}
