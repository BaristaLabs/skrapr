namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that can be used to inject a style tag to inline CSS.
    /// </summary>
    public class InjectStyleElementTask : SkraprTask, IConditionalExpressionTask
    {
        public override string Name
        {
            get { return "InjectStyleElement"; }
        }

        /// <summary>
        /// Gets or sets an optional expression that will be evaluated to determine if the style should be injected.
        /// </summary>
        /// <remarks>
        /// If the condition is truthy, the script tag will be injected.
        /// </remarks>
        public string Condition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content of the style tag.
        /// </summary>
        public string Styles
        {
            get;
            set;
        }
        public override async Task PerformTask(ISkraprWorker worker)
        {
            await worker.DevTools.InjectStyleElement(Styles, cancellationToken: worker.CancellationToken);
        }
    }
}
