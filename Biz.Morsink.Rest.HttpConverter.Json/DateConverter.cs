using System;
using Newtonsoft.Json;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    internal class DateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(DateTime) || objectType == typeof(DateTime?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
            {
                var dt = (DateTime)value;
                writer.WriteValue(dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }
        }
    }
}