namespace BaristaLabs.Skrapr
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an abstract implementation of the ITask interface.
    /// </summary>
    public abstract class SkraprTask : ISkraprTask
    {
        public abstract string Name
        {
            get;
        }

        public string Description
        {
            get;
            set;
        }

        public bool Disabled
        {
            get;
            set;
        }

        public abstract Task PerformTask(ISkraprWorker worker);

        public override string ToString()
        {
            return Name;
        }
    }
}
