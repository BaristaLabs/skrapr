namespace BaristaLabs.Skrapr.ChromeDevTools.Network
{
    /// <summary>
    /// Fired if request ended up loading from cache.
    /// </summary>
    public sealed class RequestServedFromCacheEvent : IEvent
    {
    
        
        /// <summary>
        /// Request identifier.
        /// </summary>
        
        public string RequestId
        {
            get;
            set;
        }
    
    }
}