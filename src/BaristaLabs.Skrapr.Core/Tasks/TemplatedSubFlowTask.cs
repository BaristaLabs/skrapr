namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using BaristaLabs.Skrapr.Converters;
    using HandlebarsDotNet;
    using Humanizer;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    /// Represents a task that contains one or more handlebars-based templates that are populated with data from attributes defined on elements matching the selector
    /// </summary>
    /// <remarks>
    /// For instance, given a selector: "a" and taskTemplates: [{ name: "navigate", url: "{{href}}" }]
    /// on a page that has 3 anchor tags present, three navigate tasks will be created with the href
    /// of the anchors populated as the url and then sequentially run.
    /// </remarks>
    public class TemplatedSubFlowTask : SkraprTask
    {
        public override string Name
        {
            get { return "TemplatedSubFlow"; }
        }

        /// <summary>
        /// Gets or sets the selector for the tasks to be generated.
        /// </summary>
        public string Selector
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the templates for tasks that will be generated, one set per matching selector element.
        /// </summary>
        [JsonProperty("taskTemplates", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public JArray TaskTemplates
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates if the selectors should be processed in random order.
        /// </summary>
        public bool Shuffle
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            Handlebars.RegisterHelper("kebaberize", (writer, context, parameters) =>
            {
                if (parameters.Length != 1)
                {
                    throw new HandlebarsException("kebaberize expects exactly one parameter.");
                }

                writer.WriteSafeString(
                parameters[0].ToString()
                    .Underscore()
                    .Dasherize()
                    );
            });

            //Compile the task templates
            var template = Handlebars.Compile(TaskTemplates.ToString());

            //Get the matching selectors
            var documentNode = await worker.Session.DOM.GetDocument(1);
            var selectorResponse = await worker.Session.DOM.QuerySelectorAll(new ChromeDevTools.DOM.QuerySelectorAllCommand
            {
                NodeId = documentNode.NodeId,
                Selector = Selector
            });

            var nodeTasks = new List<Tuple<long, IList<ISkraprTask>>>();

            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new TaskConverter());

            //For each selector, bind the data to the template, get the tasks for each node.
            int currentIndex = 0;
            foreach (var nodeId in selectorResponse.NodeIds)
            {
                if (!worker.DevTools.ChildNodes.ContainsKey(nodeId))
                {
                    worker.Logger.LogError("{taskName} Expected that a node with an id of {id} would be in the child node dictionary.", Name, nodeId);
                    continue;
                }

                var frameState = await worker.DevTools.GetCurrentFrameState();
                var node = worker.DevTools.ChildNodes[nodeId];
                var attributes = node.GetAttributes();

                //Add some additional metadata that templates can use.
                attributes.Add("$index", currentIndex.ToString());
                attributes.Add("$oneBasedindex", (currentIndex + 1).ToString());
                attributes.Add("$zeroBasedindex", (currentIndex).ToString());
                attributes.Add("$title", frameState.Title);
                attributes.Add("$url", frameState.Url);

                var subTasksString = template(attributes);

                try
                {
                    var subTasks = JsonConvert.DeserializeObject<IList<ISkraprTask>>(subTasksString, jsonSerializerSettings);
                    nodeTasks.Add(new Tuple<long, IList<ISkraprTask>>(nodeId, subTasks));
                }
                catch(JsonSerializationException ex)
                {
                    worker.Logger.LogError("{taskName} Encountered an error when deserializing tasks for nodeId {nodeId}: {ex}", Name, nodeId, ex);
                    throw;
                }
                currentIndex++;
            }

            if (Shuffle)
            {
                nodeTasks.Shuffle();
            }

            ActionBlock<Tuple<long, IList<ISkraprTask>>> subTaskFlow = null;

            subTaskFlow = new ActionBlock<Tuple<long, IList<ISkraprTask>>>(async (nodeTask) => await ProcessNodeTask(nodeTask, worker, subTaskFlow), new ExecutionDataflowBlockOptions
            {
                CancellationToken = worker.CancellationToken,
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1
            });

            //Execute tasks for each node.
            foreach(var nodeTask in nodeTasks)
            {
                subTaskFlow.Post(nodeTask);
            }

            worker.Logger.LogDebug("{taskName} Processing {0} nodes in subflow", Name, subTaskFlow.InputCount);

            await subTaskFlow.Completion;

            worker.Logger.LogDebug("{taskName} Completed processing all nodes in subflow.", Name);
        }

        private async Task ProcessNodeTask(Tuple<long, IList<ISkraprTask>> nodeTask, ISkraprWorker worker, ActionBlock<Tuple<long, IList<ISkraprTask>>> subTaskFlow)
        {
            worker.Logger.LogDebug("{taskName} Started processing subtasks for nodeId {nodeId}", Name, nodeTask.Item1);

            try
            {
                foreach (var task in nodeTask.Item2)
                {
                    await worker.ProcessSkraprTask(task);
                }
            }
            catch (Exception ex) when (ex is AssertionFailedException || ex is NavigationFailedException)
            {
                //Add it back into the queue.
                subTaskFlow.Post(nodeTask);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                worker.Logger.LogWarning("{taskName} is terminating due to a cancellation request.", Name);
                throw;
            }
            catch (Exception ex)
            {
                worker.Logger.LogError("{taskName} An unhandled exception occurred processing subtasks for nodeId {nodeId}: {exception}", Name, nodeTask.Item1, ex);
                throw;
            }

            worker.Logger.LogDebug("{taskName} Completed processing subtasks for nodeId {nodeId}", Name, nodeTask.Item1);

            if (subTaskFlow.InputCount == 0)
            {
                subTaskFlow.Complete();
            }

            worker.Logger.LogDebug("{taskName} {count} nodes remaining", Name, subTaskFlow.InputCount);
        }
    }
}
