namespace BaristaLabs.Skrapr.Tasks
{
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    public class SetAttributeValueTask : ITask
    {
        public string Name
        {
            get { return "SetAttributeValue"; }
        }

        public string Selector
        {
            get;
            set;
        }

        public string AttributeName
        {
            get;
            set;
        }

        public string AttributeValue
        {
            get;
            set;
        }

        public async Task PerformTask(SkraprContext context)
        {
            var nodeId = await context.DevTools.GetNodeForSelector(Selector);
            if (nodeId < 1)
                return;

            await context.Session.SendCommand(new Dom.SetAttributeValueCommand
            {
                NodeId = nodeId,
                Name = AttributeName,
                Value = AttributeValue
            });
        }
    }
}
