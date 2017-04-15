namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System;
    using System.Threading.Tasks;
    using Input = ChromeDevTools.Input;
    using Css = ChromeDevTools.CSS;
    using Dom = ChromeDevTools.DOM;
    using Page = ChromeDevTools.Page;

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
            var nodeId = await context.DevTools.GetNodeForSelector(".logo-placeholder");
            if (nodeId < 1)
                return;

            var viewPort = await context.Session.SendCommand<Page.GetLayoutMetricsCommand, Page.GetLayoutMetricsCommandResponse>(new Page.GetLayoutMetricsCommand
            {

            });

            var nodeBoxModel = await context.Session.SendCommand<Dom.GetBoxModelCommand, Dom.GetBoxModelCommandResponse>(new Dom.GetBoxModelCommand
            {
                NodeId = nodeId
            });

            for(int i = 0; i < nodeBoxModel.Model.Content.Length; i++)
            {
                nodeBoxModel.Model.Content[i] = nodeBoxModel.Model.Content[i] / 2;
            }
            await context.Session.SendCommand(new Dom.HighlightQuadCommand
            {
                Quad = nodeBoxModel.Model.Content,
                Color = new Dom.RGBA
                {
                    R = 0,
                    G = 0,
                    B = 255,
                    A = 0.7
                },
                OutlineColor = new Dom.RGBA
                {
                    R = 255,
                    G = 0,
                    B = 0,
                    A = 1
                }
            });

            //await context.Session.SendCommand(new Dom.HighlightRectCommand
            //{
            //    X = (long)layoutTreeNode.BoundingBox.X,
            //    Y = (long)layoutTreeNode.BoundingBox.Y,
            //    Height = (long)layoutTreeNode.BoundingBox.Height,
            //    Width = (long)layoutTreeNode.BoundingBox.Width,
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
            //    }
            //});

            var computedStyles = await context.Session.SendCommand<Css.GetComputedStyleForNodeCommand, Css.GetComputedStyleForNodeCommandResponse>(new Css.GetComputedStyleForNodeCommand
            {
                NodeId = nodeId
            });

            await context.Session.SendCommand(new Dom.HighlightNodeCommand
            {
                NodeId = nodeId,
                HighlightConfig = new Dom.HighlightConfig
                {
                    ShowInfo = true,
                    ContentColor = new Dom.RGBA
                    {
                        R = 0,
                        G = 0,
                        B = 255,
                        A = 0.7
                    },
                    BorderColor = new Dom.RGBA
                    {
                        R = 255,
                        G = 0,
                        B = 0,
                        A = 1
                    }
                }
            });

            //var response = await context.Session.SendCommand<Input.DispatchMouseEventCommand, Input.DispatchMouseEventCommandResponse>(new Input.DispatchMouseEventCommand
            //{
            //    Button = "left",
            //    Type = "mousePressed",
            //    ClickCount = 1,
            //    Modifiers = 0,
            //    X = (long)toGo.X,
            //    Y = (long)toGo.Y,
            //    Timestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds()
            //});

            //response = await context.Session.SendCommand<Input.DispatchMouseEventCommand, Input.DispatchMouseEventCommandResponse>(new Input.DispatchMouseEventCommand
            //{
            //    Button = "left",
            //    Type = "mouseReleased",
            //    ClickCount = 1,
            //    Modifiers = 0,
            //    X = (long)toGo.X,
            //    Y = (long)toGo.Y,
            //    Timestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds()
            //});
        }
    }
}
