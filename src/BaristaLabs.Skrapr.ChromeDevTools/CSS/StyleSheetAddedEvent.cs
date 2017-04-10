namespace BaristaLabs.Skrapr.ChromeDevTools.CSS
{
    /// <summary>
    /// Fired whenever an active document stylesheet is added.
    /// </summary>
    public sealed class StyleSheetAddedEvent : IEvent
    {
    
        
        /// <summary>
        /// Added stylesheet metainfo.
        /// </summary>
        
        public CSSStyleSheetHeader Header
        {
            get;
            set;
        }
    
    }
}