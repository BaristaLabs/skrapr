namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that scrolls to the absolute bottom of the page
    /// </summary>
    public class ScrollToAbsoluteBottomTask : SkraprTask
    {
        public override string Name
        {
            get { return "ScrollToAbsoluteBottom"; }
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            await worker.DevTools.ScrollToAbsoluteBottom(cancellationToken: worker.CancellationToken);
        }
    }
}
