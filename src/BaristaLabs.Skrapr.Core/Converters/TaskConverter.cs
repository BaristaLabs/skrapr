namespace BaristaLabs.Skrapr.Converters
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class TaskConverter : JsonConverter
    {
        public static Lazy<IDictionary<string, Type>> s_taskTypes = new Lazy<IDictionary<string, Type>>(() =>
       {
           var taskTypeDictionary = new Dictionary<string, Type>();
           var taskInterfaceType = typeof(ISkraprTask);
           var types = typeof(TaskConverter).GetTypeInfo().Assembly
               .GetTypes()
               .Select(t => t.GetTypeInfo())
               .Where(ti => ti.IsClass && !ti.IsAbstract && ti.ImplementedInterfaces.Any(ii => ii == taskInterfaceType));

           foreach (var t in types)
           {
               var taskInstance = (ISkraprTask)Activator.CreateInstance(t.AsType());
               if (String.IsNullOrWhiteSpace(taskInstance.Name))
                   throw new InvalidOperationException($"The type {t} does not specify a name.");

               if (taskTypeDictionary.ContainsKey(taskInstance.Name))
                   throw new InvalidOperationException($"The task name {taskInstance.Name} on {t} has already been specified by {taskTypeDictionary[taskInstance.Name]}");
               
               taskTypeDictionary.Add(taskInstance.Name, t.AsType());
           }

           return taskTypeDictionary;
       });

        static TaskConverter()
        {
            
        }

        
        public override bool CanConvert(Type objectType)
        {
            return typeof(ISkraprTask).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject task = JObject.Load(reader);
            object target = null;

            if (task.TryGetValue("name", out JToken value))
            {
                var taskName = value.Value<string>();
                if (s_taskTypes.Value.ContainsKey(taskName))
                    target = Activator.CreateInstance(s_taskTypes.Value[taskName]);
                else
                    throw new ArgumentException($"Invalid task type: { taskName }");
            }
            else
            {
                throw new InvalidOperationException("The specified task did not indicate a name:" + task.ToString());
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
