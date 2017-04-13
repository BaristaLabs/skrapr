namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.ChromeDevTools;

    public class SkraprContext
    {
        public SkraprDevTools DevTools
        {
            get;
            set;
        }
        public ChromeSession Session
        {
            get;
            set;
        }
    }
}
