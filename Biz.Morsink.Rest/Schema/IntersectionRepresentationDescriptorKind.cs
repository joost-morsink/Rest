using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biz.Morsink.Identity.PathProvider;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    public class IntersectionRepresentationDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static IntersectionRepresentationDescriptorKind Instance { get; } = new IntersectionRepresentationDescriptorKind();
        public static bool IsOfKind(Type type)
            => typeof(IntersectionRepresentation).IsAssignableFrom(type);

        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (!IsOfKind(context.Type))
                return null;
            var typeParams = IntersectionRepresentation.GetTypeParameters(context.Type);
            return TypeDescriptor.MakeIntersection(context.Type.Name, typeParams.Select(tp => creator.GetDescriptor(tp)), context.Type);
        }

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type)
            where C : SerializationContext<C>
            => IsOfKind(type)
                ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
                : null;

        bool TypeDescriptorCreator.IKind.IsOfKind(Type type)
            => IsOfKind(type);
        private class SerializerImpl<C, T> : Serializer<C>.Typed<T>
            where C : SerializationContext<C>
            where T : IntersectionRepresentation
        {
            public SerializerImpl(Serializer<C> parent) : base(parent)
            {
            }

            public override T Deserialize(C context, SItem item)
            {
                return (T)IntersectionRepresentation.GetTypeParameters(typeof(T))
                    .Select(type => (type, value: Parent.Deserialize(context, type, item)))
                    .Aggregate(IntersectionRepresentation.Create(), (ir, t) => ir.Add(t.type, t.value))
                    .Create();
            }

            public override SItem Serialize(C context, T item)
            {
                var props = item.GetTypes().Zip(item.GetValues(), (type, value) => Parent.Serialize(context, type, value))
                     .OfType<SObject>()
                     .SelectMany(o => o.Properties)
                     .GroupBy(p => p.Name, CaseInsensitiveEqualityComparer.Instance)
                     .Select(g => g.First());
                return new SObject(props);
            }
        }
    }
}
