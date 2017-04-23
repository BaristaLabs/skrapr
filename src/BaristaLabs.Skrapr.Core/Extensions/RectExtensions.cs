namespace BaristaLabs.Skrapr.Extensions
{
    using BaristaLabs.Skrapr.Utilities;
    using System;
    using Dom = ChromeDevTools.DOM;

    public static class RectExtensions
    {
        public static Point GetMiddleOfRect(this Dom.Rect rect)
        {
            return new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
        }

        public static Point GetRandomSpotWithinRect(this Dom.Rect rect, int border = 2)
        {
            if (border < 0)
                border = Math.Abs(border);

            if (rect.Width <= border * 2 || rect.Height <= border * 2)
                throw new ArgumentOutOfRangeException(nameof(border));

            if (border == 0)
            {
                return new Point(RandomUtils.Random.NextDouble(rect.X, rect.X + rect.Width), RandomUtils.Random.NextDouble(rect.Y, rect.Y + rect.Height));
            }

            return GetRandomSpotWithinRect(new Dom.Rect
            {
                X = rect.X + border,
                Y = rect.Y + border,
                Width = rect.Width - (border * 2),
                Height = rect.Height - (border * 2)
            }, 0);
        }
    }
}
