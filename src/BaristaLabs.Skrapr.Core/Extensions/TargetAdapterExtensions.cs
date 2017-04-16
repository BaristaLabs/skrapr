namespace BaristaLabs.Skrapr.Extensions
{
    using System.Threading.Tasks;
    using Target = ChromeDevTools.Target;

    public static class TargetAdapterExtensions
    {
        public static async Task<Target.TargetInfo> GetTargetInfo(this Target.TargetAdapter targetAdapter, string targetId)
        {
            var targetInfoResponse = await targetAdapter.GetTargetInfo(new Target.GetTargetInfoCommand
            {
                TargetId = targetId
            });

            return targetInfoResponse.TargetInfo;
        }
    }
}
