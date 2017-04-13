namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;

    public class ClickDomElementTask : ITask
    {
        public string Name
        {
            get { return "ClickDomElement"; }
        }

        public string Selector
        {
            get;
            set;
        }

        public async Task PerformTask(SkraprContext context)
        {
            await context.DevTools.ClickDomElement(Selector);
        }
    }
}
