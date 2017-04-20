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
        private static TRandom s_random = TRandom.New(new Troschuetz.Random.Generators.NR3Generator());

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

            if (nodeIds.NodeIds.Length == 0)
            {
                worker.Logger.LogError("{taskName} Did not find any nodes corresponding to selector {selector}, skipping.", Name, Selector);
                return;
            }

            var nodeId = nodeIds.NodeIds.First();
            if (nodeId < 1)
                return;

            //TODO: Position the viewport so the element is on screen if isn't already.

            var targetBoundingBox = await worker.DevTools.GetBoundingClientRect(nodeId);

            //Get a random point within the click area
            var target = targetBoundingBox.GetRandomSpotWithinBox();

            //If we're in debug mode, illustrate where we're going to click.
            if (worker.IsDebugEnabled)
            {
                await worker.Session.DOM.HighlightRect(new Dom.HighlightRectCommand
                {
                    X = (long)target.X / 2,
                    Y = (long)target.Y / 2,
                    Width = 1,
                    Height = 1,
                    Color = new Dom.RGBA
                    {
                        R = 255,
                        G = 255,
                        B = 0,
                        A = 1
                    },
                    OutlineColor = new Dom.RGBA
                    {
                        R = 255,
                        G = 0,
                        B = 0,
                        A = 1
                    },
                });

                //Wait 3 seconds.
                await Task.Delay(3000);

                await worker.Session.DOM.HideHighlight(new Dom.HideHighlightCommand());
            }

            //Click the random point, with a random delay between the down and up mouse events.
            var clickDelay = s_random.Next(100, 1500);

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
