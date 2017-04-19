namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Converters;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a task that contains one or more handlebars-based templates that are populated with data based on the results of a script
    /// </summary>
    /// <remarks>
    /// For instance, given a script result of [{ 'href': 'example.com' }] and a template [{ name: "navigate", url: "{{href}}" }]
    /// on a page that has 3 anchor tags present, three navigate tasks will be created with the properties
    /// of the objects populating the templates and then sequentially run.
    /// </remarks>
    public class TemplatedScriptedSubFlowTask : SkraprTask
    {
        public override string Name
        {
            get { return "TemplatedScriptedSubFlow"; }
        }

        public JArray TaskTemplates
        {
            get;
            set;
        }

        public override Task PerformTask(ISkraprWorker worker)
        {
            throw new NotImplementedException();
        }
    }
}
