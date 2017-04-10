namespace BaristaLabs.Skrapr.ChromeDevTools.Profiler
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConsoleProfileFinishedEvent : IEvent
    {
    
        
        /// <summary>
        /// Gets or sets the id
        /// </summary>
        
        public string Id
        {
            get;
            set;
        }
    
        
        /// <summary>
        /// Location of console.profileEnd().
        /// </summary>
        
        public BaristaLabs.Skrapr.ChromeDevTools.Debugger.Location Location
        {
            get;
            set;
        }
    
        
        /// <summary>
        /// Gets or sets the profile
        /// </summary>
        
        public Profile Profile
        {
            get;
            set;
        }
    
        
        /// <summary>
        /// Profile title passed as an argument to console.profile().
        /// </summary>
        
        public string Title
        {
            get;
            set;
        }
    
    }
}