using Biz.Morsink.Rest.FSharp;
using Biz.Morsink.Rest.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.Json.FSharp
{
    using static Morsink.Rest.FSharp.Names;
    /// <summary>
    /// A JsonConverter for F# union types.
    /// </summary>
    public class FSharpUnionConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether the provided type is an F# union type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an F# union type.</returns>
        public static bool IsFSharpUnionType(Type type)
            => Biz.Morsink.Rest.FSharp.Utils.IsFsharpUnionType(type) && !typeof(IEnumerable).IsAssignableFrom(type);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (UnionType.IsSingleValue)
            {
                var val = UnionType.Cases.Values.First().Parameters[0].Property.GetValue(value);
                serializer.Serialize(writer, val);
            }
            else
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
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (UnionType.IsSingleValue)
            {
                var type = UnionType.Cases.Values.First().Parameters[0].Type;
                var val = serializer.Deserialize(reader, type);
                return UnionType.Cases.Values.First().ConstructorMethod.Invoke(null, new[] { val });
            }
            else
            {
                var dict = new Dictionary<string, object>();
                var tok = JToken.ReadFrom(reader);
                if (tok is JObject obj)
                {
                    if (!obj.TryGetValue("Tag", out var tag))
                        throw new JsonSerializationException("Object does not contain a 'Tag' property.");
                    if (!UnionType.CasesByName.TryGetValue(tag.Value<string>(), out var @case))
                        throw new JsonSerializationException($"Unknown tag '{tag.Value<string>()}'");
                    var absent = @case.Parameters.Where(p => obj.Property(p.Name) == null);
                    if (absent.Any())
                        throw new FormatException($"Missing properties: {string.Join(", ", absent)}");
                    return @case.ConstructorMethod.Invoke(null, @case.Parameters.Select(p =>
                        {
                            using (var rdr = obj.Property(p.Name).Value.CreateReader())
                                return serializer.Deserialize(rdr, p.Type);
                        }).ToArray());
                }
                else
                    throw new JsonSerializationException("Object expected.");
            }
        }

        public override bool CanConvert(Type objectType)
            => ForType.IsAssignableFrom(objectType);
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="forType">The F# union type.</param>
        public FSharpUnionConverter(Type forType)
        {
            if (!IsFSharpUnionType(forType))
                throw new ArgumentException("Type is not a proper F# union type.");
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
        /// <summary>
        /// The F# union type this instance can convert.
        /// </summary>
        public Type ForType { get; }
        /// <summary>
        /// A UnionType representation of the F# union type.
        /// </summary>
        public UnionType UnionType { get; }
        private readonly Func<object, int> tagFunc;
    }
}
