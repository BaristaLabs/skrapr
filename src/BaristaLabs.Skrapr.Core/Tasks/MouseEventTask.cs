namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System;
    using System.Threading.Tasks;
    using Input = ChromeDevTools.Input;
    using Css = ChromeDevTools.CSS;
    using Dom = ChromeDevTools.DOM;
    using Page = ChromeDevTools.Page;
    using System.Linq;

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
            await context.Session.DOM.GetDocument(-1);

            var nodeId = await context.DevTools.GetNodeForSelector(Selector);
            if (nodeId < 1)
                return;

            if (context.DevTools.ChildNodes.ContainsKey(nodeId))
            {
                var backendNodeInfo = context.DevTools.ChildNodes[nodeId].BackendNodeId;
                //await context.Session.Runtime.CallFunctionOn(new ChromeDevTools.Runtime.CallFunctionOnCommand
                //{
                //    ObjectId = backendNodeInfo.
                //});
            }

            await context.Session.DOM.SetInspectedNode(new Dom.SetInspectedNodeCommand
            {
                NodeId = nodeId
            });

            var result = await context.DevTools.EvaluateScript("JSON.stringify($0.getBoundingClientRect())");
            var nodeBoxModel = await context.Session.DOM.GetBoxModel(nodeId);

            var highlightObject = await context.Session.DOM.GetHighlightObjectForTest(nodeId);
            var contentPath = highlightObject.Paths.FirstOrDefault(p => p.Name == "content");
            var contentPathPoints = contentPath.GetQuad();

            var scaleFactor = await context.DevTools.GetPageScaleFactor();
            //await context.Session.DOM.HighlightRect(new Dom.HighlightRectCommand
            //{
            //    X = layoutMetrics.LayoutViewport.PageX,
            //    Y = layoutMetrics.LayoutViewport.PageY,
            //    Width = layoutMetrics.LayoutViewport.ClientWidth,
            //    Height = layoutMetrics.LayoutViewport.ClientHeight,
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

            await context.Session.DOM.HighlightRect(new Dom.HighlightRectCommand
            {
                X = (long)contentPathPoints[0]/2,
                Y = (long)contentPathPoints[1]/2,
                Width = (long)(highlightObject.ElementInfo.NodeWidth / 2),
                Height = (long)(highlightObject.ElementInfo.NodeHeight / 2.2),
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
                },
            });

            //await context.Session.DOM.HighlightQuad(new Dom.HighlightQuadCommand
            //{
            //    Quad = contentPath.GetQuad(),
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
