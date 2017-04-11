namespace BaristaLabs.Skrapr.Definitions
{
    using Newtonsoft.Json;

    public interface ISchedule
    {
        /// <summary>
        /// Gets the type of the schedule
        /// </summary>
        [JsonProperty("type")]
        string Type
        {
            get;
        }
    }
}
