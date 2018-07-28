using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {

        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed HalSerializer for semantic structs.
            /// </summary>
            /// <typeparam name="P">The type of the underlying value.</typeparam>
            public class SemanticStruct<P> : Typed<T>
            {
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public SemanticStruct(HalSerializer parent) : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);
                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);
                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var ctor = typeof(T).GetConstructor(new[] { typeof(P) });
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var token = Ex.Parameter(typeof(JToken), "token");
                    var block = Ex.New(ctor, Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { typeof(P) },
                        ctx, token));
                    var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, token);
                    return lambda.Compile();
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var prop = typeof(T).GetProperties().Where(p => p.PropertyType == typeof(P)).First();
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");
                    var block = Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), new[] { typeof(P) },
                        ctx,
                        Ex.Property(input, prop));
                    var lambda = Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input);
                    return lambda.Compile();
                }
            }
        }
    }
}
