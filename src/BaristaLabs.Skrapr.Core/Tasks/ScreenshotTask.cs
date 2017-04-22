namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that takes a screenshot of a page.
    /// </summary>
    public class ScreenshotTask : SkraprTask
    {
        public override string Name
        {
            get { return "Screenshot"; }
        }

        public string OutputFilename
        {
            get;
            set;
        }

        public ScreenshotTask()
        {
            OutputFilename = "screenshot.png";
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            await worker.Session.Page.TakeFullPageScreenshot(OutputFilename, cancellationToken: worker.CancellationToken);
        }
    }
}
