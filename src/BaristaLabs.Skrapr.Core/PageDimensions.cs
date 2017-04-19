namespace BaristaLabs.Skrapr
{
    /// <summary>
    /// Represents a class that contains the current dimensions of the page.
    /// </summary>
    public class PageDimensions
    {
        public long ScrollX
        {
            get;
            set;
        }

        public long ScrollY
        {
            get;
            set;
        }

        public long FullWidth
        {
            get;
            set;
        }

        public long FullHeight
        {
            get;
            set;
        }

        public long WindowWidth
        {
            get;
            set;
        }

        public long WindowHeight
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
