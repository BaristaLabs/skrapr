namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    /// <summary>
    /// Represents a task that sets the specified attributes of the element matching the selector to the specified value.
    /// </summary>
    public class SetAttributeValueTask : SkraprTask
    {
        public override string Name
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

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var nodeId = await worker.Session.DOM.GetNodeIdForSelector(Selector);
            if (nodeId < 1)
                return;

            await worker.Session.SendCommand(new Dom.SetAttributeValueCommand
            {
                NodeId = nodeId,
                Name = AttributeName,
                Value = AttributeValue
            });
        }
    }
}
