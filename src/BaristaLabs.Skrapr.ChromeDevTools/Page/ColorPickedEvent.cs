namespace BaristaLabs.Skrapr.ChromeDevTools.Page
{
    /// <summary>
    /// Fired when a color has been picked.
    /// </summary>
    public sealed class ColorPickedEvent : IEvent
    {
    
        
        /// <summary>
        /// RGBA of the picked color.
        /// </summary>
        
        public BaristaLabs.Skrapr.ChromeDevTools.DOM.RGBA Color
        {
            get;
            set;
        }
    
    }
}