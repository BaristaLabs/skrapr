namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using System.Threading.Tasks;
    using System;
    using Newtonsoft.Json;
    using System.ComponentModel;

    /// <summary>
    /// Represents a task that ensures that jQuery is present on the page.
    /// </summary>
    /// <remarks>
    /// If any version of jQuery is present, jQuery will not be injected. To override this behavior, set
    /// the condition property to be "window['jQuery'] === undefined && jQuery().jquery === '3.2.1"
    /// </remarks>
    public class EnsureJQueryTask : SkraprTask, IConditionalExpressionTask
    {
        private const string JQueryCDNUrl = "https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js";

        public override string Name
        {
            get { return "EnsureJQuery"; }
        }

        public string Condition
        {
            get;
        }

        /// <summary>
        /// Gets or sets the url to jQuery. (Optional)
        /// </summary>
        [JsonProperty("jQueryUrl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(JQueryCDNUrl)]
        public string JQueryUrl
        {
            get;
            set;
        }

        /// <summary>
        /// When used in conjunction with removeAll = true, indiates the variable that jQuery will be assigned to.
        /// </summary>
        public string GlobalName
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates if the injected jQuery's control of the $ variable should be relinquished.
        /// </summary>
        public bool NoConflict
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether to remove all jQuery variables from the global scope (including jQuery itself).
        /// </summary>
        public bool RemoveAll
        {
            get;
            set;
        }

        public EnsureJQueryTask()
        {
            Condition = "window['jQuery'] === undefined";
            JQueryUrl = JQueryCDNUrl;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            await worker.DevTools.InjectScriptElement(JQueryUrl);
            if (NoConflict)
            {
                if (RemoveAll == true && !String.IsNullOrWhiteSpace(GlobalName))
                {
                    await worker.Session.Runtime.Evaluate($@"var {GlobalName} = jQuery.noConflict(true);");
                }
                else
                {
                    var strRemoveAll = RemoveAll ? "true" : "false";
                    await worker.Session.Runtime.Evaluate($@"jQuery.noConflict({{strRemoveAll}});");
                }
            }
        }
    }
}
