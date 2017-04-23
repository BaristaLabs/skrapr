namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.Utilities;
    using System;

    public class BoundingClientRect
    {
        public double Top
        {
            get;
            set;
        }

        public double Right
        {
            get;
            set;
        }

        public double Bottom
        {
            get;
            set;
        }

        public double Left
        {
            get;
            set;
        }

        public double Width
        {
            get;
            set;
        }

        public double Height
        {
            get;
            set;
        }

        public Point GetRandomSpotWithinBox(int border = 2)
        {
            if (border < 0)
                border = Math.Abs(border);

            if (Width <= border * 2 || Height <= border * 2)
                throw new ArgumentOutOfRangeException(nameof(border));

            if (border == 0)
            {
                return new Point(RandomUtils.Random.NextDouble(Left, Right), RandomUtils.Random.NextDouble(Top, Bottom));
            }

            var boundingBorder = new BoundingClientRect
            {
                Left = Left + border,
                Right = Right - border,
                Top = Top + border,
                Bottom = Bottom - border,
                Width = Width - (border * 2),
                Height = Height - (border * 2)
            };

            return boundingBorder.GetRandomSpotWithinBox(border = 0);
        }

        public Tuple<double, double> GetOnscreenDelta(PageDimensions pageDimensions)
        {
            var currentViewPort = new BoundingClientRect
            {
                Top = pageDimensions.ScrollY,
                Bottom = pageDimensions.ScrollY + pageDimensions.WindowHeight,
                Left = pageDimensions.ScrollX,
                Right = pageDimensions.ScrollX + pageDimensions.WindowWidth,
                Width = pageDimensions.WindowWidth,
                Height = pageDimensions.WindowHeight
            };

            return GetDelta(currentViewPort);
        }

        public Tuple<double, double> GetDelta(BoundingClientRect targetRect)
        {
            double deltaX = 0, deltaY = 0;

            if (targetRect.Top < Top && targetRect.Bottom > Bottom)
                deltaY = 0;
            else
            {
                //Negative values scroll down.
                if (targetRect.Bottom < Bottom)
                {
                    deltaY = targetRect.Bottom - Bottom;

                    // Add a random amount between the target top and the top of the screen
                    deltaY += RandomUtils.Random.NextDouble(targetRect.Top - Bottom, 0);
                    deltaY = Math.Ceiling(deltaY);
                }


                //Positive values scroll up.
                if (targetRect.Top > Top)
                {
                    deltaY = targetRect.Top - Top;

                    deltaY  += RandomUtils.Random.NextDouble(0, targetRect.Bottom - Top);
                    deltaY = Math.Floor(deltaY);
                }
            }

            if (targetRect.Left < Left && targetRect.Right > Right)
                deltaX = 0;
            else
            {
                //Negative values to scroll right.
                if (targetRect.Right < Right)
                { 
                    deltaX = Right - targetRect.Right;

                    deltaX += RandomUtils.Random.NextDouble(targetRect.Left - Top, 0);
                    deltaX = Math.Ceiling(deltaX);
                }

                //Positive values to scroll left.
                if (targetRect.Left > Left)
                {
                    deltaX = Left - targetRect.Left;

                    deltaX += RandomUtils.Random.NextDouble(0, targetRect.Right - Left);
                    deltaX = Math.Floor(deltaX);
                }
            }

            return new Tuple<double, double>(deltaX, deltaY);
        }
    }
}
