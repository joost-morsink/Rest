using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biz.Morsink.Identity;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// The Schema repository maps identity values for Types to TypeDescriptors.
    /// </summary>
    public class SchemaRepository : RestRepository<TypeDescriptor>, IRestGet<TypeDescriptor, NoParameters>
    {
        /// <summary>
        /// Gets a TypeDescriptor for some reference.
        /// </summary>
        /// <param name="id">The identity value of the TypeDescriptor.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous RestResponse that may contain a TypeDescriptor.</returns>
        public ValueTask<RestResponse<TypeDescriptor>> Get(IIdentity<TypeDescriptor> id, NoParameters parameters)
        {
            if (id.Value is Type type)
            {
                var desc = type.GetDescriptor();
                return desc == null
                    ? RestResult.NotFound<TypeDescriptor>().ToResponseAsync()
                    : Rest.Value(desc).ToResponseAsync();
            }
            else
            {
                var desc = TypeDescriptorCreator.GetDescriptorByName(id.Value?.ToString());
                return desc == null
                    ? RestResult.NotFound<TypeDescriptor>().ToResponseAsync()
                    : Rest.Value(desc).ToResponseAsync();
            }
        }
    }
}
