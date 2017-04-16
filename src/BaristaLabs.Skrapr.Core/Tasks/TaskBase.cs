namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an abstract implementation of the ITask interface.
    /// </summary>
    public abstract class TaskBase : ITask
    {
        public abstract string Name
        {
            get;
        }

        public abstract Task PerformTask(SkraprContext context);

        public override string ToString()
        {
            return Name;
        }
    }
}
