namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    public class RemoveDomElementTask : SkraprTask
    {
        public override string Name
        {
            get { return "RemoveDomElement"; }
        }

        public string Selector
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var nodeId = await worker.Session.DOM.GetNodeIdForSelector(Selector);
            if (nodeId < 1)
                return;

            await worker.Session.SendCommand(new Dom.RemoveNodeCommand()
            {
                NodeId = nodeId
            });
        }
    }
}
