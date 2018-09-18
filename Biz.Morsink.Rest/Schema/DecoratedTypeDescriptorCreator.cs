using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    public class DecoratedTypeDescriptorCreator : ITypeDescriptorCreator
    {
        private readonly ITypeDescriptorCreator inner;
        private readonly RepresentableDescriptorKind representableDescriptorKind;
        private readonly ConcurrentDictionary<Type, TypeDescriptor> byType;
        private readonly ConcurrentDictionary<string, TypeDescriptor> byName;

        public DecoratedTypeDescriptorCreator(ITypeDescriptorCreator inner, IEnumerable<ITypeRepresentation> typeRepresentations)
        {
            this.inner = inner;
            representableDescriptorKind = new RepresentableDescriptorKind(typeRepresentations);
            byType = new ConcurrentDictionary<Type, TypeDescriptor>();
            byName = new ConcurrentDictionary<string, TypeDescriptor>();
        }

        public Serializer<C>.IForType CreateSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
        {
            var specificSerializer = representableDescriptorKind.GetSerializer(serializer, type);
            return specificSerializer ?? inner.CreateSerializer(serializer, type);
        }

        public TypeDescriptor GetDescriptor(TypeDescriptorCreator.Context context)
            => byType.GetOrAdd(context.Type, ty =>
            {
                var desc = representableDescriptorKind.GetDescriptor(this, context) ?? inner.GetDescriptor(context);
                byName.AddOrUpdate(GetTypeName(ty), desc, (name, td) => td);
                return desc;
            });


        public TypeDescriptor GetDescriptor(Type type)
            => GetDescriptor(new TypeDescriptorCreator.Context(type));

        public TypeDescriptor GetDescriptorByName(string name)
            => byName.TryGetValue(name, out var res) ? res : null;

        public TypeDescriptor GetReferableDescriptor(TypeDescriptorCreator.Context context)
        {
            var desc = GetDescriptor(context);
            if (TypeDescriptorCreator.IsPrimitiveTypeDescriptor(desc))
                return desc;
            else
                return TypeDescriptor.Referable.Create(GetTypeName(context.Type), desc);
        }

        public string GetTypeName(Type type)
            => type.ToString().Replace('+', '.');
    }
}
