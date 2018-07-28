using System;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Utils;
using Newtonsoft.Json.Linq;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {

        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed HalSerializer for Nullable&lt;T&gt; types.
            /// </summary>
            public class Nullable : Typed<T>
            {
                private readonly Type valueType;
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public Nullable(HalSerializer parent)
                    : base(parent)
                {
                    valueType = typeof(T).GetGeneric(typeof(Nullable<>));
                    if (valueType == null)
                        throw new ArgumentException("Generic type should be Nullable<X>", nameof(T));
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }
                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);
                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);

                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(JToken), "input");

                    var block = Ex.Condition(
                            Ex.MakeBinary(System.Linq.Expressions.ExpressionType.Equal, Ex.Property(input, nameof(JToken.Type)), Ex.Constant(JTokenType.Null)),
                            Ex.Default(typeof(T)),
                            Ex.New(typeof(T).GetConstructor(new[] { valueType }),
                                Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { valueType }, ctx, input)));
                    return Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input).Compile();
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");

                    var block = Ex.Condition(
                        Ex.Property(input, nameof(Nullable<int>.HasValue)),
                        Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), new[] { valueType }, ctx, Ex.Property(input, nameof(Nullable<int>.Value))),
                        Ex.Constant(JValue.CreateNull(), typeof(JToken)));
                    return Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input).Compile();
                }
            }
        }
    }
}
