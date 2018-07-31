using Biz.Morsink.Rest.HttpConverter.Json;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter
{
    public class UnionRepresentationSchemaTranslator : IJsonSchemaTranslator
    {
        private readonly Lazy<IEnumerable<IJsonSchemaTranslator>> translators;
        private readonly TypeDescriptorCreator typeDescriptorCreator;

        public UnionRepresentationSchemaTranslator(IServiceProvider serviceProvider, TypeDescriptorCreator typeDescriptorCreator)
        {
            this.translators = new Lazy<IEnumerable<IJsonSchemaTranslator>>(() =>
                serviceProvider.GetServices<IJsonSchemaTranslator>().Where(tr => tr != this));
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
            var visitor = new JsonSchemaTypeDescriptorVisitor(typeDescriptorCreator, translators.Value);
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
