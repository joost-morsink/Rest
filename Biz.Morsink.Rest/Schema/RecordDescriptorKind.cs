
using Biz.Morsink.Identity.PathProvider;
using Biz.Morsink.Rest.Serialization;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for records.
    /// A record type is a type with either a parameterless constructor and a bunch of properties with getters and setters, or a type with a single constructor and for each constructor parameter a readonly property.
    /// </summary>
    public class RecordDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static RecordDescriptorKind Instance { get; } = new RecordDescriptorKind();
        private RecordDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for a record type.
        /// A record type is a type with either a parameterless constructor and a bunch of properties with getters and setters, or a type with a single constructor and for each constructor parameter a readonly property.
        /// This method returns null if the context does not represent a record tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var ti = context.Type.GetTypeInfo();

            if (HasParameterlessConstructor(context.Type))
            {
                var props = GetWriteableProperties(ti)
                    .Select(x => new PropertyDescriptor<TypeDescriptor>(x.Name, creator.GetReferableDescriptor(context.WithType(x.PropertyType).WithCutoff(null)), x.GetCustomAttributes<RequiredAttribute>().Any()));

                return props.Any()
                    ? new TypeDescriptor.Record(context.Type.ToString(), props, context.Type)
                    : null;
            }
            else
            {
                var props = GetReadonlyProperties(ti, context.Cutoff).ToArray();
                if (!props.All(pi => pi.CanRead && !pi.CanWrite))
                    return null;
                var properties = GetConstructorProperties(ti)
                    .Select(g => g.Select(p => new PropertyDescriptor<TypeDescriptor>(
                        p.Item1.Name,
                        creator.GetReferableDescriptor(context.WithType(p.Item1.PropertyType).WithCutoff(null)),
                        !p.Item2.GetCustomAttributes<OptionalAttribute>().Any())))
                    .FirstOrDefault();

                return properties?.Any() == true ? new TypeDescriptor.Record(context.Type.ToString(), properties, context.Type) : null;
            }
        }

        public static IEnumerable<IGrouping<ConstructorInfo, (PropertyInfo, ParameterInfo)>> GetConstructorProperties(Type type)
        {
            var props = GetReadonlyProperties(type).ToArray();
            return from ci in type.GetTypeInfo().DeclaredConstructors
                   let ps = ci.GetParameters()
                   where !ci.IsStatic && ps.Length > 0 && ps.Length >= props.Length
                       && ps.Join(props, p => p.Name, p => p.Name, (_, __) => 1, CaseInsensitiveEqualityComparer.Instance).Count() == props.Length
                   from p in ps.Join(props, p => p.Name, p => p.Name,
                       (par, prop) => (par, prop, ci),
                       CaseInsensitiveEqualityComparer.Instance)
                   group (p.prop, p.par) by p.ci;
        }
        public static IEnumerable<PropertyInfo> GetReadonlyProperties(Type type, Type cutoff = null)
            => type.GetTypeInfo()
                .Iterate(x => x.BaseType?.GetTypeInfo())
                .TakeWhile(x => x != cutoff && x != null)
                .SelectMany(x => x.DeclaredProperties)
                .Where(p => p.CanRead && !p.CanWrite && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                .GroupBy(x => x.Name)
                .Select(x => x.First());


        public static IEnumerable<PropertyInfo> GetWriteableProperties(Type ti)
        {
            return ti.GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                   .TakeWhile(x => x != null)
                   .SelectMany(x => x.DeclaredProperties)
                   .Where(p => p.CanRead && p.CanWrite && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                   .GroupBy(x => x.Name)
                   .Select(x => x.First());
        }
        public static IEnumerable<PropertyInfo> GetReadableProperties(Type ti)
        {
            return ti.GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                   .TakeWhile(x => x != null)
                   .SelectMany(x => x.DeclaredProperties)
                   .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                   .GroupBy(x => x.Name)
                   .Select(x => x.First());
        }

        public static bool HasParameterlessConstructor(Type type)
            => type.GetTypeInfo().DeclaredConstructors.Where(ci => !ci.IsStatic && ci.GetParameters().Length == 0).Any();
        /// <summary>
        /// A record type is a type with either a parameterless constructor and a bunch of properties with getters and setters, or a type with a single constructor and for each constructor parameter a readonly property.
        /// </summary>
        public bool IsOfKind(Type type)
        {
            var ti = type.GetTypeInfo();

            if (HasParameterlessConstructor(type))
                return IsOfKindMutable(type);
            else
                return IsOfKindImmutable(type);
        }

        public static bool IsOfKindImmutable(Type type)
            => GetConstructorProperties(type).Any();

        public static bool IsOfKindMutable(Type type)
            => GetWriteableProperties(type).Any();


        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
            ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
            : null;
        private class SerializerImpl<C, T> : Serializer<C>.Typed<T>.Func
            where C : SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> parent) : base(parent) { }
            protected override Func<C, SItem, T> MakeDeserializer()
            {
                if (IsOfKindMutable(typeof(T)))
                    return MakeMutableDeserializer();
                else if (IsOfKindImmutable(typeof(T)))
                    return MakeImmutableDeserializer();
                else
                    throw new NotSupportedException();
            }

            private Func<C, SItem, T> MakeImmutableDeserializer()
            {
                var x = GetConstructorProperties(typeof(T)).First();
                var ci = x.Key;
                var props = x.AsEnumerable();
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(SItem), "item");
                var obj = Ex.Parameter(typeof(SObject), "obj");
                var parameters = props.Select(t => Ex.Parameter(t.Item2.ParameterType, $"p{t.Item1.Name}")).ToArray();
                var block = Ex.Block(parameters.Prepend(obj),
                    Ex.Assign(obj, Ex.Convert(input, typeof(SObject))),
                    Ex.Property(obj, nameof(SObject.Properties)).Foreach(prop =>
                        Ex.Switch(Ex.Call(Ex.Property(prop, nameof(SProperty.Name)), nameof(string.ToLower), Type.EmptyTypes),
                            props.Select((p, idx) =>
                                Ex.SwitchCase(
                                    Ex.Block(
                                        Ex.Assign(parameters[idx],
                                            Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { parameters[idx].Type },
                                                ctx, Ex.Property(prop, nameof(SProperty.Token)))),
                                        Ex.Default(typeof(void))),
                                    Ex.Constant(p.Item1.Name.ToLower()))).ToArray())),
                    Ex.New(ci, parameters));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            private Func<C, SItem, T> MakeMutableDeserializer()
            {
                var props = GetWriteableProperties(typeof(T));
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(SItem), "input");
                var obj = Ex.Parameter(typeof(SObject), "obj");
                var result = Ex.Parameter(typeof(T), "result");
                var block = Ex.Block(new[] { obj, result },
                    Ex.Assign(result, Ex.New(typeof(T))),
                    Ex.Assign(obj, Ex.Convert(input, typeof(SObject))),
                    Ex.Property(obj, nameof(SObject.Properties)).Foreach(prop =>
                        Ex.Switch(Ex.Call(Ex.Property(prop, nameof(SProperty.Name)), nameof(string.ToLower), Type.EmptyTypes),
                            props.Select(p =>
                                Ex.SwitchCase(
                                    Ex.Block(
                                        Ex.Assign(Ex.Property(result, p),
                                            Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { p.PropertyType },
                                                ctx, Ex.Property(prop, nameof(SProperty.Token)))),
                                        Ex.Default(typeof(void))),
                                    Ex.Constant(p.Name.ToLower()))).ToArray())),
                    result);
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            protected override Func<C, T, SItem> MakeSerializer()
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(T), "input");
                var props = Ex.Parameter(typeof(List<SProperty>), "props");
                var block = Ex.Block(new[] { props },
                    Ex.Assign(props, Ex.New(typeof(List<SProperty>))),
                    Ex.Block(GetReadableProperties(typeof(T)).Select(prop =>
                        Ex.Call(props, nameof(List<SProperty>.Add), Type.EmptyTypes,
                            Ex.New(typeof(SProperty).GetConstructor(new[] { typeof(string), typeof(SItem) }),
                                Ex.Constant(prop.Name),
                                Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { prop.PropertyType },
                                    ctx, Ex.Property(input, prop)))))),
                    Ex.New(typeof(SObject).GetConstructor(new[] { typeof(IEnumerable<SProperty>) }), props));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
        }
    }
}
