using Biz.Morsink.Rest.FSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json.FSharp
{
    public class FSharpUnionConverter : JsonConverter
    {
        public static bool IsFSharpUnionType(Type type)
            => Biz.Morsink.Rest.FSharp.Utils.IsFsharpUnionType(type);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public FSharpUnionConverter(Type forType)
        {
            ForType = forType;
            UnionType = UnionType.Create(forType);
        }
        public Type ForType { get; }
        public UnionType UnionType { get; }
    }
}
