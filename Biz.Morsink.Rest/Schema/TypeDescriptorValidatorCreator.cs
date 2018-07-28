using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class TypeDescriptorValidatorCreator
    {
        public TypeDescriptorCreator TypeDescriptorCreator { get; }
        private readonly ITypeRepresentation[] typeRepresentations;

        public TypeDescriptorValidatorCreator(TypeDescriptorCreator typeDescriptorCreator, IEnumerable<ITypeRepresentation> typeRepresentations)
        {
            TypeDescriptorCreator = typeDescriptorCreator;
            this.typeRepresentations = typeRepresentations.ToArray();
        }
        public Validator Create(TypeDescriptor typeDescriptor)
            => new Validator(typeDescriptor, this);

        public Type GetRepresentationType(Type type)
        {
            foreach(var rep in typeRepresentations)
            {
                var repType = rep.GetRepresentationType(type);
                if (repType != null)
                    return repType;
            }
            return type;
        }
    }
}
