using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Static helper class to construct generic SemanticStructConverters.
    /// </summary>
    public static class SemanticStructConverter
    {
        /// <summary>
        /// Discovers the underlying type of a semantic struct and constructs a converter.
        /// </summary>
        public static JsonConverter Create(Type structType)
            => (JsonConverter)Activator.CreateInstance(typeof(SemanticStructConverter<,>).MakeGenericType(structType, SemanticStructKind.GetUnderlyingType(structType)));
        /// <summary>
        /// Discovers the underlying type of a semantic struct and constructs a converter.
        /// </summary>
        public static JsonConverter Create<S>()
            => Create(typeof(S));
    }
    /// <summary>
    /// Implements a JsonConverter for semantic structs.
    /// </summary>
    /// <typeparam name="S">The type of the semantic struct.</typeparam>
    /// <typeparam name="P">The type of the underlying property.</typeparam>
    public class SemanticStructConverter<S, P> : JsonConverter
    {
        private readonly Func<P, S> create;
        private readonly Func<S, P> get;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SemanticStructConverter()
        {
            create = MakeCreate();
            get = MakeGet();
        }

        private Func<P, S> MakeCreate()
        {
            var p = Ex.Parameter(typeof(P), "p");
            var block = Ex.New(typeof(S).GetConstructor(new[] { typeof(P) }), p);
            var lambda = Ex.Lambda<Func<P, S>>(block, p);
            return lambda.Compile();
        }
        
        private Func<S, P> MakeGet()
        {
            var s = Ex.Parameter(typeof(S), "s");
            var block = Ex.Property(s, typeof(S).GetProperties().Where(p => p.PropertyType == typeof(P)).First());
            var lambda = Ex.Lambda<Func<S, P>>(block, s);
            return lambda.Compile();
        }

        /// <summary>
        /// This class can only convert objects of type S.
        /// </summary>
        public override bool CanConvert(Type objectType)
            => objectType == typeof(S);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var p = serializer.Deserialize<P>(reader);
            return create(p);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var p = get((S)value);
            serializer.Serialize(writer, p);
        }
    }
}
