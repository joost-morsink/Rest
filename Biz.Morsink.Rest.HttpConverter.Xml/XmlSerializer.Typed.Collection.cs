using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Ex = System.Linq.Expressions.Expression;
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public partial class XmlSerializer
    {
                public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed XmlSerializer for collection types.
            /// </summary>
            public class Collection : Typed<T>
            {
                private readonly Type basetype;
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                public Collection(XmlSerializer parent) : base(parent)
                {
                    basetype = typeof(T).GetGeneric(typeof(IEnumerable<>));
                    if (basetype == null)
                        throw new ArgumentException("Generic type is not a collection");
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }
                private Func<T, XElement> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var result = Ex.Parameter(typeof(XElement), "result");
                    var block = Ex.Block(new[] { result },
                        Ex.Assign(result, Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName) }), Ex.Constant((XName)"Array"))),
                        input.Foreach(item =>
                            Ex.Call(result, nameof(XElement.Add), Type.EmptyTypes,
                                Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                    Ex.Convert(item, typeof(object))),
                                    typeof(object)))),
                        result);
                    var lambda = Ex.Lambda(block, input);
                    return (Func<T, XElement>)lambda.Compile();
                }
                private Func<XElement, T> MakeDeserializer()
                {
                    var input = Ex.Parameter(typeof(XElement), "input");
                    var children = Ex.Parameter(typeof(XElement[]), "children");
                    var idx = Ex.Parameter(typeof(int), "idx");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    if (typeof(T).IsArray || !typeof(ICollection<>).MakeGenericType(basetype).IsAssignableFrom(typeof(T)))
                    {
                        var result = Ex.Parameter(basetype.MakeArrayType(), "tmp");
                        var block = Ex.Block(new[] { children, idx, result },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(XElement)),
                                    Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes))),
                            Ex.Assign(result, Ex.NewArrayBounds(basetype, Ex.Property(children, nameof(Array.Length)))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Assign(Ex.ArrayAccess(result, idx),
                                Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                    Ex.ArrayIndex(children, idx), Ex.Constant(basetype)), basetype)),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            Ex.Convert(result, typeof(T)));

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    else if (typeof(T).GetConstructor(new [] { typeof(IEnumerable<>).MakeGenericType(basetype) })!=null)
                    {
                        var elements = Ex.Parameter(typeof(List<>).MakeGenericType(basetype), "elements");
                        var block = Ex.Block(new[] { children, idx, elements },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(XElement)),
                                    Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Assign(elements, Ex.New(elements.Type)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Call(elements, nameof(List<object>.Add), Type.EmptyTypes,
                                Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                    Ex.ArrayIndex(children, idx), Ex.Constant(basetype)), basetype)),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            Ex.New(typeof(T).GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(basetype) }),
                                Ex.Convert(elements, typeof(IEnumerable<>).MakeGenericType(basetype))));

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    else if (typeof(T).GetConstructor(Type.EmptyTypes) != null)
                    {
                        var result = Ex.Parameter(typeof(T), "result");
                        var block = Ex.Block(new[] { children, idx, result },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(XElement)),
                                    Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes))),
                            Ex.Assign(result, Ex.New(typeof(T).GetConstructor(Type.EmptyTypes))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Call(Ex.Convert(result, typeof(ICollection<>).MakeGenericType(basetype)), nameof(ICollection<object>.Add), Type.EmptyTypes,
                                Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                    Ex.ArrayIndex(children, idx), Ex.Constant(basetype)), basetype)),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            result);

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    return null;
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);
            }
        }

    }
}
