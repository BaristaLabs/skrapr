namespace BaristaLabs.Skrapr.Extensions
{
    using System.Threading.Tasks;
    using Runtime = ChromeDevTools.Runtime;

    public static class RuntimeAdapterExtensions
    {

        /// <summary>
        /// Evaluates the specified script expression on global object.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="isPromise"></param>
        /// <returns></returns>
        public static async Task<Runtime.RemoteObject> Evaluate(this Runtime.RuntimeAdapter runtimeAdapter, string script, long? contextId = null, bool returnByValue = false, bool awaitPromise = false)
        {
            var evaluateResponse = await runtimeAdapter.Session.SendCommand<Runtime.EvaluateCommand, Runtime.EvaluateCommandResponse>(new Runtime.EvaluateCommand
            {
                AwaitPromise = awaitPromise,
                ContextId = contextId,
                Expression = script,
                GeneratePreview = false,
                IncludeCommandLineAPI = true,
                ObjectGroup = "Skrapr",
                ReturnByValue = returnByValue,
                Silent = false,
                UserGesture = true
            });

            return evaluateResponse.Result;
        }
    }
}
