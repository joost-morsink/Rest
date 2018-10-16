using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A TypeDescriptorCreator that decorates another TypeDescriptorCreator with extra type representations.
    /// </summary>
    public class DecoratedTypeDescriptorCreator : ITypeDescriptorCreator
    {
        private readonly ITypeDescriptorCreator inner;
        private readonly Lazy<RepresentableDescriptorKind> representableDescriptorKind;
        private List<ITypeRepresentation> representations;
        private readonly ConcurrentDictionary<Type, TypeDescriptor> byType;
        private readonly ConcurrentDictionary<string, TypeDescriptor> byName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inner">The TypeDescriptorCreator to decorate.</param>
        public DecoratedTypeDescriptorCreator(ITypeDescriptorCreator inner)
        {
            representations = new List<ITypeRepresentation>();
            this.inner = inner;
            representableDescriptorKind = new Lazy<RepresentableDescriptorKind>(() =>
            {
                var res = new RepresentableDescriptorKind(representations);
                representations = null;
                return res;
            });
            byType = new ConcurrentDictionary<Type, TypeDescriptor>();
            byName = new ConcurrentDictionary<string, TypeDescriptor>();
        }
        /// <summary>
        /// Add a type representation.
        /// </summary>
        /// <param name="typeRepresentation">The type representation to add.</param>
        /// <returns>This.</returns>
        public DecoratedTypeDescriptorCreator Decorate(ITypeRepresentation typeRepresentation)
        {
            representations.Add(typeRepresentation);
            return this;
        }
        /// <summary>
        /// Adds a range of type representations.
        /// </summary>
        /// <param name="typeRepresentations">The type representations to add.</param>
        /// <returns>This.</returns>
        public DecoratedTypeDescriptorCreator Decorate(IEnumerable<ITypeRepresentation> typeRepresentations)
        {
            representations.AddRange(typeRepresentations);
            return this;
        }
        /// <summary>
        /// Adds a range of type representations.
        /// </summary>
        /// <param name="typeRepresentation">A function generating the type representaions to add.</param>
        /// <returns>This.</returns>
        public DecoratedTypeDescriptorCreator Decorate(Func<DecoratedTypeDescriptorCreator, IEnumerable<ITypeRepresentation>> typeRepresentation)
        {
            representations.AddRange(typeRepresentation(this));
            return this;
        }
        public Serializer<C>.IForType CreateSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
        {
            var specificSerializer = representableDescriptorKind.Value.GetSerializer(serializer, type);
            return specificSerializer ?? inner.CreateSerializer(serializer, type);
        }

        public TypeDescriptor GetDescriptor(TypeDescriptorCreator.Context context)
            => byType.GetOrAdd(context.Type, ty =>
            {
                var desc = representableDescriptorKind.Value.GetDescriptor(this, context) ?? inner.GetDescriptor(context);
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
            => inner.GetTypeName(type);
    }
}
