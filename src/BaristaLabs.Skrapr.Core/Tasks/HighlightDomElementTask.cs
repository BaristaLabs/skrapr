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
            System.Threading.Thread.Sleep(2000);
            var layoutTreeNode = await context.DevTools.GetLayoutTreeNodeForDomElement(Selector);
            if (layoutTreeNode == null)
                return;

            await context.Session.SendCommand(new Dom.SetInspectModeCommand
            {
                Mode = Dom.InspectMode.SearchForNode,
                HighlightConfig = new Dom.HighlightConfig
                {
                    ContentColor = new Dom.RGBA
                    {
                        R = 0,
                        G = 0,
                        B = 255,
                        A = 0.7
                    }
                }
            });

            await context.Session.SendCommand(new Dom.HighlightRectCommand
            {
                X = 100,
                Y = 100,
                Height = 100,
                Width = 100,
                Color = new Dom.RGBA
                {
                    R = 1,
                    G = 1,
                    B = 255,
                    A = 0.5
                },
                OutlineColor = new Dom.RGBA
                {
                    R = 255,
                    G = 1,
                    B = 1,
                    A = 1
                }
            });

            await context.Session.SendCommand(new Dom.HideHighlightCommand());
            await context.Session.SendCommand(new Dom.SetInspectedNodeCommand
            {
                NodeId = layoutTreeNode.NodeId
            });

            var response = await context.Session.SendCommand<Dom.HighlightNodeCommand, Dom.HighlightNodeCommandResponse>(new Dom.HighlightNodeCommand
            {
                NodeId = layoutTreeNode.NodeId,
                HighlightConfig = new Dom.HighlightConfig
                {
                    ContentColor = new Dom.RGBA
                    {
                        R = 0,
                        G = 0,
                        B = 255,
                        A = 0.7
                    }
                }
                //HighlightConfig = HighlightConfig
            });
        }
    }
}
