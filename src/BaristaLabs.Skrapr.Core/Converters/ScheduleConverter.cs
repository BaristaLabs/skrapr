namespace BaristaLabs.Skrapr.Converters
{
    using BaristaLabs.Skrapr.Definitions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Reflection;

    public class ScheduleConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanConvert(System.Type objectType)
        {
            return typeof(ISchedule).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var typeName = jsonObject["type"].ToString();

            ISchedule schedule;
            switch (typeName)
            {
                case "cron":
                    schedule = new CronSchedule();
                    break;
                default:
                    return null;
            }

            serializer.Populate(jsonObject.CreateReader(), schedule);
            return schedule;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }
    }
}
