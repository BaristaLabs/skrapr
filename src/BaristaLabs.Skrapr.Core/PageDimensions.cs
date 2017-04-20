namespace BaristaLabs.Skrapr
{
    /// <summary>
    /// Represents a class that contains the current dimensions of the page.
    /// </summary>
    public class PageDimensions
    {
        public double ScrollX
        {
            get;
            set;
        }

        public double ScrollY
        {
            get;
            set;
        }

        public double FullWidth
        {
            get;
            set;
        }

        public double FullHeight
        {
            get;
            set;
        }

        public double WindowWidth
        {
            get;
            set;
        }

        public double WindowHeight
        {
            get;
            set;
        }

        public double DevicePixelRatio
        {
            get;
            set;
        }

        public string OriginalOverflowStyle
        {
            get;
            set;
        }
    }
}
