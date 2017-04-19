namespace BaristaLabs.Skrapr
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a Skrapr task that performs an action on a page.
    /// </summary>
    public interface ISkraprTask
    {
        /// <summary>
        /// Gets the name of the task
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Provides a short description of the purpose of the task.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// Gets a value that indicates if the task is disabled.
        /// </summary>
        bool Disabled
        {
            get;
        }

        Task PerformTask(ISkraprWorker worker);
    }

    /// <summary>
    /// Represents a Skrapr task that contains a condition.
    /// </summary>
    public interface IConditionalSkraprTask : ISkraprTask
    {
        string Condition
        {
            get;
        }
    }
}
