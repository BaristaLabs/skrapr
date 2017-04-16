namespace BaristaLabs.Skrapr.Tasks
{
    using Page = ChromeDevTools.Page;
    using System.Threading.Tasks;
    using System;
    using System.IO;

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
            var result = await worker.Session.Page.CaptureScreenshot(new Page.CaptureScreenshotCommand());
            var imageBytes = Convert.FromBase64String(result.Data);
            File.WriteAllBytes(OutputFilename, imageBytes);
        }
    }
}
