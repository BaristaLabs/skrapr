namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;

    public interface ITask
    {
        string Name
        {
            get;
        }

        Task PerformTask(SkraprContext context);
    }
}
