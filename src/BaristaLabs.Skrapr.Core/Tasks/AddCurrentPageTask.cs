namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;
    using Target = ChromeDevTools.Target;

    /// <summary>
    /// Represents a task that adds the url of the page to the queue.
    /// </summary>
    public class AddCurrentPageTask : SkraprTask
    {
        public override string Name
        {
            get { return "AddCurrentPage"; }
        }

        public string Rule
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var targetInfoResponse = await worker.Session.Target.GetTargetInfo(new Target.GetTargetInfoCommand
            {
                TargetId = worker.DevTools.TargetId,
            });

            worker.AddTarget(new SkraprTarget(targetInfoResponse.TargetInfo.Url, Rule));
        }
    }
}
