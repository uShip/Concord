using System;
using Newtonsoft.Json;

namespace concord.Output.JsonConverters
{
    public class TimeSpanJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timespan = (TimeSpan)value;

            writer.WriteValue(Math.Round(timespan.TotalMilliseconds, 4));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (TimeSpan);
        }
    }
}