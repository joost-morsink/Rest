using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A container class for all metadata for a Rest api.
    /// </summary>
    public class RestApiDescription
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repositories">All Rest repositories.</param>
        /// <param name="typeDescriptorCreator">A TypeDescriptorCreator to aid in generating metadata.</param>
        public RestApiDescription(IEnumerable<IRestRepository> repositories, ITypeDescriptorCreator typeDescriptorCreator)
        {
            EntityTypes = repositories.SelectMany(r => r.GetCapabilities()).ToLookup(c => c.EntityType);
            TypeDescriptors = repositories.SelectMany(r => r.GetCapabilities())
                .SelectMany(c => new[] { c.EntityType, c.ParameterType, c.ResultType })
                .Distinct()
                .ToDictionary(t => t, t => typeDescriptorCreator.GetDescriptor(t));
        }
        /// <summary>
        /// Contains a lookup for all Rest capability descriptors belonging to a given resource type.
        public ILookup<Type, RestCapabilityDescriptor> EntityTypes { get; }
        /// <summary>
        /// Contains a dictionary for all type descriptors.
        /// </summary>
        public IReadOnlyDictionary<Type, TypeDescriptor> TypeDescriptors { get; }

    }
}
