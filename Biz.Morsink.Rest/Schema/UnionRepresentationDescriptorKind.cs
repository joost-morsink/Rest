using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class UnionRepresentationDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static UnionRepresentationDescriptorKind Instance { get; } = new UnionRepresentationDescriptorKind();
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (!IsOfKind(context.Type))
                return null;
            var typeParams = (IReadOnlyList<Type>)context.Type.GetMethod(nameof(UnionRepresentation<object, object>.GetTypeParameters)).Invoke(null, null);
            return TypeDescriptor.MakeUnion(context.Type.Name, typeParams.Select(tp => creator.GetDescriptor(tp)), context.Type);
        }

        public bool IsOfKind(Type type)
            => typeof(UnionRepresentation).IsAssignableFrom(type);
    }
}
