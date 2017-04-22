namespace BaristaLabs.Skrapr
{
    using Newtonsoft.Json;

    public interface ISkraprSchedule
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
