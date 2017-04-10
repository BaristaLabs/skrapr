namespace BaristaLabs.Skrapr.ChromeDevTools.LayerTree
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LayerPaintedEvent : IEvent
    {
    
        
        /// <summary>
        /// The id of the painted layer.
        /// </summary>
        
        public string LayerId
        {
            get;
            set;
        }
    
        
        /// <summary>
        /// Clip rectangle.
        /// </summary>
        
        public BaristaLabs.Skrapr.ChromeDevTools.DOM.Rect Clip
        {
            get;
            set;
        }
    
    }
}