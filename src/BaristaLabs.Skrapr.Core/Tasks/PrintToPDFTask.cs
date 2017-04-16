namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PrintToPDFTask : SkraprTask
    {
        public override string Name
        {
            get { return "PrintToPDF"; }
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var base64Pdf = await worker.Session.SendCommand("Page.printToPDF", JToken.FromObject(new object()), CancellationToken.None);

        }
    }
}
