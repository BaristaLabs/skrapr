namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using BaristaLabs.Skrapr.Converters;
    using HandlebarsDotNet;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Represents a task that contains one or more handlebars-based templates that are populated with data based on the selector
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

                var node = worker.DevTools.ChildNodes[nodeId];
                var attributes = node.GetAttributes();

                //Add some additional metadata that templates can use.
                attributes.Add("$index", currentIndex.ToString());
                attributes.Add("$oneBasedindex", (currentIndex + 1).ToString());
                attributes.Add("$zeroBasedindex", (currentIndex).ToString());

                var subTasksString = template(attributes);

                try
                {
                    var subTasks = JsonConvert.DeserializeObject<IList<ISkraprTask>>(subTasksString, jsonSerializerSettings);
                    nodeTasks.Add(new Tuple<long, IList<ISkraprTask>>(nodeId, subTasks));
                }
                catch(JsonSerializationException ex)
                {
                    worker.Logger.LogError("{taskName} Encountered an error when deserializing tasks for nodeId {nodeId}: {ex}", Name, nodeId, ex);
                }
                currentIndex++;
            }

            if (Shuffle)
            {
                nodeTasks.Shuffle();
            }

            //Execute tasks for each node.
            foreach(var nodeTask in nodeTasks)
            {
                worker.Logger.LogDebug("{taskName} Started processing subtasks for nodeId {nodeId}", Name, nodeTask.Item1);

                foreach(var task in nodeTask.Item2)
                {
                    await worker.ProcessSkraprTask(task);
                }

                worker.Logger.LogDebug("{taskName} Completed processing subtasks for nodeId {nodeId}", Name, nodeTask.Item1);
            }
        }
    }
}
