using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            /// Default implementation for typed HalSerializers.
            /// Assumes a record like structure.
            /// Supports both mutable and immutable classes.
            /// </summary>
            public class Default : Typed<T>
            {
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public Default(HalSerializer parent)
                    : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);
                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);


                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var parameterlessConstructor = typeof(T).GetTypeInfo().GetConstructor(Type.EmptyTypes);
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(JToken), "input");

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
                            Ex.Convert(Ex.Call(input, nameof(JToken.Children), new[] { typeof(JProperty) }), typeof(IEnumerable<JProperty>)).Foreach(current =>
                                Ex.Switch(Ex.Call(Ex.Property(current, nameof(JProperty.Name)), nameof(string.ToUpperInvariant), Type.EmptyTypes),

                                    props.Select(prop =>
                                        Ex.SwitchCase(
                                            Ex.Block(
                                                Ex.Assign(
                                                    Ex.Property(result, prop),
                                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { prop.PropertyType }, ctx, Ex.Property(current, nameof(JProperty.Value)))),
                                                Ex.Default(typeof(void))),
                                            Ex.Constant(prop.Name.ToUpperInvariant()))).ToArray())),
                            result);

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                    else
                    {
                        var ctor = typeof(T).GetConstructors().Where(c => c.IsPublic).OrderByDescending(c => c.GetParameters().Length).First();
                        var parameters = ctor.GetParameters().Select(p => Ex.Parameter(p.ParameterType, p.Name.ToUpperInvariant())).ToArray();
                        var block = Ex.Block(parameters,
                            Ex.Convert(Ex.Call(input, nameof(JToken.Children), new[] { typeof(JProperty) }), typeof(IEnumerable<JProperty>)).Foreach(current =>
                                Ex.Switch(
                                    Ex.Call(Ex.Property(current, nameof(JProperty.Name)), nameof(string.ToUpperInvariant), Type.EmptyTypes),
                                    parameters.Select(par =>
                                        Ex.SwitchCase(
                                            Ex.Block(
                                                Ex.Assign(par,
                                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { par.Type },
                                                        ctx, Ex.Property(current, nameof(JProperty.Value)))),
                                                Ex.Default(typeof(void))),
                                            Ex.Constant(par.Name.ToUpperInvariant())))
                                        .ToArray())),
                            Ex.New(ctor, parameters));

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");

                    var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                         .TakeWhile(x => x != null)
                         .SelectMany(x => x.DeclaredProperties)
                         .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                         .GroupBy(x => x.Name)
                         .Select(x => x.First())
                         .ToArray();
                    var serializeMethod = typeof(HalSerializer).GetMethods()
                        .Where(m => m.Name == nameof(HalSerializer.Serialize)
                            && m.ContainsGenericParameters
                            && m.GetGenericArguments().Length == 1
                            && m.GetParameters().Length == 2)
                        .First();

                    var block = Ex.New(typeof(JObject).GetConstructor(new[] { typeof(object[]) }),
                        Ex.NewArrayInit(typeof(object),
                            props.Select(prop =>
                                Ex.New(typeof(JProperty).GetConstructor(new[] { typeof(string), typeof(object) }),
                                    Ex.Constant(prop.Name.CasedToCamelCase()),
                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), Type.EmptyTypes, Ex.Constant(prop.PropertyType), ctx, Ex.Convert(Ex.Property(input, prop), typeof(object)))))));

                    var lambda = Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input);
                    return lambda.Compile();
                }

            }
        }
    }
}
