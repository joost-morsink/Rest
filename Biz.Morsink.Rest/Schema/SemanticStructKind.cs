using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Biz.Morsink.Rest.Serialization;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for semantic structs. 
    /// Semantic structs are structs for a single value. 
    /// The only thing they add is type information for the programming environment.
    /// </summary>
    public class SemanticStructKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Gets the underlying value type for a semantic struct.
        /// </summary>
        /// <param name="type">The type of a semantic struct.</param>
        /// <returns>The underlying type if the specified type is a semantic struct, null otherwise.</returns>
        public static Type GetUnderlyingType(Type type)
            => GetUnderlyingProperty(type)?.PropertyType;

        public static PropertyInfo GetUnderlyingProperty(Type type)
        {
            var ti = type.GetTypeInfo();
            var ctors = ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.IsPublic).ToArray();

            if (ti.IsValueType && ctors.Length == 1 && ctors[0].GetParameters().Length == 1)
            {
                var param = ctors[0].GetParameters()[0];
                var props = ti.DeclaredProperties.Where(p => p.GetMethod != null && !p.GetMethod.IsStatic && p.GetMethod.IsPublic).ToArray();
                if (props.Length == 1 && props[0].CanRead && !props[0].CanWrite && props[0].PropertyType == param.ParameterType)
                {
                    return props[0];
                }
            }
            return null;
        }
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static SemanticStructKind Instance { get; } = new SemanticStructKind();
        private SemanticStructKind() { }
        /// <summary>
        /// Gets a type descriptor for a semantic struct type.
        /// A semantic struct type is a value type with a constructor with a single parameter and a single property of the same type.
        /// This method returns null if the context does not represent a record tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var t = GetUnderlyingType(context.Type);
            return t == null ? null : creator.GetDescriptor(context.WithType(t).WithCutoff(null));
        }
        /// <summary>
        /// A semantic struct type is a value type with a constructor with a single parameter and a single property of the same type.
        /// </summary>
        public bool IsOfKind(Type type)
            => GetUnderlyingType(type) != null;

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
                var ci = typeof(T).GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic && !c.IsStatic).First();
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(SItem), "input");
                var block = Ex.New(ci,
                    Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { ci.GetParameters()[0].ParameterType },
                        ctx, input));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            protected override Func<C, T, SItem> MakeSerializer()
            {
                var prop = GetUnderlyingProperty(typeof(T));
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(T), "input");
                var block = Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { prop.PropertyType },
                    ctx, Ex.Property(input, prop));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
        }
    }
}
