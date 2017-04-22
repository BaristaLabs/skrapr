namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;
    using Target = ChromeDevTools.Target;

    /// <summary>
    /// Represents a task that adds the url of the page to the queue.
    /// </summary>
    public class AddCurrentPageAsTargetTask : SkraprTask
    {
        public override string Name
        {
            get { return "AddCurrentPageAsTarget"; }
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var targetInfoResponse = await worker.Session.Target.GetTargetInfo(new Target.GetTargetInfoCommand
            {
                TargetId = worker.DevTools.TargetId,
            });

            worker.Post(new NavigateTask
            {
                Url = targetInfoResponse.TargetInfo.Url
            });
        }
    }
}
