namespace BaristaLabs.Skrapr.Tasks
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class PrintToPDFTask : SkraprTask
    {
        public override string Name
        {
            get { return "PrintToPDF"; }
        }

        /// <summary>
        /// Gets or sets the name of the file to output the pdf to.
        /// </summary>
        public string OutputFilename
        {
            get;
            set;
        }

        public PrintToPDFTask()
        {
            OutputFilename = "printtopdf.pdf";
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var result = await worker.Session.Page.PrintToPDF(new ChromeDevTools.Page.PrintToPDFCommand(), 60000);
            var imageBytes = Convert.FromBase64String(result.Data);
            worker.Logger.LogDebug("{taskName} Saving pdf to {fileName}", Name, OutputFilename);
            File.WriteAllBytes(OutputFilename, imageBytes);
            worker.Logger.LogDebug("{taskName} wrote {bytes} bytes to {fileName}", Name, imageBytes.LongCount(), OutputFilename);
            imageBytes = null;
            result = null;
        }
    }
}
