namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;

    public class HumanDelayTask : SkraprTask
    {
        public override string Name
        {
            get { return "HumanDelay"; }
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            //For a random period of time, move the mouse around, scroll up and down, hover over anchor tags, etc.

            //await worker.Session.DOM.QuerySelectorAll(new ChromeDevTools.DOM.QuerySelectorAllCommand
            //{
            //    Selector = "a"
            //});

            
        }
    }
}
