using System;
using System.Reflection;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for nullable types.
    /// A nullable type is Nullable&lt;T&gt; for some T.
    /// </summary>
    public class NullableDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static NullableDescriptorKind Instance { get; } = new NullableDescriptorKind();
        private NullableDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for a nullable type.
        /// A nullable type is Nullable&lt;T&gt; for some T.
        /// This method returns null if the context does not represent a nullable tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var ti = context.Type.GetTypeInfo();
            var ga = ti.GetGenericArguments();
            if (ga.Length == 1)
            {
                if (ti.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var t = creator.GetReferableDescriptor(context.WithType(ga[0]));
                    return t == null ? null : new TypeDescriptor.Union(t.ToString() + "?", new TypeDescriptor[] { t, TypeDescriptor.Null.Instance });
                }
            }
            return null;
        }
    }
}
