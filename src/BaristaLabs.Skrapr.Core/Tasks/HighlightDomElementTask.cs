namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System.Linq;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    /// <summary>
    /// Represents a task that highlights the specified dom element based on a css selector.
    /// </summary>
    public class HighlightDomElementTask : SkraprTask
    {
        public override string Name
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

        public HighlightDomElementTask()
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
            var scaleFactor = await worker.DevTools.GetPageScaleFactor();
            var xScaleFactor = scaleFactor.Item1;
            var yScaleFactor = scaleFactor.Item2;

            var documentNode = await worker.Session.DOM.GetDocument(1);

            var nodeIds = await worker.Session.DOM.QuerySelectorAll(new Dom.QuerySelectorAllCommand
            {
                NodeId = documentNode.NodeId,
                Selector = Selector
            });

            //var nodeId = nodeIds.NodeIds.First();
            //if (nodeId < 1)
            //    return;
            foreach (var nodeId in nodeIds.NodeIds)
            {
                var highlightObject = await worker.Session.DOM.GetHighlightObjectForTest(nodeId);
                var contentPath = highlightObject.Paths.FirstOrDefault(p => p.Name == "content");
                var contentPathPoints = contentPath.GetQuad();

                var targetRect = new Dom.Rect
                {
                    X = contentPathPoints[0] / xScaleFactor,
                    Y = contentPathPoints[1] / yScaleFactor,
                    Width = highlightObject.ElementInfo.NodeWidth / xScaleFactor,
                    Height = highlightObject.ElementInfo.NodeHeight / yScaleFactor //2.2
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
