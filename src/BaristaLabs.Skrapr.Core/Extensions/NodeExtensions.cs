namespace BaristaLabs.Skrapr.Extensions
{
    using System;
    using System.Collections.Generic;
    using Dom = ChromeDevTools.DOM;

    public static class NodeExtensions
    {
        /// <summary>
        /// Returns the attributes in the node as a dictionary object.
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, string> GetAttributes(this Dom.Node node)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (node == null || node.Attributes == null || node.Attributes.Length == 0)
                return result;

            if (node.Attributes.Length % 2 != 0)
                throw new InvalidOperationException("Expected an even number of attributes.");

            for(int i = 0; i < node.Attributes.Length - 1; i += 2)
            {
                result.Add(node.Attributes[i], node.Attributes[i + 1]);
            }
            return result;
        }
    }
}
