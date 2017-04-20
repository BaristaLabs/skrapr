namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System.Linq;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    /// <summary>
    /// Represents a task that highlights the specified dom elements based on a css selector.
    /// </summary>
    public class HighlightDomElementsTask : SkraprTask
    {
        public override string Name
        {
            get { return "HighlightDomElements"; }
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

        public Dom.RGBA Color
        {
            get;
            set;
        }

        public Dom.RGBA OutlineColor
        {
            get;
            set;
        }

        public HighlightDomElementsTask()
        {
            Color = new Dom.RGBA
            {
                R = 0,
                G = 0,
                B = 255,
                A = 0.7
            };

            OutlineColor = new Dom.RGBA
            {
                R = 255,
                G = 0,
                B = 0,
                A = 1
            };
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var documentNode = await worker.Session.DOM.GetDocument(1);
            var pageDimensions = await worker.Session.Page.GetPageDimensions(documentNodeId: documentNode.NodeId);

            var nodeIds = await worker.Session.DOM.QuerySelectorAll(new Dom.QuerySelectorAllCommand
            {
                NodeId = documentNode.NodeId,
                Selector = Selector
            });

            foreach (var nodeId in nodeIds.NodeIds)
            {
                var highlightObject = await worker.Session.DOM.GetHighlightObjectForTest(nodeId);
                var contentPath = highlightObject.Paths.FirstOrDefault(p => p.Name == "content");
                var contentPathPoints = contentPath.GetQuad();

                var targetRect = new Dom.Rect
                {
                    X = contentPathPoints[0] / pageDimensions.DevicePixelRatio,
                    Y = contentPathPoints[1] / pageDimensions.DevicePixelRatio,
                    Width = highlightObject.ElementInfo.NodeWidth / pageDimensions.DevicePixelRatio,
                    Height = highlightObject.ElementInfo.NodeHeight / pageDimensions.DevicePixelRatio //2.2
                };

                await worker.Session.DOM.HighlightRect(new Dom.HighlightRectCommand
                {
                    X = (long)(targetRect.X),
                    Y = (long)(targetRect.Y),
                    Width = (long)(targetRect.Width),
                    Height = (long)(targetRect.Height),
                    Color = Color,
                    OutlineColor = OutlineColor
                });
            }
        }
    }
}
