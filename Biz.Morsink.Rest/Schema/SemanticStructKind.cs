using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
        {
            var ti = type.GetTypeInfo();
            var ctors = ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.IsPublic).ToArray();

            if (ti.IsValueType && ctors.Length == 1 && ctors[0].GetParameters().Length == 1)
            {
                var param = ctors[0].GetParameters()[0];
                var props = ti.DeclaredProperties.Where(p => p.GetMethod != null && !p.GetMethod.IsStatic && p.GetMethod.IsPublic).ToArray();
                if (props.Length == 1 && props[0].CanRead && !props[0].CanWrite && props[0].PropertyType == param.ParameterType)
                {
                    return param.ParameterType;
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
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var t = GetUnderlyingType(context.Type);
            return t == null ? null : creator.GetDescriptor(context.WithType(t).WithCutoff(null));
        }
        /// <summary>
        /// A semantic struct type is a value type with a constructor with a single parameter and a single property of the same type.
        /// </summary>
        public bool IsOfKind(Type type)
            => GetUnderlyingType(type) != null;
    }
}
