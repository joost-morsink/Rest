using Biz.Morsink.Identity.PathProvider;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
namespace Biz.Morsink.Rest.FSharp
{

    using static FSharp.Names;
    using static FSharp.Utils;
    /// <summary>
    /// This class represents a type descriptor creator kind for F# union types.
    /// An F# union type is a well defined concept within the F# programming language.
    /// </summary>
    public class FSharpUnionDescriptorKind : TypeDescriptorCreator.IKind
    {
        private const string Some = nameof(Some);
        private const string Value = nameof(Value);
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static FSharpUnionDescriptorKind Instance { get; } = new FSharpUnionDescriptorKind();
        private FSharpUnionDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for an F# union type.
        /// An F# union type is a well defined concept within the F# programming language.
        /// This method returns null if the context does not represent an F# union tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (!IsOfKind(context.Type))
                return null;
            var opt = GetOptionKindInnerType(context.Type);
            if (opt != null)
            {
                var optDesc = creator.GetDescriptor(opt);
                return TypeDescriptor.MakeUnion($"Optional<{optDesc.Name}>", new[]
                    {
                        optDesc,
                        TypeDescriptor.Null.Instance
                    }, context.Type);
            }
            else
            {
                var utype = UnionType.Create(context.Type);
                if (utype.IsSingleValue)
                    return creator.GetDescriptor(utype.Cases.First().Value.Parameters[0].Type);
                else
                {
                    var typeDescs = utype.Cases.Values.Select(c =>
                    TypeDescriptor.MakeRecord(c.Name,
                        new[] {
                                new PropertyDescriptor<TypeDescriptor>(Tag, TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, c.Name),true)
                        }.Concat(
                            c.Parameters.Select(p => new PropertyDescriptor<TypeDescriptor>(p.Name.CasedToPascalCase(), creator.GetDescriptor(p.Type), true))
                            ), null));
                    return TypeDescriptor.MakeUnion(context.Type.ToString(), typeDescs, context.Type);
                }
            }
        }
        public static Type GetOptionKindInnerType(Type type)
            => type.Namespace == Microsoft_FSharp_Core && type.Name == FSharpOption_1
            ? type.GetGenericArguments()[0]
            : null;
        public bool IsOfKind(Type type)
            => IsFsharpUnionType(type) && !typeof(IEnumerable).IsAssignableFrom(type);

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
            ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
            : null;
        private class SerializerImpl<C, T> : Serializer<C>.Typed<T>.Func
            where C : SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> parent) : base(parent)
            {
                UnionType = UnionType.Create(GetFsharpUnionType(typeof(T)));
            }

            public UnionType UnionType { get; }

            protected override Func<C, SItem, T> MakeDeserializer()
            {
                var opt = GetOptionKindInnerType(typeof(T));
                if (opt != null)
                    return MakeOptionDeserializer(opt);
                else if (UnionType.IsSingleValue)
                    return MakeSingleValueDeserializer();
                else
                    return MakeRegularDeserializer();
            }

            private Func<C, SItem, T> MakeOptionDeserializer(Type opt)
            {
                var input = Ex.Parameter(typeof(SItem), "input");
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var svalue = Ex.Parameter(typeof(SValue), "svalue");
                var block = Ex.Block(new[] { svalue },
                    Ex.Assign(svalue, Ex.TypeAs(input, typeof(SValue))),
                    Ex.Condition(Ex.MakeBinary(Et.AndAlso,
                        Ex.MakeBinary(Et.NotEqual, svalue, Ex.Default(typeof(SValue))),
                        Ex.MakeBinary(Et.Equal, Ex.Property(svalue, nameof(SValue.Value)), Ex.Default(typeof(object)))),
                        Ex.Default(typeof(T)),
                        Ex.Call(typeof(T).GetMethod(Some, BindingFlags.Static | BindingFlags.Public),
                            Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { opt },
                                ctx, input))));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }
            private Func<C, SItem, T> MakeRegularDeserializer()
            {
                var input = Ex.Parameter(typeof(SItem), "input");
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var dict = Ex.Parameter(typeof(Dictionary<string, SItem>), "dict");
                var tag = Ex.Parameter(typeof(string), "tag");
                var dictIndexer = dict.Type.GetProperties().First(p => p.GetIndexParameters().Length > 0);
                var res = Ex.Parameter(typeof(T), "res");
                var getOrDef = typeof(SerializerImpl<C, T>).GetMethod(nameof(GetOrDefault), BindingFlags.NonPublic | BindingFlags.Instance);
                var block = Ex.Block(new[] { dict },
                    Ex.Assign(dict,
                        Ex.Call(Ex.Convert(input, typeof(SObject)), nameof(SObject.ToDictionary), Type.EmptyTypes,
                            Ex.Constant(CaseInsensitiveEqualityComparer.Instance))),
                    Ex.Assign(tag, Ex.MakeIndex(dict, dictIndexer, new[] { Ex.Constant(Tag) })),
                    Ex.Switch(Ex.Call(tag, nameof(string.ToLowerInvariant), Type.EmptyTypes),
                        UnionType.Cases.Values.Select(@case =>
                            Ex.SwitchCase(
                                Ex.Assign(res,
                                    Ex.Call(@case.ConstructorMethod,
                                        @case.Parameters.Select(par => Ex.Call(Ex.Constant(this), getOrDef.MakeGenericMethod(par.Type), ctx, dict, Ex.Constant(par.Name))))),
                                Ex.Constant(@case.Name.ToLowerInvariant()))).ToArray()),
                    res);
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            private Func<C, SItem, T> MakeSingleValueDeserializer()
            {
                var input = Ex.Parameter(typeof(SItem), "input");
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var @case = UnionType.Cases.Values.First();
                var block = Ex.Call(@case.ConstructorMethod,
                    Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { @case.Parameters[0].Type },
                        ctx, input));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            private X GetOrDefault<X>(C context, Dictionary<string, SItem> dict, string key)
            {
                if (dict.TryGetValue(key, out var item))
                    return Parent.Deserialize<X>(context, item);
                else
                    return default;
            }
            protected override Func<C, T, SItem> MakeSerializer()
            {
                var opt = GetOptionKindInnerType(typeof(T));
                if (opt != null)
                    return MakeOptionSerializer(opt);
                else if (UnionType.IsSingleValue)
                    return MakeSingleCaseSerializer();
                else
                    return MakeRegularSerializer();
            }
            private Func<C, T, SItem> MakeOptionSerializer(Type opt)
            {
                var input = Ex.Parameter(typeof(T), "input");
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var block = Ex.Condition(
                    Ex.MakeBinary(Et.Equal, input, Ex.Default(typeof(T))),
                    Ex.Constant(SValue.Null, typeof(SItem)),
                    Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { opt }, ctx, Ex.Property(input, Value)));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
            private Func<C, T, SItem> MakeRegularSerializer()
            {
                var input = Ex.Parameter(typeof(T), "input");
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var tag = Ex.Parameter(typeof(int), "tag");
                var res = Ex.Parameter(typeof(List<SProperty>), "res");
                var newSprop = typeof(SProperty).GetConstructor(new[] { typeof(string), typeof(SItem) });
                var newSvalue = typeof(SValue).GetConstructor(new[] { typeof(object) });
                var newSObject = typeof(SObject).GetConstructor(new[] { typeof(IEnumerable<SProperty>) });
                var block = Ex.Block(new[] { tag, res },
                    Ex.Assign(tag, Ex.Property(input, Tag)),
                    Ex.Assign(res, Ex.New(res.Type)),
                    Ex.Switch(tag, UnionType.Cases.Select(@case =>
                        Ex.SwitchCase(
                            Ex.Block(
                                Ex.Call(res, nameof(List<SProperty>.Add), Type.EmptyTypes,
                                    Ex.New(newSprop, Ex.Constant(Tag), Ex.New(newSvalue, Ex.Constant(@case.Value.Name)))),
                                Ex.Block(@case.Value.Parameters.Select(par =>
                                    Ex.Call(res, nameof(List<SProperty>.Add), Type.EmptyTypes,
                                        Ex.New(newSprop, Ex.Constant(par.Name),
                                            Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { par.Type },
                                                ctx, Ex.Property(input, par.Property))))))),
                            Ex.Constant(@case.Key))).ToArray()),
                    Ex.New(newSObject, res));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }

            private Func<C, T, SItem> MakeSingleCaseSerializer()
            {
                var input = Ex.Parameter(typeof(T), "input");
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var prop = UnionType.Cases.Values.First().Parameters[0].Property;
                var block = Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { prop.PropertyType },
                    ctx, Ex.Property(input, prop));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
        }
    }
}
