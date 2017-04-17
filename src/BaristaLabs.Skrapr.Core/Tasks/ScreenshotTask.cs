namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Dom = ChromeDevTools.DOM;
    using Emulation = ChromeDevTools.Emulation;
    using Page = ChromeDevTools.Page;

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
            await worker.DevTools.TakeFullPageScreenshot(OutputFilename);
        }
    }
}
