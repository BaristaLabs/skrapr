namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    public class RemoveDomElementTask : ITask
    {
        public string Name
        {
            get { return "RemoveDomElement"; }
        }

        public string Selector
        {
            get;
            set;
        }

        public async Task PerformTask(SkraprContext context)
        {
            var nodeId = await context.Session.DOM.GetNodeIdForSelector(Selector);
            if (nodeId < 1)
                return;

            await context.Session.SendCommand(new Dom.RemoveNodeCommand()
            {
                NodeId = nodeId
            });
        }
    }
}
