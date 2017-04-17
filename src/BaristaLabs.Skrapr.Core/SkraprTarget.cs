namespace BaristaLabs.Skrapr
{
    using System;

    /// <summary>
    /// Represents a target for a Skraping activity.
    /// </summary>
    public class SkraprTarget
    {
        public string Url
        {
            get;
            set;
        }

        public string Rule
        {
            get;
            set;
        }

        public SkraprTarget(string url, string rule)
        {
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            Url = url;
            Rule = rule;
        }
    }
}
