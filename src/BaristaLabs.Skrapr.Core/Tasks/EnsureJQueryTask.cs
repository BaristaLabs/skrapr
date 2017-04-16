namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;
    using System;

    public class EnsureJQueryTask : TaskBase
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
    
        public override Task PerformTask(SkraprContext context)
        {
            throw new NotImplementedException();
        }
    }
}
