namespace BaristaLabs.Skrapr.Extensions
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;

    /// <summary>
    /// Contains common helper methods for the DOM Adapter.
    /// </summary>
    public static class DOMAdapterExtensions
    {
        /// <summary>
        /// Returns the box model for the given node id.
        /// </summary>
        /// <param name="domAdapter"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public static async Task<Dom.BoxModel> GetBoxModel(this Dom.DOMAdapter domAdapter, long nodeId)
        {
            var result = await domAdapter.Session.DOM.GetBoxModel(new Dom.GetBoxModelCommand
            {
                NodeId = nodeId
            });

            if (result == null)
                return null;

            return result.Model;
        }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <param name="domAdapter"></param>
        /// <param name="depth"></param>
        /// <param name="pierce"></param>
        /// <returns></returns>
        public static async Task<Dom.Node> GetDocument(this Dom.DOMAdapter domAdapter, long depth = 1, bool pierce = false)
        {
            var response = await domAdapter.Session.SendCommand<Dom.GetDocumentCommand, Dom.GetDocumentCommandResponse>(new Dom.GetDocumentCommand
            {
                Depth = depth,
                Pierce = pierce
            });

            return response.Root;
        }

        public static async Task<HighlightObject> GetHighlightObjectForTest(this Dom.DOMAdapter domAdapter, long nodeId)
        {
            var result = await domAdapter.Session.SendCommand<Dom.GetHighlightObjectForTestCommand, Dom.GetHighlightObjectForTestCommandResponse>(new Dom.GetHighlightObjectForTestCommand
            {
                NodeId = nodeId
            });

            var data = result.Highlight as JObject;
            if (data == null)
                throw new InvalidOperationException($"No Highlight Object was returned for nodeId {nodeId}");

            return data.ToObject<HighlightObject>();
        }

        /// <summary>
        /// Returns the node id for the given css selector. Value will be less than 1 if selector does not correspond to a dom element.
        /// </summary>
        /// <param name="cssSelector"></param>
        /// <returns></returns>
        public static async Task<long> GetNodeIdForSelector(this Dom.DOMAdapter domAdapter, string cssSelector)
        {
            var document = await domAdapter.Session.DOM.GetDocument();
            var domElement = await domAdapter.Session.DOM.QuerySelector(new Dom.QuerySelectorCommand
            {
                NodeId = document.NodeId,
                Selector = cssSelector
            });

            return domElement.NodeId;
        }
    }

    public class HighlightObject
    {
        [JsonProperty("displayAsMaterial")]
        public bool DisplayAsMaterial
        {
            get;
            set;
        }

        [JsonProperty("elementInfo")]
        public HighlightObjectElementInfo ElementInfo
        {
            get;
            set;
        }

        [JsonProperty("paths")]
        public IList<HighlightObjectPath> Paths
        {
            get;
            set;
        }

        [JsonProperty("showRulers")]
        public bool ShowRulers
        {
            get;
            set;
        }

        [JsonProperty("showExtensionLines")]
        public bool ShowExtensionLines
        {
            get;
            set;
        }
    }

    public class HighlightObjectElementInfo
    {
        [JsonProperty("tagName")]
        public string TagName
        {
            get;
            set;
        }

        [JsonProperty("idValue")]
        public string IdValue
        {
            get;
            set;
        }

        [JsonProperty("className")]
        public string ClassName
        {
            get;
            set;
        }

        [JsonProperty("nodeWidth")]
        public double NodeWidth
        {
            get;
            set;
        }

        [JsonProperty("nodeHeight")]
        public double NodeHeight
        {
            get;
            set;
        }
    }

    public class HighlightObjectPath
    {
        [JsonProperty("fillColor")]
        public string FillColor
        {
            get;
            set;
        }

        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty("outlineColor")]
        public string OutlineColor
        {
            get;
            set;
        }

        [JsonProperty("path")]
        public string[] Path
        {
            get;
            set;
        }

        public double[] GetQuad()
        {
            var result = new List<double>();

            foreach(var item in Path)
            {
                //Ignore the "M/L/Z" tokens for now..?
                if (double.TryParse(item, out double itemDouble))
                {
                    result.Add(itemDouble);
                }
            }

            return result.ToArray();
        }

        public long[] GetLong()
        {
            var result = new List<long>();

            foreach (var item in Path)
            {
                //Ignore the "M/L/Z" tokens for now..?
                if (long.TryParse(item, out long itemLong))
                {
                    result.Add(itemLong);
                }
            }

            return result.ToArray();
        }
    }
}
