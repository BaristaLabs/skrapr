namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Network = ChromeDevTools.Network;
    using Runtime = ChromeDevTools.Runtime;

    /// <summary>
    /// Represents a task that evaluates a javascript expression and submits the output to the datastore, optionally validatating it.
    /// </summary>
    public class ScrapeTask : SkraprTask, IConditionalExpressionTask
    {
        public override string Name
        {
            get { return "Scrape"; }
        }

        /// <summary>
        /// Gets or sets an optional expression that will be evaluated to determine if the current page will be scraped.
        /// </summary>
        /// <remarks>
        /// If the condition is truthy, the page will be scraped.
        /// </remarks>
        public string Condition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the JavaScript that will be evaluated which gathers data from the current page.
        /// </summary>
        /// <remarks>
        /// Gathered data must contain an _id id
        /// </remarks>
        public JObject Gather
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a Json Schema that the scraped data will be validated against.
        /// </summary>
        public JObject Schema
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of resource url patterns that will be stored with the data.
        /// </summary>
        public JObject Resources
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            if (Gather == null)
                throw new InvalidOperationException("Gather must be specified on a Scrape task.");

            var result = await GetDataObject(worker);

            //worker.Session.run
            //TODO: Change this to use a repository.
            var id = result["_id"].ToString();
            if (!String.IsNullOrWhiteSpace(id))
            {
                Directory.CreateDirectory($"./data/{id}");
                File.WriteAllText($"./data/{id}/default.json", result.ToString());

                foreach (var key in worker.DevTools.Responses.Keys)
                {
                    if (Regex.IsMatch(key, @"\.jpg$", RegexOptions.IgnoreCase))
                    {
                        var responseBody = await worker.Session.Network.GetResponseBody(new Network.GetResponseBodyCommand
                        {
                            RequestId = worker.DevTools.Responses[key].RequestId
                        });

                        var fileName = Path.GetFileName(key);
                        if (responseBody.Base64Encoded)
                        {
                            var imageBytes = Convert.FromBase64String(responseBody.Body);
                            using (var fileStream = File.Open($"./data/{id}/{fileName}", FileMode.Create))
                            {
                                fileStream.Seek(0, SeekOrigin.End);
                                await fileStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                            }
                        }
                    }
                }
            }
        }

        private async Task<JObject> GetDataObject(ISkraprWorker worker)
        {
            var result = new JObject();
            foreach (var gatherProperty in Gather.Properties())
            {
                var propertyName = gatherProperty.Name;

                bool awaitPromise = false;
                string gatherScript = null;
                switch (gatherProperty.Value.Type)
                {
                    case JTokenType.String:
                        gatherScript = gatherProperty.Value.Value<string>();
                        break;
                    case JTokenType.Object:
                        //TODO: Implement this -- objects can specify additional settings.
                        throw new NotImplementedException("Objects are not yet supported.");
                    default:
                        throw new InvalidOperationException($"Unknown or invalid value for gather property {propertyName}: {gatherProperty.Value.Type}");
                }

                if (String.IsNullOrWhiteSpace(gatherScript))
                    throw new InvalidOperationException($"A gather script must be specified for property {propertyName}");

                var fnGatherScript = $@"(function() {{
    'use strict';
    var result = {gatherScript}
    return JSON.stringify({{ result }});
}})();";
                var evaluateResponse = await worker.Session.Runtime.Evaluate(new Runtime.EvaluateCommand
                {
                    AwaitPromise = awaitPromise,
                    Expression = fnGatherScript,
                    IncludeCommandLineAPI = true,
                    ContextId = worker.DevTools.CurrentFrameContext.Id,
                    ObjectGroup = "Skrapr"
                });

                if (evaluateResponse.Result.Subtype == "error")
                    throw new InvalidOperationException($"An error occurred while evaluating script on property '{propertyName}':");

                var strResult = evaluateResponse.Result.Value as string;
                if (strResult == null)
                    throw new InvalidOperationException($"Unable to obtain the gather result for property {propertyName}");

                var objResult = JToken.Parse(strResult);
                result.Add(propertyName, objResult["result"]);
            }

            return result;
        }
    }
}
