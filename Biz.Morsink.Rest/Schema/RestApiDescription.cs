using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class RestApiDescription
    {
        public RestApiDescription(IEnumerable<IRestRepository> repositories, TypeDescriptorCreator typeDescriptorCreator)
        {
            EntityTypes = repositories.SelectMany(r => r.GetCapabilities()).ToLookup(c => c.EntityType);
            repositories.SelectMany(r => r.GetCapabilities())
                .SelectMany(c => new[] { c.EntityType, c.ParameterType, c.ResultType })
                .Distinct()
                .ToDictionary(t => t, t => typeDescriptorCreator.GetDescriptor(t));
        }
        public ILookup<Type, RestCapabilityDescriptor> EntityTypes { get; }
        public IReadOnlyDictionary<Type, TypeDescriptor> TypeDescriptors { get; }

    }
}
