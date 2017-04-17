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
            dynamic dimensions = await worker.DevTools.GetPageDimensions();

            await worker.Session.Emulation.SetVisibleSize(new Emulation.SetVisibleSizeCommand
            {
                Width = (long)dimensions.fullWidth,
                Height = (long)dimensions.fullHeight
            });

            var result = await worker.Session.SendCommand<Page.CaptureScreenshotCommand, Page.CaptureScreenshotCommandResponse>(new Page.CaptureScreenshotCommand(), millisecondsTimeout: 60000);
            var imageBytes = Convert.FromBase64String(result.Data);
            File.WriteAllBytes(OutputFilename, imageBytes);
            imageBytes = null;

            await worker.Session.Emulation.SetVisibleSize(new Emulation.SetVisibleSizeCommand
            {
                Width = (long)dimensions.windowWidth,
                Height = (long)dimensions.windowHeight
            });

        }
    }
}
