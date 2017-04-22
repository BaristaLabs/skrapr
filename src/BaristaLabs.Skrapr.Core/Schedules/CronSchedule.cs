namespace BaristaLabs.Skrapr.Schedules
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a schedule defined as a Cron Expression.
    /// </summary>
    public class CronSchedule : SkraprSchedule
    {
        public override string Type
        {
            get { return "cron"; }
        }

        [JsonProperty("cronExpression", Required = Required.Always)]
        public string CronExpression
        {
            get;
            set;
        }
    }
}
