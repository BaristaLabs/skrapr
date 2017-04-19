namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Troschuetz.Random;
    using Dom = ChromeDevTools.DOM;
    using Input = ChromeDevTools.Input;

    /// <summary>
    /// Represents a task that clicks a specified element on the page by selector.
    /// </summary>
    public class ClickDomElementTask : SkraprTask, IConditionalExpressionTask
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
        /// Gets or sets an optional expression that will be evaluated to determine if the element should be clicked.
        /// </summary>
        /// <remarks>
        /// If the condition is truthy, the element will be clicked.
        /// </remarks>
        public string Condition
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

            //If we're in debug mode, illustrate where we're going to click.
            if (worker.IsDebugEnabled)
            {
                //TODO: Make this better... javascript based even.
                //var scaleFactor = await worker.DevTools.GetPageScaleFactor();
                //var xScaleFactor = scaleFactor.Item1;
                //var yScaleFactor = scaleFactor.Item2;

                //var highlightRect = new Dom.Rect
                //{
                //    X = contentPathPoints[0] / xScaleFactor,
                //    Y = contentPathPoints[1] / yScaleFactor,
                //    Width = highlightObject.ElementInfo.NodeWidth / xScaleFactor,
                //    Height = highlightObject.ElementInfo.NodeHeight / yScaleFactor //2.2
                //};

                //await worker.Session.DOM.HighlightRect(new Dom.HighlightRectCommand
                //{
                //    X = (long)(highlightRect.X),
                //    Y = (long)(highlightRect.Y),
                //    Width = (long)(highlightRect.Width),
                //    Height = (long)(highlightRect.Height),
                //    Color = new Dom.RGBA
                //    {
                //        R = 0,
                //        G = 0,
                //        B = 255,
                //        A = 0.7
                //    },
                //    OutlineColor = new Dom.RGBA
                //    {
                //        R = 255,
                //        G = 0,
                //        B = 0,
                //        A = 1
                //    },
                //});

                //await worker.Session.DOM.HighlightRect(new Dom.HighlightRectCommand
                //{
                //    X = (long)target.X,
                //    Y = (long)target.Y,
                //    Width = 1,
                //    Height = 1,
                //    Color = new Dom.RGBA
                //    {
                //        R = 255,
                //        G = 255,
                //        B = 0,
                //        A = 1
                //    },
                //    OutlineColor = new Dom.RGBA
                //    {
                //        R = 255,
                //        G = 255,
                //        B = 0,
                //        A = 1
                //    },
                //});

                ////Wait 5 seconds.
                //await Task.Delay(5000);
            }

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

            await Task.Delay(clickDelay);

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
