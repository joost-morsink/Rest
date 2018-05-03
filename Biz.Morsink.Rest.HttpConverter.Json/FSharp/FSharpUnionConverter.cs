using Biz.Morsink.Rest.FSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.Json.FSharp
{
    using Newtonsoft.Json.Serialization;
    using static Morsink.Rest.FSharp.Names;
    public class FSharpUnionConverter : JsonConverter
    {
        public static bool IsFSharpUnionType(Type type)
            => Biz.Morsink.Rest.FSharp.Utils.IsFsharpUnionType(type);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var kees = UnionType.Cases[tagFunc(value)];
            var namingStrategy = (serializer.ContractResolver as DefaultContractResolver)?.NamingStrategy;
            var namerFunc = namingStrategy != null
                ? new Func<string, string>(x => namingStrategy.GetPropertyName(x, false))
                : x => x;



            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
            => ForType.IsAssignableFrom(objectType);

        public FSharpUnionConverter(Type forType)
        {
            ForType = forType;
            UnionType = UnionType.Create(forType);
            tagFunc = MakeTagFunc();
        }
        private Func<object, int> MakeTagFunc()
        {
            var p = Ex.Parameter(typeof(object), "p");
            var block = Ex.Property(p, Tag);
            var lambda = Ex.Lambda<Func<object, int>>(block, p);
            return lambda.Compile();
        }
        public Type ForType { get; }
        public UnionType UnionType { get; }
        private readonly Func<object, int> tagFunc;
    }
}
