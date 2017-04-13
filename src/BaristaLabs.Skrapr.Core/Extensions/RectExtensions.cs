namespace BaristaLabs.Skrapr.Extensions
{
    using Dom = ChromeDevTools.DOM;

    public static class RectExtensions
    {
        public static Point GetMiddleOfRect(this Dom.Rect rect)
        {
            return new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
        }
    }
}
