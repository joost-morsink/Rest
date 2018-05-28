using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for unions.
    /// A union type is an abstract class containing nested 'case' classes that derive from the abstract class.
    /// </summary>
    public class UnionDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static UnionDescriptorKind Instance { get; } = new UnionDescriptorKind();
        private UnionDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for a union type.
        /// A union type is an abstract class containing nested 'case' classes that derive from the abstract class.
        /// This method returns null if the context does not represent a union tyoe.
        /// </summary>
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
