using Biz.Morsink.Rest.HttpConverter.Json;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter
{
    public class UnionRepresentationSchemaTranslator : IJsonSchemaTranslator
    {
        private readonly Lazy<IEnumerable<IJsonSchemaTranslator>> translators;
        private readonly ITypeDescriptorCreator typeDescriptorCreator;

        public UnionRepresentationSchemaTranslator(IServiceProvider serviceProvider, ITypeDescriptorCreator typeDescriptorCreator)
        {
            this.translators = new Lazy<IEnumerable<IJsonSchemaTranslator>>(() =>
                serviceProvider.GetServices<IJsonSchemaTranslator>().Where(tr => tr != this));
            this.typeDescriptorCreator = typeDescriptorCreator;
        }


        public JsonConverter GetConverter(Type type)
        {
            if (!UnionRepresentationDescriptorKind.IsOfKind(type))
                return null;
            return new Converter(type,typeDescriptorCreator);
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
            private readonly (Type, TypeDescriptor)[] optionTypes;

            public Converter(Type type, ITypeDescriptorCreator creator)
            {
                optionTypes = UnionRepresentation.GetTypeParameters(type)?.Select(t => (t, creator.GetDescriptor(t))).ToArray();

                if (optionTypes == null)
                    throw new ArgumentException("Type is not a UnionRepresentation.");
            }
            public override bool CanRead => true;
            public override bool CanWrite => true;

            public override bool CanConvert(Type objectType)
                => UnionRepresentationDescriptorKind.IsOfKind(objectType);

            private int Score(TypeDescriptor td, JToken jtok)
            {
                var score = 0;
                if (jtok is JObject jobj)
                {
                    var props = (td as TypeDescriptor.Record)?.Properties;
                    if (props != null)
                    {
                        var req = new HashSet<string>(props.Where(p => p.Value.Required).Select(p => p.Key));
                        foreach (var prop in jobj.Properties())
                        {
                            if(props.TryGetValue(prop.Name, out var desc))
                            {
                                if (desc.Required)
                                    req.Remove(desc.Name);
                                score += 10;
                            }
                        }
                        if (req.Count == 0)
                            score *= 10;
                        return score;
                    }
                }
                return 1;
            }
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var types = UnionRepresentation.GetTypeParameters(objectType);
                var token = serializer.Deserialize<JToken>(reader);
                Type best = null;
                int score = int.MinValue;
                foreach(var (type,desc) in optionTypes)
                {
                    var sc = Score(desc, token);
                    if(sc > score)
                    {
                        best = type;
                        score = sc;
                    }
                }
                using (var rdr = token.CreateReader())
                    return UnionRepresentation.FromOptions(types).Create(serializer.Deserialize(rdr, best));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, ((UnionRepresentation)value).GetItem());
            }
        }
    }
}
