using System;
using System.Reflection;

namespace Biz.Morsink.Rest.Schema
{
    public class NullableDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static NullableDescriptorKind Instance { get; } = new NullableDescriptorKind();
        private NullableDescriptorKind() { }
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
