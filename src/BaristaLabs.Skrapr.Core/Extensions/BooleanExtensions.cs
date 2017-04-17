namespace BaristaLabs.Skrapr.Extensions
{
    public static class BooleanExtensions
    {
        public static string GetJSValue(this bool b)
        {
            if (b)
                return "true";
            return "false";
        }
    }
}
