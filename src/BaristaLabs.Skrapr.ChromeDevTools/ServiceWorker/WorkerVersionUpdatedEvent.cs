namespace BaristaLabs.Skrapr.ChromeDevTools.ServiceWorker
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WorkerVersionUpdatedEvent : IEvent
    {
    
        
        /// <summary>
        /// Gets or sets the versions
        /// </summary>
        
        public ServiceWorkerVersion[] Versions
        {
            get;
            set;
        }
    
    }
}