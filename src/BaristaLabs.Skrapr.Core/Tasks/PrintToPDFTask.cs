namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PrintToPDFTask : ITask
    {
        public string Name
        {
            get { return "PrintToPDF"; }
        }

        public async Task PerformTask(SkraprContext context)
        {
            var base64Pdf = await context.Session.SendCommand("Page.printToPDF", JToken.FromObject(new object()), CancellationToken.None);

        }
    }
}
