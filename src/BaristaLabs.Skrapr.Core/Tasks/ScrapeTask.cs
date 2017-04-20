namespace BaristaLabs.Skrapr.Tasks
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a task that evaluates a javascript expression and submits the output to the datastore, optionally validatating it.
    /// </summary>
    public class ScrapeTask : SkraprTask
    {
        public override string Name
        {
            get { return "ScrapeTask"; }
        }

        /// <summary>
        /// Gets a validator that will be evaluated prior to scraping.
        /// </summary>
        public string PreValidate
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
        public string Gather
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
        public IList<string> Resources
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            throw new NotImplementedException();
        }
    }
}
