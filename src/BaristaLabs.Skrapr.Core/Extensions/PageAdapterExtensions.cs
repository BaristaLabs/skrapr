namespace BaristaLabs.Skrapr.Extensions
{
    using System;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;
    using Page = ChromeDevTools.Page;

    public static class PageAdapterExtensions
    {
        /// <summary>
        /// Returns the present frame / resource tree structure.
        /// </summary>
        /// <returns></returns>
        public static async Task<Page.FrameResourceTree> GetResourceTree(this Page.PageAdapter pageAdapter)
        {
            var getFramesResponse = await pageAdapter.Session.SendCommand<Page.GetResourceTreeCommand, Page.GetResourceTreeCommandResponse>(new Page.GetResourceTreeCommand());
            return getFramesResponse.FrameTree;
        }

        public static async Task<Page.GetLayoutMetricsCommandResponse> GetLayoutMetrics(this Page.PageAdapter pageAdapter)
        {
            return await pageAdapter.Session.Page.GetLayoutMetrics(new Page.GetLayoutMetricsCommand
            {
            });
        }

        /// <summary>
        /// Returns an object that contains the css page dimensions comparing the BoxModel to the LayoutMetrics
        /// </summary>
        /// <remarks>
        /// This forces a layout and node ids obtained before this may not be valid.
        /// </remarks>
        /// <returns></returns>
        public static async Task<PageDimensions> GetPageDimensions(this Page.PageAdapter pageAdapter, Dom.GetBoxModelCommandResponse documentBoxModel = null, Page.GetLayoutMetricsCommandResponse layoutMetrics = null, long? documentNodeId = null)
        {
            if (documentBoxModel == null || layoutMetrics == null)
            {
                if (documentNodeId.HasValue == false)
                {
                    var documentNode = await pageAdapter.Session.DOM.GetDocument(1);
                    documentNodeId = documentNode.NodeId;
                }

                if (documentBoxModel == null)
                {
                    documentBoxModel = await pageAdapter.Session.DOM.GetBoxModel(new Dom.GetBoxModelCommand
                    {
                        NodeId = documentNodeId.Value
                    });
                }

                if (layoutMetrics == null)
                {
                    layoutMetrics = await pageAdapter.Session.Page.GetLayoutMetrics();
                }
            }

            double scaleX = layoutMetrics.LayoutViewport.ClientWidth / documentBoxModel.Model.Width;
            double scaleY = layoutMetrics.LayoutViewport.ClientHeight / documentBoxModel.Model.Height;
            if (scaleX != scaleY)
                throw new InvalidOperationException($"Did not expect a non-proportional scale factor: scaleX:{scaleX} scaleY: {scaleY}");

            return new PageDimensions
            {
                DevicePixelRatio = scaleX,
                FullHeight = (long)Math.Round((layoutMetrics.LayoutViewport.PageY / scaleY) + (layoutMetrics.LayoutViewport.ClientHeight / scaleY)),
                FullWidth = (long)Math.Round((layoutMetrics.LayoutViewport.PageX / scaleX) + (layoutMetrics.LayoutViewport.ClientWidth / scaleX)),
                OriginalOverflowStyle = "",
                ScrollX = (long)layoutMetrics.VisualViewport.PageX,
                ScrollY = (long)layoutMetrics.VisualViewport.PageY,
                WindowHeight = (long)(layoutMetrics.VisualViewport.ClientHeight / scaleY),
                WindowWidth = (long)(layoutMetrics.VisualViewport.ClientWidth / scaleY)
            };
        }
    }
}
