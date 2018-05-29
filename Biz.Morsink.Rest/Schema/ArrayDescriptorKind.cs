using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for collections (arrays).
    /// A collection type implements IEnumerable, and ideally IEnumerable&lt;T&gt; for some T.
    /// </summary>
    public class ArrayDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static ArrayDescriptorKind Instance { get; } = new ArrayDescriptorKind();
        private ArrayDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for a collection type.
        /// A collection type implements IEnumerable, and ideally IEnumerable&lt;T&gt; for some T.
        /// This method returns null if the context does not represent a collection tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var elementType = GetElementType(context.Type);
            if (elementType == null)
                return null;
            else
            {
                var inner = creator.GetReferableDescriptor(context.WithType(elementType).WithCutoff(null));
                return TypeDescriptor.MakeArray(inner);
            }
        }
        private static Type GetElementType(Type type)
        {
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                var q = from itf in type.GetTypeInfo().ImplementedInterfaces.Concat(new[] { type })
                        let iti = itf.GetTypeInfo()
                        let ga = iti.GetGenericArguments()
                        where ga.Length == 1 && iti.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        select ga[0];
                return q.FirstOrDefault() ?? typeof(object);
            }
            else
                return null;
        }
        /// <summary>
        /// A collection type implements IEnumerable, and ideally IEnumerable&lt;T&gt; for some T.
        /// </summary>
        public bool IsOfKind(Type type)
            => GetElementType(type) != null;
    }
}
