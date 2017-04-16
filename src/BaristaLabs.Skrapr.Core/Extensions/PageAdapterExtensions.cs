namespace BaristaLabs.Skrapr.Extensions
{
    using System.Threading.Tasks;
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
    }
}
