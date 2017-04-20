namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that asserts the condition is true, and if not throws an exception.
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

        public string Message
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var result = await worker.Session.Runtime.EvaluateCondition(Assertion, worker.DevTools.CurrentFrameContext.Id);
            if (result == false)
                throw new AssertionFailedException(Message);
        }
    }
}
