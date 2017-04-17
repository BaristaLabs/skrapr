namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Troschuetz.Random;
    using Dom = ChromeDevTools.DOM;
    using Input = ChromeDevTools.Input;

    /// <summary>
    /// Represents a task that clicks a specified element on the page by selector.
    /// </summary>
    public class ClickDomElementTask : SkraprTask
    {
        public override string Name
        {
            get { return "ClickDomElement"; }
        }

        /// <summary>
        /// none, left, middle, right
        /// </summary>
        public string Button
        {
            get;
            set;
        }

        /// <summary>
        /// The css selector of the element to be clicked.
        /// </summary>
        public string Selector
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that determines if the click will cause a navigation event.
        /// </summary>
        public bool IsNavigation
        {
            get;
            set;
        }

        public ClickDomElementTask()
        {
            Button = "left";
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var documentNode = await worker.Session.DOM.GetDocument(1);

            //Get the node to click.
            var nodeIds = await worker.Session.DOM.QuerySelectorAll(new Dom.QuerySelectorAllCommand
            {
                NodeId = documentNode.NodeId,
                Selector = Selector
            });

            var nodeId = nodeIds.NodeIds.First();
            if (nodeId < 1)
                return;

            //TODO: Position the viewport so the element is on screen if isn't already.

            //Get the highlight object and create a rectangle representing the click area.
            var highlightObject = await worker.Session.DOM.GetHighlightObjectForTest(nodeId);
            var contentPath = highlightObject.Paths.FirstOrDefault(p => p.Name == "content");
            var contentPathPoints = contentPath.GetQuad();

            var targetClickRect = new Dom.Rect
            {
                X = contentPathPoints[0],
                Y = contentPathPoints[1],
                Width = highlightObject.ElementInfo.NodeWidth,
                Height = highlightObject.ElementInfo.NodeHeight
            };

            //Get a random point within the click area
            var target = targetClickRect.GetRandomSpotWithinRect();

            //Click the random point, with a random delay between the down and up mouse events.
            var clickDelay = TRandom.New().Next(100, 1000);

            await worker.Session.Input.DispatchMouseEvent(new Input.DispatchMouseEventCommand
            {
                Button = Button,
                Type = "mousePressed",
                ClickCount = 1,
                Modifiers = 0,
                X = (long)target.X,
                Y = (long)target.Y,
                Timestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds()
            });

            await Task.Run(() => Thread.Sleep(clickDelay));

            await worker.Session.Input.DispatchMouseEvent(new Input.DispatchMouseEventCommand
            {
                Button = Button,
                Type = "mouseReleased",
                ClickCount = 1,
                Modifiers = 0,
                X = (long)target.X,
                Y = (long)target.Y,
                Timestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds()
            });

            //If navigation is specified, wait for navigation.
            if (IsNavigation)
                await worker.DevTools.WaitForNextNavigation();
        }
    }
}
