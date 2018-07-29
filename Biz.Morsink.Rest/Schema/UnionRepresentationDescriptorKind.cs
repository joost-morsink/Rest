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
            var typeParams = GetTypes(context.Type);
            return TypeDescriptor.MakeUnion(context.Type.Name, typeParams.Select(tp => creator.GetDescriptor(tp)), context.Type);
        }

        bool TypeDescriptorCreator.IKind.IsOfKind(Type type) => IsOfKind(type);

        public static bool IsOfKind(Type type)
            => typeof(UnionRepresentation).IsAssignableFrom(type);

        public static IReadOnlyList<Type> GetTypes(Type type)
            => (IReadOnlyList<Type>)type.GetMethod(nameof(UnionRepresentation<object, object>.GetTypeParameters)).Invoke(null, null);

    }
}
