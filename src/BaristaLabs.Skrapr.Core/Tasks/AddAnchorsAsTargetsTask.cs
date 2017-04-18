﻿namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that can be used to add a number of anchors as targets.
    /// </summary>
    public class AddAnchorsAsTargetsTask : SkraprTask
    {
        public override string Name
        {
            get { return "AddAnchorsAsTargets"; }
        }

        /// <summary>
        /// Gets or sets a Css Selector that is used to determine the anchor nodes to add.
        /// </summary>
        public string Selector
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the rule that the target will be added to.
        /// </summary>
        public string Rule
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var documentNode = await worker.Session.DOM.GetDocument(1);
            var selectorResponse = await worker.Session.DOM.QuerySelectorAll(new ChromeDevTools.DOM.QuerySelectorAllCommand
            {
                NodeId = documentNode.NodeId,
                Selector = Selector
            });

            foreach(var nodeId in selectorResponse.NodeIds)
            {
                if (worker.DevTools.ChildNodes.ContainsKey(nodeId))
                {
                    var node = worker.DevTools.ChildNodes[nodeId];
                    var attributes = node.GetAttributes();
                    if (attributes.ContainsKey("href"))
                    {
                        worker.AddTarget(new SkraprTarget(attributes["href"], Rule));
                        worker.Logger.LogDebug("{taskName} Added {href} to the target queue. Rule: {rule}", Name, attributes["href"], Rule);
                    }
                }
                else
                {
                    worker.Logger.LogError("{taskName} Expected that a node with an id of {id} would be in the child node dictionary.", Name, nodeId);
                }
            }
        }
    }
}
