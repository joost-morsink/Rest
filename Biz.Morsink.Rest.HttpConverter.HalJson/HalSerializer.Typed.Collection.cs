using System;
using System.Collections.Generic;
using System.Linq;
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
            /// Typed HalSerializer for collection types.
            /// </summary>
            public class Collection : Typed<T>
            {
                private readonly Type baseType;
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public Collection(HalSerializer parent) : base(parent)
                {
                    baseType = typeof(T).GetGeneric(typeof(IEnumerable<>));
                    if (baseType == null)
                        throw new ArgumentException("Generic type is not a collection");
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();

                }
                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);
                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);

                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var input = Ex.Parameter(typeof(JToken), "input");
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var children = Ex.Parameter(typeof(JToken[]), "children");
                    var idx = Ex.Parameter(typeof(int), "idx");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    if (typeof(T).IsArray || !typeof(ICollection<>).MakeGenericType(baseType).IsAssignableFrom(typeof(T)))
                    {
                        var result = Ex.Parameter(baseType.MakeArrayType(), "result");
                        var block = Ex.Block(new[] { children, idx, result },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(JToken)),
                                    Ex.Convert(
                                        Ex.Call(input, nameof(JToken.Children), Type.EmptyTypes),
                                        typeof(IEnumerable<JToken>)))),
                            Ex.Assign(result, Ex.NewArrayBounds(baseType, Ex.Property(children, nameof(Array.Length)))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Assign(Ex.ArrayAccess(result, idx),
                                Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { baseType },
                                    ctx, Ex.ArrayIndex(children, idx))),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            Ex.Convert(result, typeof(T)));

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                    else if (typeof(T).GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(baseType) }) != null)
                    {
                        var elements = Ex.Parameter(typeof(List<>).MakeGenericType(baseType), "elements");
                        var block = Ex.Block(new[] { children, idx, elements },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(JToken)),
                                    Ex.Convert(
                                        Ex.Call(input, nameof(JToken.Children), Type.EmptyTypes),
                                        typeof(IEnumerable<JToken>)))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Assign(elements, Ex.New(elements.Type)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Call(elements, nameof(List<object>.Add), Type.EmptyTypes,
                                Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { baseType },
                                    ctx, Ex.ArrayIndex(children, idx))),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            Ex.New(typeof(T).GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(baseType) }),
                                Ex.Convert(elements, typeof(IEnumerable<>).MakeGenericType(baseType))));

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                    else if (typeof(T).GetConstructor(Type.EmptyTypes) != null)
                    {
                        var result = Ex.Parameter(typeof(T), "result");
                        var block = Ex.Block(new[] { children, idx, result },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(JToken)),
                                    Ex.Convert(
                                        Ex.Call(input, nameof(JToken.Children), Type.EmptyTypes),
                                        typeof(IEnumerable<JToken>)))),
                            Ex.Assign(result, Ex.New(typeof(T).GetConstructor(Type.EmptyTypes))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Call(Ex.Convert(result, typeof(ICollection<>).MakeGenericType(baseType)), nameof(ICollection<object>.Add), Type.EmptyTypes,
                                Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { baseType },
                                    ctx, Ex.ArrayIndex(children, idx))),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            result);

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                    return null;
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var result = Ex.Parameter(typeof(List<object>), "result");
                    var block = Ex.Block(new[] { result },
                        Ex.Assign(result, Ex.New(typeof(List<object>))),
                        input.Foreach(item =>
                            Ex.Call(result, nameof(List<object>.Add), Type.EmptyTypes,
                                Ex.Convert(
                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), Type.EmptyTypes, ctx, Ex.Convert(item, typeof(object))),
                                    typeof(object)))),
                        Ex.New(typeof(JArray).GetConstructor(new[] { typeof(object[]) }),
                            Ex.Call(typeof(Enumerable), nameof(Enumerable.ToArray), new[] { typeof(object) }, result)));

                    var lambda = Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input);
                    return lambda.Compile();
                }
            }
        }
    }
}
