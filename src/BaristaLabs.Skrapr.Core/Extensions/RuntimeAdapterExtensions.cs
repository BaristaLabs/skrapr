namespace BaristaLabs.Skrapr.Extensions
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Threading;
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
        public static async Task<Runtime.RemoteObject> Evaluate(this Runtime.RuntimeAdapter runtimeAdapter, string script, long? contextId = null, bool returnByValue = false, bool awaitPromise = false, bool silent = false, bool userGesture = true, CancellationToken cancellationToken = default(CancellationToken))
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
            }, cancellationToken: cancellationToken);

            if (evaluateResponse.ExceptionDetails != null)
                throw new JavaScriptException(evaluateResponse.ExceptionDetails);

            return evaluateResponse.Result;
        }

        /// <summary>
        /// Evaluates an expression that returns true or false.
        /// </summary>
        public static async Task<bool> EvaluateCondition(this Runtime.RuntimeAdapter runtimeAdapter, string condition, long? contextId = null, CancellationToken cancellationToken = default(CancellationToken))
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
            }, cancellationToken: cancellationToken);

            if (evaluateResponse.ExceptionDetails != null)
                throw new JavaScriptException(evaluateResponse.ExceptionDetails);

            if (evaluateResponse.Result.Type == "boolean" && evaluateResponse.Result.Value is bool)
            {
                return (bool)evaluateResponse.Result.Value;
            }

            throw new InvalidOperationException($"Unexpected response from condition evaluation: {evaluateResponse.Result.Type}");
        }


        /// <summary>
        /// Returns an object that contains the css page dimensions as returned by the JavaScript object model.
        /// </summary>
        /// <returns></returns>
        public static async Task<PageDimensions> GetReportedPageDimensions(this Runtime.RuntimeAdapter runtimeAdapter, long? contextId = null)
        {
            var result = await runtimeAdapter.Session.Runtime.Evaluate(@"
(function() {
    'use strict';

    var max = function (nums) {
        return Math.max.apply(Math, nums.filter(function(x) { return x; }));
    };

    var widths = [
        document.documentElement.clientWidth,
        document.body.scrollWidth,
        document.documentElement.scrollWidth,
        document.body.offsetWidth,
        document.documentElement.offsetWidth
    ];
    var heights = [
        document.documentElement.clientHeight,
        document.body.scrollHeight,
        document.documentElement.scrollHeight,
        document.body.offsetHeight,
        document.documentElement.offsetHeight
    ];

    var result = {
        scrollX: window.scrollX,
        scrollY: window.scrollY,
        fullWidth: max(widths),
        fullHeight: max(heights),
        windowWidth: window.innerWidth,
        windowHeight: window.innerHeight,
        devicePixelRatio: window.devicePixelRatio,
        originalOverflowStyle: document.documentElement.style.overflow
    };

    return JSON.stringify(result);
})();
", contextId: contextId);

            var resultObject = JObject.Parse(result.Value as string);
            return resultObject.ToObject<PageDimensions>();
        }

    }
}
