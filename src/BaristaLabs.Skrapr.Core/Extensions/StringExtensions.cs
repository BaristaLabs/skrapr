namespace BaristaLabs.Skrapr.Extensions
{
    using System;

    public static class StringExtensions
    {
        public static string GetJSValue(this string str, string startEndTag = "'")
        {
            if (String.IsNullOrWhiteSpace(str))
                return "undefined";
            return "'" + str.Replace("'", "''") + "'";
        }
    }
}
