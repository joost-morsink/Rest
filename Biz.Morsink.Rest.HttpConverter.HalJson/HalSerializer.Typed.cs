using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Utils;
using Newtonsoft.Json.Linq;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {
        private static string StripName(string name)
        {
            if (name.Contains('`'))
                return name.Substring(0, name.IndexOf('`'));
            else
                return name;
        }

        public abstract class Typed<T> : IForType
        {
            protected Typed(HalSerializer parent)
            {
                Parent = parent;
            }

            public abstract JToken Serialize(HalContext context, T item);
            public abstract T Deserialize(HalContext context, JToken token);

            Type IForType.Type => typeof(T);
            JToken IForType.Serialize(HalContext context, object item) => Serialize(context, (T)item);
            object IForType.Deserialize(HalContext context, JToken token) => Deserialize(context, token);

            protected HalSerializer Parent { get; }

            public class Simple : Typed<T>
            {
                private readonly IDataConverter converter;

                public Simple(HalSerializer parent, IDataConverter converter)
                    : base(parent)
                {
                    this.converter = converter;
                }

                public override T Deserialize(HalContext context, JToken token)
                    => token.Value<T>();

                public override JToken Serialize(HalContext context, T item)
                    => new JValue(item);
            }

            public class Nullable : Typed<T>
            {
                private readonly Type valueType;
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;
                
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
                    return Ex.Lambda<Func<HalContext,JToken, T>>(block, ctx, input).Compile();
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");

                    var block = Ex.Condition(
                        Ex.Property(input, nameof(Nullable<int>.HasValue)),
                        Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), new[] { valueType }, ctx, Ex.Property(input, nameof(Nullable<int>.Value))),
                        Ex.Constant(JValue.CreateNull()));
                    return Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input).Compile();
                }
            }
            public class Default : Typed<T>
            {
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;

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
                    var ctx = Ex.Parameter(typeof(T), "ctx");
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
                                Ex.Switch(Ex.Call(Ex.Property(current, nameof(JProperty.Name)),nameof(string.ToUpperInvariant), Type.EmptyTypes),

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
                        throw new NotSupportedException();
                    }
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var ctx = Ex.Parameter(typeof(T), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");

                    var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                         .TakeWhile(x => x != null)
                         .SelectMany(x => x.DeclaredProperties)
                         .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                         .GroupBy(x => x.Name)
                         .Select(x => x.First())
                         .ToArray();

                    var block = Ex.New(typeof(JObject).GetConstructor(new[] { typeof(object[]) }),
                        Ex.NewArrayInit(typeof(object),
                            props.Select(prop =>
                                Ex.New(typeof(JProperty).GetConstructor(new[] { typeof(string), typeof(object) }),
                                    Ex.Constant(prop.Name.CasedToCamelCase()),
                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), new[] { prop.PropertyType }, ctx, Ex.Property(input, prop))))));

                    var lambda = Ex.Lambda<Func<HalContext,T,JToken>>(block, ctx, input);
                    return lambda.Compile();
                }

            }
        }
    }
}
