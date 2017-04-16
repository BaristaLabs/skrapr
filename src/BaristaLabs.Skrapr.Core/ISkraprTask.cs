namespace BaristaLabs.Skrapr
{
    using System.Threading.Tasks;

    public interface ISkraprTask
    {
        string Name
        {
            get;
        }

        Task PerformTask(ISkraprWorker worker);
    }
}
