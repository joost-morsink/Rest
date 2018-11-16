using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// Representation class for Type instances.
    /// </summary>
    public class TypeRepresentation : SimpleTypeRepresentation<Type, TypeDescriptor>
    {
        private readonly Lazy<ITypeDescriptorCreator> creator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">A service provider used to get an instance of ITypeDescriptorCreator lazily.</param>
        public TypeRepresentation(Lazy<ITypeDescriptorCreator> creator)
        {
            this.creator = creator;
        }

        public override Type GetRepresentable(TypeDescriptor representation)
            => representation.AssociatedType;

        public override TypeDescriptor GetRepresentation(Type item)
            => creator.Value.GetDescriptor(item);
    }
}
