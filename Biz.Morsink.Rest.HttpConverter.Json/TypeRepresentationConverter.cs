using System;
using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    public class TypeRepresentationConverter : JsonConverter
    {
        private readonly Type objectType;
        private readonly ITypeRepresentation typeRep;

        public TypeRepresentationConverter(Type objectType, ITypeRepresentation typeRep)
        {
            this.objectType = objectType;
            this.typeRep = typeRep;
        }
        public override bool CanConvert(Type objectType)
            => this.objectType == objectType;

        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var representationType = typeRep.GetRepresentationType(objectType);
            var representation = serializer.Deserialize(reader, representationType);
            return typeRep.GetRepresentable(representation) ?? existingValue;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var representation = typeRep.GetRepresentation(value);
            serializer.Serialize(writer, representation);
        }
    }
}