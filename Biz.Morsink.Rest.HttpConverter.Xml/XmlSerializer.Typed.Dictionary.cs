using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Ex = System.Linq.Expressions.Expression;
using Biz.Morsink.Rest.Utils;
using System.Collections.Immutable;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public partial class XmlSerializer
    {
        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed XmlSerializer for Dictionary-like types.
            /// </summary>
            public class Dictionary : Typed<T>
            {
                private readonly Type valueType;
                private readonly Type kind;
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                public Dictionary(XmlSerializer parent) : base(parent)
                {
                    var (keyType, valueType) = typeof(T).GetGenerics2(typeof(IDictionary<,>));
                    if (keyType == null || keyType != typeof(string) || valueType == null)
                        throw new ArgumentException("Generic type is not a proper dictionary");
                    kind = typeof(T).IsGenericType ? typeof(T).GetGenericTypeDefinition() : typeof(T);
                    this.valueType = valueType;
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);

                private Func<XElement, T> MakeDeserializer()
                {
                    var input = Ex.Parameter(typeof(XElement), "input");
                    var @class = kind == typeof(IDictionary<,>) || kind == typeof(IImmutableDictionary<,>) || kind == typeof(ImmutableDictionary<,>)
                        ? typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType)
                        : typeof(T);
                    var result = Ex.Parameter(@class, "result");
                    var elements = Ex.Parameter(typeof(IEnumerable<XElement>), "elements");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    if (valueType == typeof(object))
                    {
                        var block = Ex.Block(new[] { result, elements },
                            Ex.Assign(result, Ex.New(@class)),
                            Ex.Assign(elements, Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes)),
                            elements.Foreach(current =>
                                Ex.Call(result, nameof(Dictionary<string, object>.Add), Type.EmptyTypes,
                                    Ex.Property(Ex.Property(current, nameof(XElement.Name)), nameof(XName.LocalName)),
                                    Ex.Condition(Ex.Property(current, nameof(XElement.HasElements)),
                                        Ex.Convert(Ex.Call(Ex.Constant(this), nameof(Deserialize), Type.EmptyTypes, current), typeof(object)),
                                        Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                            current, Ex.Constant(typeof(string))),
                                            typeof(object))))),
                            Ex.Convert(
                                kind == typeof(IImmutableDictionary<,>) || kind == typeof(ImmutableDictionary<,>)
                                    ? (Ex)Ex.Call(typeof(ImmutableDictionary), nameof(ImmutableDictionary.ToImmutableDictionary), new[] { typeof(string), valueType }, result)
                                    : result,
                                typeof(T)));
                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    else
                    {
                        var block = Ex.Block(new[] { result, elements },
                            Ex.Assign(result, Ex.New(@class)),
                            Ex.Assign(elements, Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes)),
                            elements.Foreach(current =>
                                Ex.Call(result, nameof(Dictionary<string, object>.Add), Type.EmptyTypes,
                                    Ex.Property(Ex.Property(current, nameof(XElement.Name)), nameof(XName.LocalName)),
                                    Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), new[] { valueType },
                                        current))),
                            Ex.Convert(
                                kind == typeof(IImmutableDictionary<,>) || kind == typeof(ImmutableDictionary<,>)
                                    ? (Ex)Ex.Call(typeof(ImmutableDictionary), nameof(ImmutableDictionary.ToImmutableDictionary), new[] { typeof(string), valueType }, result)
                                    : result,
                                typeof(T)));

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                }

                private Func<T, XElement> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var kvp = Ex.Parameter(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), valueType), "kvp");
                    var enumerator = Ex.Parameter(typeof(IEnumerator<>).MakeGenericType(kvp.Type), "enumerator");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    var ctor = typeof(XElement).GetConstructor(new[] { typeof(XName), typeof(object) });
                    var innerlambda = Ex.Lambda(
                        Ex.New(ctor,
                            Ex.Convert(Ex.Property(kvp, nameof(KeyValuePair<string, object>.Key)), typeof(XName)),
                            Ex.Call(typeof(Utils).GetMethod(nameof(Utils.GetContent)),
                                Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                    Ex.Convert(Ex.Property(kvp, nameof(KeyValuePair<string, object>.Value)), typeof(object))))), kvp);

                    var block = Ex.New(ctor,
                        Ex.Convert(Ex.Constant(StripName(typeof(T).Name)), typeof(XName)),
                        Ex.Convert(
                            Ex.Call(typeof(Enumerable).GetMethods()
                                .Where(m => m.Name == nameof(Enumerable.Select)
                                    && m.GetGenericArguments().Length == 2
                                    && m.GetParameters().Length == 2
                                    && m.GetParameters()[1].ParameterType == typeof(Func<,>).MakeGenericType(m.GetGenericArguments()[0], m.GetGenericArguments()[1]))
                                .First().MakeGenericMethod(kvp.Type, typeof(XElement)),
                                Ex.Convert(input, typeof(IEnumerable<>).MakeGenericType(kvp.Type)),
                                innerlambda), typeof(object)));
                    var lambda = Ex.Lambda<Func<T, XElement>>(block, input);

                    return lambda.Compile();
                }
            }
        }

    }
}
