namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Represents a task that ensures that jQuery is present on the page.
    /// </summary>
    public class EnsureJQueryTask : SkraprTask
    {
        public EnsureJQueryTask()
        {
            JQueryGlobalName = "jQuery";
        }

        public override string Name => "EnsureJQuery";

        public bool NoConflict
        {
            get;
            set;
        }

        public bool RemoveAll
        {
            get;
            set;
        }

        public string JQueryGlobalName
        {
            get;
            set;
        }
    
        public override Task PerformTask(ISkraprWorker worker)
        {
            throw new NotImplementedException();
        }
    }
}
