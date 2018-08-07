using Biz.Morsink.Rest.Serialization;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for unit types.
    /// A unit type has a parameterless constructor and no instance properties.
    /// </summary>
    public class UnitDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static UnitDescriptorKind Instance { get; } = new UnitDescriptorKind();
        private UnitDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for a unit type.
        /// A unit type has a parameterless constructor and no instance properties.
        /// This method returns null if the context does not represent a unit tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var ti = context.Type.GetTypeInfo();
            var parameterlessConstructors = ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.GetParameters().Length == 0);
            return parameterlessConstructors.Any()
                && !ti.Iterate(x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != context.Cutoff && x != null).SelectMany(x => x.DeclaredProperties.Where(p => !p.GetAccessors()[0].IsStatic)).Any()
                ? new TypeDescriptor.Record(context.Type.ToString(), Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>(), context.Type)
                : null;
        }
        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type)
            where C : SerializationContext<C>
            => IsOfKind(type)
                ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
                : null;

        private static ConstructorInfo GetParameterlessConstructor(TypeInfo ti)
            => ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.GetParameters().Length == 0).FirstOrDefault();

        public bool IsOfKind(Type type)
        {
            var ti = type.GetTypeInfo();
            var parameterlessConstructor = GetParameterlessConstructor(ti);
            return parameterlessConstructor != null
                && !ti.Iterate(x => x.BaseType?.GetTypeInfo())
                    .TakeWhile(x => x != null)
                    .SelectMany(x => x.DeclaredProperties.Where(p => !p.GetAccessors()[0].IsStatic))
                    .Any();
        }
        private class SerializerImpl<C, T> : Serializer<C>.Typed<T>.Func
            where C : SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> parent) : base(parent)
            {

            }
            protected override Func<C, SItem, T> MakeDeserializer()
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var item = Ex.Parameter(typeof(SItem), "item");
                var block = Ex.New(GetParameterlessConstructor(typeof(T).GetTypeInfo()));
                return Ex.Lambda<Func<C, SItem, T>>(block, ctx, item).Compile();
            }
            protected override Func<C, T, SItem> MakeSerializer()
                => (ctx, item) => new SObject();
        }
    }
}
