using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    public class RepresentableDescriptorKind : TypeDescriptorCreator.IKind
    {

        private readonly IEnumerable<ITypeRepresentation> representations;

        public RepresentableDescriptorKind(IEnumerable<ITypeRepresentation> representations)
        {
            this.representations = representations ?? Enumerable.Empty<ITypeRepresentation>();
        }
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var repr = representations.Where(r => r.IsRepresentable(context.Type)).Select(r => r.GetRepresentationType(context.Type)).FirstOrDefault();
            if (repr == null)
                return null;
            else
                return creator.GetDescriptor(context.WithType(repr).WithCutoff(null));
        }

        public bool IsOfKind(Type type)
            => representations.Any(repr => repr.IsRepresentable(type));

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
        {
            var typeRep = representations.FirstOrDefault(repr => repr.IsRepresentable(type));
            if (typeRep == null)
                return null;

            throw new NotImplementedException();
        }
        private class SerializerImpl<C, T> : Serializer<C>.Typed<T>
            where C : SerializationContext<C>
        {
            private readonly ITypeRepresentation representation;
            private readonly Type representationType;

            public SerializerImpl(Serializer<C> parent, ITypeRepresentation representation) : base(parent)
            {
                this.representation = representation;
                representationType = representation.GetRepresentationType(typeof(T));
            }

            public override T Deserialize(C context, SItem item)
            {
                var repr = Parent.Deserialize(context, representationType, item);
                return (T)representation.GetRepresentable(repr);
            }
            public override SItem Serialize(C context, T item)
            {
                var repr = representation.GetRepresentation(item);
                return Parent.Serialize(context, representationType, repr);
            }
        }
    }
}
