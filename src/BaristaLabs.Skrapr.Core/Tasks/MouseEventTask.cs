namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System;
    using System.Threading.Tasks;
    using Input = ChromeDevTools.Input;

    public class MouseEventTask : ITask
    {
        public MouseEventTask()
        {
            Type = "mouseMoved";
        }

        public string Name
        {
            get { return "MouseEvent"; }
        }

        public string Selector
        {
            get;
            set;
        }

        /// <summary>
        /// mouseMoved, mousePressed, mouseReleased
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// none, left, middle, right
        /// </summary>
        public string Button
        {
            get;
            set;
        }

        public async Task PerformTask(SkraprContext context)
        {
            var layoutTreeNode = await context.DevTools.GetLayoutTreeNodeForDomElement(Selector);
            if (layoutTreeNode == null)
                return;

            var toGo = layoutTreeNode.BoundingBox.GetMiddleOfRect();

            var response = await context.Session.SendCommand<Input.DispatchMouseEventCommand, Input.DispatchMouseEventCommandResponse>(new Input.DispatchMouseEventCommand
            {
                Button = "left",
                Type = "mousePressed",
                ClickCount = 0,
                Modifiers = 0,
                X = (long)toGo.X,
                Y = (long)toGo.Y,
                Timestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds()
            });

            response = await context.Session.SendCommand<Input.DispatchMouseEventCommand, Input.DispatchMouseEventCommandResponse>(new Input.DispatchMouseEventCommand
            {
                Button = "left",
                Type = "mouseReleased",
                ClickCount = 0,
                Modifiers = 0,
                X = (long)toGo.X,
                Y = (long)toGo.Y,
                Timestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds()
            });
        }
    }
}
