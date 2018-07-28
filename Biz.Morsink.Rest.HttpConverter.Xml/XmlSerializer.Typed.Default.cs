using System;
using System.Linq;
using System.Reflection;
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
            /// Default implementation for typed XmlSerializers.
            /// Assumes a 'record-like' structure.
            /// Supports both mutable and immutable classes.
            /// </summary>
            public class Default : Typed<T>
            {
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                public Default(XmlSerializer parent) : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);

                private Func<T, XElement> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                        .TakeWhile(x => x != null)
                        .SelectMany(x => x.DeclaredProperties)
                        .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                        .GroupBy(x => x.Name)
                        .Select(x => x.First())
                        .ToArray();
                    var block = Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName), typeof(object[]) }),
                        Ex.Convert(Ex.Constant(StripName(typeof(T).Name)), typeof(XName)),
                        Ex.NewArrayInit(typeof(object),
                            props.Select(prop =>
                                Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName), typeof(object) }),
                                Ex.Convert(Ex.Constant(prop.Name), typeof(XName)),
                                Ex.Call(typeof(Utils), nameof(Utils.GetContentOrNil), Type.EmptyTypes,
                                    Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                        Ex.Constant(prop.PropertyType),
                                        Ex.Convert(Ex.Property(input, prop), typeof(object))))))));
                    var lambda = Ex.Lambda(block, input);
                    return (Func<T, XElement>)lambda.Compile();
                }
                private Func<XElement, T> MakeDeserializer()
                {
                    var parameterlessConstructor = typeof(T).GetTypeInfo().GetConstructor(Type.EmptyTypes);
                    var input = Ex.Parameter(typeof(XElement), "input");


                    if (parameterlessConstructor != null)
                    {
                        var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                            .TakeWhile(x => x != null)
                            .SelectMany(x => x.DeclaredProperties)
                            .Where(p => p.CanRead && p.CanWrite && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                            .GroupBy(x => x.Name.ToUpperInvariant())
                            .Select(x => x.First())
                            .ToArray();
                        var result = Ex.Parameter(typeof(T), "result");
                        var block = Ex.Block(new[] { result },
                            Ex.Assign(result, Ex.New(parameterlessConstructor)),
                            Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes).Foreach(current =>
                                Ex.Switch(
                                    Ex.Call(Ex.Property(Ex.Property(current, nameof(XElement.Name)), nameof(XName.LocalName)), nameof(string.ToUpperInvariant), Type.EmptyTypes),
                                    props.Select(prop =>
                                        Ex.SwitchCase(
                                            Ex.Block(
                                                Ex.Assign(
                                                    Ex.Property(result, prop),
                                                    Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                                        current, Ex.Constant(prop.PropertyType)), prop.PropertyType)),
                                                Ex.Default(typeof(void))),
                                            Ex.Constant(prop.Name.ToUpperInvariant()))).ToArray())),
                            result);

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    else
                    {
                        var ctor = typeof(T).GetConstructors().Where(c => c.IsPublic).OrderByDescending(c => c.GetParameters().Length).First();
                        var parameters = ctor.GetParameters().Select(p => Ex.Parameter(p.ParameterType, p.Name.ToUpperInvariant())).ToArray();
                        var block = Ex.Block(parameters,
                            Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes).Foreach(current =>
                                Ex.Block(
                                    Ex.Switch(
                                    Ex.Call(Ex.Property(Ex.Property(current, nameof(XElement.Name)), nameof(XName.LocalName)), nameof(string.ToUpperInvariant), Type.EmptyTypes),
                                    parameters.Select(par =>
                                        Ex.SwitchCase(
                                            Ex.Block(
                                                Ex.Assign(
                                                    par,
                                                    Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                                        current, Ex.Constant(par.Type)), par.Type)),
                                                Ex.Default(typeof(void))),
                                            Ex.Constant(par.Name.ToUpperInvariant()))).ToArray()))),
                            Ex.New(ctor, parameters.ToArray()));

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                }
            }
        }

    }
}
