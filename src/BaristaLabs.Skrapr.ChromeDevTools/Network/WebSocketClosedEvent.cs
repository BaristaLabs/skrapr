namespace BaristaLabs.Skrapr.ChromeDevTools.Network
{
    /// <summary>
    /// Fired when WebSocket is closed.
    /// </summary>
    public sealed class WebSocketClosedEvent : IEvent
    {
    
        
        /// <summary>
        /// Request identifier.
        /// </summary>
        
        public string RequestId
        {
            get;
            set;
        }
    
        
        /// <summary>
        /// Timestamp.
        /// </summary>
        
        public double Timestamp
        {
            get;
            set;
        }
    
    }
}