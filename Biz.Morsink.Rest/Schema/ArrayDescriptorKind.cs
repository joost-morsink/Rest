using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class ArrayDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static ArrayDescriptorKind Instance { get; } = new ArrayDescriptorKind();
        private ArrayDescriptorKind() { }
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(context.Type.GetTypeInfo()))
            {
                var q = from itf in context.Type.GetTypeInfo().ImplementedInterfaces.Concat(new[] { context.Type })
                        let iti = itf.GetTypeInfo()
                        let ga = iti.GetGenericArguments()
                        where ga.Length == 1 && iti.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        select ga[0];
                var inner = creator.GetReferableDescriptor(context.WithType(q.FirstOrDefault() ?? typeof(object)).WithCutoff(null));
                return new TypeDescriptor.Array(inner);
            }
            else
                return null;
        }
    }
}
