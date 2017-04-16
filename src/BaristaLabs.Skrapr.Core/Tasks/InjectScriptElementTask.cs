namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that can be used to inject a script tag to an external JS resource.
    /// </summary>
    public class InjectScriptElementTask : SkraprTask
    {
        public override string Name
        {
            get { return "InjectScriptElement"; }
        }

        /// <summary>
        /// Gets or sets an optional expression that will be evaluated to determine if the script should be injected.
        /// </summary>
        /// <remarks>
        /// If the condition is truthy, the script tag will NOT be injected.
        /// </remarks>
        public string Condition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates if the condition returns a promise that should be awaited.
        /// </summary>
        public bool AwaitConditionPromise
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the type attribute of the script tag that will be injected. Defaults to text/javacript.
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the url to the javascript to inject.
        /// </summary>
        public string ScriptUrl
        {
            get;
            set;
        }

        public InjectScriptElementTask()
        {
            Type = "text/javascript";
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            await worker.DevTools.InjectScriptElement(ScriptUrl, Type, Condition, AwaitConditionPromise);
        }
    }
}
