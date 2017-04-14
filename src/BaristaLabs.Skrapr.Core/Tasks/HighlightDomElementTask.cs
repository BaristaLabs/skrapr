namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    public class HighlightDomElementTask : ITask
    {
        public string Name
        {
            get { return "HighlightDomElement"; }
        }

        public string Selector
        {
            get;
            set;
        }

        public Dom.HighlightConfig HighlightConfig
        {
            get;
            set;
        }

        public async Task PerformTask(SkraprContext context)
        {
            var layoutTreeNode = await context.DevTools.GetLayoutTreeNodeForDomElement(Selector);
            if (layoutTreeNode == null)
                return;

            var response = await context.Session.SendCommand<Dom.HighlightNodeCommand, Dom.HighlightNodeCommandResponse>(new Dom.HighlightNodeCommand
            {
                NodeId = layoutTreeNode.NodeId,
                ObjectId = null,
                HighlightConfig = HighlightConfig
            });
        }
    }
}
