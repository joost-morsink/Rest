using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    public class TypeDescriptorConverter : JsonConverter
    {
        private readonly IOptions<JsonHttpConverterOptions> options;

        public TypeDescriptorConverter(IOptions<JsonHttpConverterOptions> options)
        {
            this.options = options;
        }
        public override bool CanConvert(Type objectType)
            => typeof(TypeDescriptor).IsAssignableFrom(objectType);
        public override bool CanRead => false;
        public override bool CanWrite => true;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var visitor = new JsonSchemaTypeDescriptorVisitor();
            var schema = visitor.Transform((TypeDescriptor)value);
            schema.WriteTo(writer, serializer.Converters.ToArray());
        }
    }
}
