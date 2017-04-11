namespace BaristaLabs.Skrapr.Converters
{
    using BaristaLabs.Skrapr.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Reflection;

    public class TaskConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ITask).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject task = JObject.Load(reader);
            object target = null;

            switch (task["Name"].Value<String>())
            {
                case "Navigate":
                    target = new NavigateTask();
                    break;
                default:
                    throw new ArgumentException("Invalid task type");
            }

            serializer.Populate(task.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
