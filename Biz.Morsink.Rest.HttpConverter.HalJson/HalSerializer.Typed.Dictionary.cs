using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            /// Typed HalSerializer for Dictionary-like types.
            /// </summary>
            public class Dictionary : Typed<T>
            {
                private readonly Type valueType;
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;
                private readonly Type kind;

                public Dictionary(HalSerializer parent) : base(parent)
                {
                    var (keyType, valueType) = typeof(T).GetGenerics2(typeof(IDictionary<,>));
                    if (keyType == null || keyType != typeof(string) || valueType == null)
                        throw new ArgumentException("Generic type is not a proper dictionary");
                    kind = typeof(T).IsGenericType ? typeof(T).GetGenericTypeDefinition() : typeof(T);
                    this.valueType = valueType;
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }
                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);
                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);

                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(JToken), "input");
                    var @class = kind == typeof(IDictionary<,>) || kind == typeof(IImmutableDictionary<,>) || kind == typeof(ImmutableDictionary<,>)
                        ? typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType)
                        : typeof(T);
                    var result = Ex.Parameter(@class, "result");
                    var props = Ex.Parameter(typeof(IEnumerable<JProperty>), "props");
                    if (valueType == typeof(object))
                    {
                        var block = Ex.Block(new[] { result, props },
                            Ex.Assign(result, Ex.New(@class)),
                            Ex.Assign(props, Ex.Convert(Ex.Call(input, nameof(JToken.Children), new[] { typeof(JProperty) }), typeof(IEnumerable<JProperty>))),
                            props.Foreach(prop =>
                                Ex.Call(result, nameof(IDictionary<string, object>.Add), Type.EmptyTypes,
                                    Ex.Property(prop, nameof(JProperty.Name)),
                                    Ex.Condition(Ex.TypeIs(Ex.Property(prop, nameof(JProperty.Value)), typeof(JObject)),
                                        Ex.Convert(Ex.Call(Ex.Constant(this), nameof(Deserialize), Type.EmptyTypes,
                                            ctx, Ex.Property(prop, nameof(JProperty.First))), typeof(object)),
                                        Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), Type.EmptyTypes,
                                            Ex.Constant(typeof(string)), ctx, Ex.Property(prop, nameof(JProperty.First))), typeof(object))))),
                            Ex.Convert(
                                kind == typeof(IImmutableDictionary<,>) || kind == typeof(ImmutableDictionary<,>)
                                    ? (Ex)Ex.Call(typeof(ImmutableDictionary), nameof(ImmutableDictionary.ToImmutableDictionary), new[] { typeof(string), valueType }, result)
                                    : result,
                                typeof(T)));

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                    else
                    {
                        var block = Ex.Block(new[] { result, props },
                            Ex.Assign(result, Ex.New(@class)),
                            Ex.Assign(props, Ex.Convert(Ex.Call(input, nameof(JToken.Children), new[] { typeof(JProperty) }), typeof(IEnumerable<JProperty>))),
                            props.Foreach(prop =>
                                Ex.Call(result, nameof(IDictionary<string, object>.Add), Type.EmptyTypes,
                                    Ex.Property(prop, nameof(JProperty.Name)),
                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { valueType },
                                        ctx, Ex.Property(prop, nameof(JProperty.First))))),
                            Ex.Convert(
                                kind == typeof(IImmutableDictionary<,>) || kind == typeof(ImmutableDictionary<,>)
                                    ? (Ex)Ex.Call(typeof(ImmutableDictionary), nameof(ImmutableDictionary.ToImmutableDictionary), new[] { typeof(string), valueType }, result)
                                    : result,
                                typeof(T)));

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");
                    var kvp = Ex.Parameter(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), valueType), "kvp");
                    var select = typeof(Enumerable).GetMethods()
                        .Where(m => m.Name == nameof(Enumerable.Select)
                            && m.GetGenericArguments().Length == 2
                            && m.GetParameters().Length == 2
                            && m.GetParameters()[1].ParameterType == typeof(Func<,>).MakeGenericType(m.GetGenericArguments()[0], m.GetGenericArguments()[1]))
                        .First().MakeGenericMethod(kvp.Type, typeof(JProperty));
                    var ctor = typeof(JObject).GetConstructor(new[] { typeof(object) });
                    var pctor = typeof(JProperty).GetConstructor(new[] { typeof(string), typeof(object) });
                    var innerlambda = Ex.Lambda(
                        Ex.New(pctor,
                            Ex.Property(kvp, nameof(KeyValuePair<string, object>.Key)),
                            Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), Type.EmptyTypes,
                                ctx,
                                Ex.Convert(Ex.Property(kvp, nameof(KeyValuePair<string, object>.Value)), typeof(object)))),
                        kvp);

                    var block = Ex.Convert(Ex.New(ctor,
                        Ex.Convert(
                            Ex.Call(select,
                                Ex.Convert(input, typeof(IEnumerable<>).MakeGenericType(kvp.Type)),
                                innerlambda), typeof(object))), typeof(JToken));

                    var lambda = Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input);

                    return lambda.Compile();
                }
            }
        }
    }
}
