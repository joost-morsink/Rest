using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class UnionDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static UnionDescriptorKind Instance { get; } = new UnionDescriptorKind();
        private UnionDescriptorKind() { }
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var ti = context.Type.GetTypeInfo();
            if (ti.IsAbstract && ti.DeclaredNestedTypes.Any(nt => nt.BaseType == context.Type))
            {
                var rec = RecordDescriptorKind.Instance.GetDescriptor(creator, context);
                TypeDescriptor res = new TypeDescriptor.Union(rec == null ? context.Type.ToString() : "", ti.DeclaredNestedTypes.Where(nt => nt.BaseType == context.Type && nt.IsPublic).Select(ty => creator.GetReferableDescriptor(context.WithType(ty).WithCutoff(context.Type))));

                if (rec != null)
                    res = new TypeDescriptor.Intersection(context.Type.ToString(), new[] { rec, res });

                return res;
            }
            else
                return null;
        }
    }
}
