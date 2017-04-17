namespace BaristaLabs.Skrapr.Extensions
{
    using System;
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
        public static async Task<Runtime.RemoteObject> Evaluate(this Runtime.RuntimeAdapter runtimeAdapter, string script, long? contextId = null, bool returnByValue = false, bool awaitPromise = false, bool silent = false, bool userGesture = true)
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
                Silent = silent,
                UserGesture = userGesture
            });

            if (evaluateResponse.ExceptionDetails != null)
                throw new JavaScriptException(evaluateResponse.ExceptionDetails);

            return evaluateResponse.Result;
        }

        /// <summary>
        /// Evaluates an expression that returns true or false.
        /// </summary>
        public static async Task<bool> EvaluateCondition(this Runtime.RuntimeAdapter runtimeAdapter, string condition, long? contextId = null)
        {

            var evaluateResponse = await runtimeAdapter.Session.SendCommand<Runtime.EvaluateCommand, Runtime.EvaluateCommandResponse>(new Runtime.EvaluateCommand
            {
                AwaitPromise = false,
                ContextId = contextId,
                Expression = $@"
!!((function() {{
    return {condition}
}})());
",
                GeneratePreview = false,
                IncludeCommandLineAPI = true,
                ObjectGroup = "Skrapr",
                ReturnByValue = false,
                Silent = true,
                UserGesture = false,
            });

            if (evaluateResponse.ExceptionDetails != null)
                throw new JavaScriptException(evaluateResponse.ExceptionDetails);

            if (evaluateResponse.Result.Type == "boolean" && evaluateResponse.Result.Value is bool)
            {
                return (bool)evaluateResponse.Result.Value;
            }

            throw new InvalidOperationException($"Unexpected response from condition evaluation: {evaluateResponse.Result.Type}");
        }
    }
}
