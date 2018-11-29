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
        /// <summary>
        /// Creates a serializer for the specified type.
        /// </summary>
        /// <typeparam name="C">The type of serialization context.</typeparam>
        /// <param name="serializer">A parent serializer/</param>
        /// <param name="type">The type to get a serializer for.</param>
        /// <returns>A serializer for the specified type if one could be constructed, null otherwise.</returns>
        public Serializer<C>.IForType CreateSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
        {
            var specificSerializer = representableDescriptorKind.Value.GetSerializer(serializer, type);
            return specificSerializer ?? inner.CreateSerializer(serializer, type);
        }
        /// <summary>
        /// Gets a TypeDescriptor for a specified TypeDescriptor creation context.
        /// </summary>
        /// <param name="context">A context/</param>
        /// <returns>A TypeDescriptor for the specified context, if one could be created, null otherwise.</returns>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator.Context context)
            => byType.GetOrAdd(context.Type, ty =>
            {
                var desc = representableDescriptorKind.Value.GetDescriptor(this, context) ?? inner.GetDescriptor(context);
                byName.AddOrUpdate(GetTypeName(ty), desc, (name, td) => td);
                return desc;
            });


        /// <summary>
        /// Gets a TypeDescriptor for a specified Type.
        /// </summary>
        /// <param name="type">The type to get a TypeDescriptor for.</param>
        /// <returns>A TypeDescriptor for the specified type.</returns>
        public TypeDescriptor GetDescriptor(Type type)
            => GetDescriptor(new TypeDescriptorCreator.Context(type));
        /// <summary>
        /// Gets a TypeDescriptor for a specified type name.
        /// </summary>
        /// <param name="name">The type's name.</param>
        /// <returns>A TypeDescriptor.</returns>
        public TypeDescriptor GetDescriptorByName(string name)
            => byName.TryGetValue(name, out var res) ? res : null;
        /// <summary>
        /// Gets a referable TypeDescriptor for a specified TypeDescriptor creation context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A TypeDescriptor if one could be constructed, null otherwise.</returns>
        public TypeDescriptor GetReferableDescriptor(TypeDescriptorCreator.Context context)
        {
            var desc = GetDescriptor(context);
            if (TypeDescriptorCreator.IsPrimitiveTypeDescriptor(desc))
                return desc;
            else
                return TypeDescriptor.Referable.Create(GetTypeName(context.Type), desc);
        }
        /// <summary>
        /// Gets the type name for some type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A name fot the specified type.</returns>
        public string GetTypeName(Type type)
            => inner.GetTypeName(type);
    }
}
