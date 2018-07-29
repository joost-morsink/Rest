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
        private IEnumerable<Type> GetNestedTypes(Type type)
        {
            var generics = type.GetGenericArguments();
            if (generics.Length == 0)
                return type.GetNestedTypes();
            else
                return type.GetNestedTypes().Select(nt => nt.MakeGenericType(generics));
        }
        /// <summary>
        /// Gets a type descriptor for a union type.
        /// A union type is an abstract class containing nested 'case' classes that derive from the abstract class.
        /// This method returns null if the context does not represent a union tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (!IsOfKind(context.Type))
                return null;
            var ti = context.Type.GetTypeInfo();
            var rec = RecordDescriptorKind.Instance.GetDescriptor(creator, context);
            var options = from ty in GetNestedTypes(ti)
                          where ty.BaseType == context.Type
                          select creator.GetReferableDescriptor(context.WithType(ty).WithCutoff(context.Type));
            TypeDescriptor res = new TypeDescriptor.Union(
                rec == null ? context.Type.ToString() : "",
                options,
                rec == null ? context.Type : null);

            if (rec != null)
                res = new TypeDescriptor.Intersection(context.Type.ToString(), new[] { rec, res }, context.Type);

            return res;
        }

        public bool IsOfKind(Type type)
        {
            var ti = type.GetTypeInfo();
            return ti.IsAbstract && GetNestedTypes(ti).Any(nt => nt.BaseType == type);
        }
    }
}
