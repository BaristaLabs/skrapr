namespace BaristaLabs.Skrapr.Converters
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class RuleConverter : JsonConverter
    {
        public static Lazy<IDictionary<string, Type>> s_ruleTypes = new Lazy<IDictionary<string, Type>>(() =>
       {
           var ruleTypeDictionary = new Dictionary<string, Type>();
           var ruleInterfaceType = typeof(ISkraprRule);
           var types = typeof(RuleConverter).GetTypeInfo().Assembly
               .GetTypes()
               .Select(t => t.GetTypeInfo())
               .Where(ti => ti.IsClass && !ti.IsAbstract && ti.ImplementedInterfaces.Any(ii => ii == ruleInterfaceType));

           foreach (var t in types)
           {
               var ruleInstance = (ISkraprRule)Activator.CreateInstance(t.AsType());
               if (String.IsNullOrWhiteSpace(ruleInstance.Type))
                   throw new InvalidOperationException($"The rule {t} does not specify a type.");

               if (ruleTypeDictionary.ContainsKey(ruleInstance.Type))
                   throw new InvalidOperationException($"The rule typename {ruleInstance.Type} on {t} has already been specified by {ruleTypeDictionary[ruleInstance.Type]}");

               ruleTypeDictionary.Add(ruleInstance.Type, t.AsType());
           }

           return ruleTypeDictionary;
       });

        static RuleConverter()
        {
            
        }

        
        public override bool CanConvert(Type objectType)
        {
            return typeof(ISkraprRule).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject rule = JObject.Load(reader);
            object target = null;

            if (rule.TryGetValue("type", out JToken value))
            {
                var ruleType = value.Value<string>();
                if (s_ruleTypes.Value.ContainsKey(ruleType))
                    target = Activator.CreateInstance(s_ruleTypes.Value[ruleType]);
                else
                    throw new ArgumentException($"Invalid or unknown rule type: { ruleType }");
            }
            else
            {
                throw new InvalidOperationException("The specified rule did not indicate a type:" + rule.ToString());
            }

            serializer.Populate(rule.CreateReader(), target);

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
