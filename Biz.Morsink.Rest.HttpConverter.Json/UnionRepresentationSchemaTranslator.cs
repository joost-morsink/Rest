using Biz.Morsink.Rest.HttpConverter.Json;
using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter
{
    public class UnionRepresentationSchemaTranslator : IJsonSchemaTranslator
    {
        private readonly IJsonSchemaProvider schemaProvider;
        private readonly TypeDescriptorCreator typeDescriptorCreator;

        public UnionRepresentationSchemaTranslator(IJsonSchemaProvider schemaProvider, TypeDescriptorCreator typeDescriptorCreator)
        {
            this.schemaProvider = schemaProvider;
            this.typeDescriptorCreator = typeDescriptorCreator;
        }


        public JsonConverter GetConverter(Type type)
        {
            if (!UnionRepresentationDescriptorKind.IsOfKind(type))
                return null;
            return Converter.Instance;
        }

        public JsonSchema GetSchema(Type type)
        {
            if (!UnionRepresentationDescriptorKind.IsOfKind(type))
                return null;
            var visitor = new JsonSchemaTypeDescriptorVisitor(typeDescriptorCreator);
            var schema = visitor.Transform(typeDescriptorCreator.GetDescriptor(type));
            return new JsonSchema(schema);
        }

        private class Converter : JsonConverter
        {
            public static Converter Instance { get; } = new Converter();
            public override bool CanRead => false;
            public override bool CanWrite => true;

            public override bool CanConvert(Type objectType)
                => UnionRepresentationDescriptorKind.IsOfKind(objectType);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, ((UnionRepresentation)value).GetItem());
            }
        }
    }
}
