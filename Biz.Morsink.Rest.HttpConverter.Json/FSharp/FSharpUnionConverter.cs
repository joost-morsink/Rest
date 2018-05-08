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
    using Biz.Morsink.Rest.Utils;
    using Newtonsoft.Json.Serialization;
    using static Morsink.Rest.FSharp.Names;
    public class FSharpUnionConverter : JsonConverter
    {
        public static bool IsFSharpUnionType(Type type)
            => Biz.Morsink.Rest.FSharp.Utils.IsFsharpUnionType(type);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var @case = UnionType.Cases[tagFunc(value)];
            var namingStrategy = (serializer.ContractResolver as DefaultContractResolver)?.NamingStrategy;
            var namerFunc = namingStrategy != null
                ? new Func<string, string>(x => namingStrategy.GetPropertyName(x, false))
                : x => x;
            writer.WriteStartObject();
            writer.WritePropertyName(namerFunc("Tag"));
            writer.WriteValue(@case.Name);
            foreach (var param in @case.Parameters)
            {
                writer.WritePropertyName(namerFunc(param.Name));
                serializer.Serialize(writer, param.Property.GetValue(value));
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dict = new Dictionary<string, object>();
            if (reader.TokenType != JsonToken.StartObject)
                throw new InvalidOperationException("JSON reader should have its cursor on a StartObject token.");
            reader.Read();
            while (reader.TokenType != JsonToken.EndObject)
            {
                var name = (string)reader.Value;
                reader.Read();
                dict[name.CasedToPascalCase()] = serializer.Deserialize(reader);
                reader.Read();
            }
            reader.Read();
            if (!dict.TryGetValue("Tag", out var tag))
                throw new FormatException("Object does not contain a 'Tag' property.");
            if (!UnionType.CasesByName.TryGetValue(tag.ToString(), out var @case))
                throw new FormatException($"Unknown tag '{tag}'");
            var absent = @case.Parameters.Where(p => !dict.ContainsKey(p.Name));
            if (absent.Any())
                throw new FormatException($"Missing properties: {string.Join(", ", absent)}");
            return @case.ConstructorMethod.Invoke(null, @case.Parameters.Select(p => dict[p.Name]).ToArray());
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
            var block = Ex.Property(Ex.Convert(p, ForType), Tag);
            var lambda = Ex.Lambda<Func<object, int>>(block, p);
            return lambda.Compile();
        }
        public Type ForType { get; }
        public UnionType UnionType { get; }
        private readonly Func<object, int> tagFunc;
    }
}
