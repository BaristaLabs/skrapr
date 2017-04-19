namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;
    using Target = ChromeDevTools.Target;

    /// <summary>
    /// Represents a task that asserts the condition is true, and if not...
    /// </summary>
    public class AssertTask : SkraprTask
    {
        public override string Name
        {
            get { return "Assert"; }
        }

        public string Assertion
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

            worker.AddTask(new NavigateTask
            {
                Url = targetInfoResponse.TargetInfo.Url
            });
        }
    }
}
