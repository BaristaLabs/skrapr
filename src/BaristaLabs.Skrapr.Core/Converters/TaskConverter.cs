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

            switch (task["name"].Value<String>())
            {
                case "Navigate":
                    target = new NavigateTask();
                    break;
                case "ClickDomElement":
                    target = new ClickDomElementTask();
                    break;
                default:
                    throw new ArgumentException($"Invalid task type: { task["name"] }");
            }

            serializer.Populate(task.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //Serialize normally.
            var t = JToken.FromObject(value);
            t.WriteTo(writer);
        }
    }
}
