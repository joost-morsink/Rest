using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biz.Morsink.Identity;
using System.Threading.Tasks;
using Biz.Morsink.Rest.Metadata;
using System.Threading;
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// The Schema repository maps identity values for Types to TypeDescriptors.
    /// </summary>
    public class SchemaRepository : RestRepository<TypeDescriptor>, IRestGet<TypeDescriptor, Empty>
    {
        private static readonly ResponseCaching CACHING = new ResponseCaching
        {
            CacheAllowed = true,
            StoreAllowed = true,
            CachePrivate = false,
            Validity = TimeSpan.FromDays(1.0)
        };
        private readonly ITypeDescriptorCreator typeDescriptorCreator;

        public SchemaRepository(ITypeDescriptorCreator typeDescriptorCreator) {
            this.typeDescriptorCreator = typeDescriptorCreator;
        }
        /// <summary>
        /// Gets a TypeDescriptor for some reference.
        /// </summary>
        /// <param name="id">The identity value of the TypeDescriptor.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous RestResponse that may contain a TypeDescriptor.</returns>
        public ValueTask<RestResponse<TypeDescriptor>> Get(IIdentity<TypeDescriptor> id, Empty parameters, CancellationToken cancellationToken)
        {
            if (id.Value is Type type)
            {
                var desc = typeDescriptorCreator.GetDescriptor(type);
                return (desc == null
                    ? RestResult.NotFound<TypeDescriptor>().ToResponse()
                    : Rest.Value(desc).ToResponse())
                    .WithMetadata(CACHING).ToAsync();
            }
            else
            {
                var desc = typeDescriptorCreator.GetDescriptorByName(id.Value?.ToString());
                return (desc == null
                    ? RestResult.NotFound<TypeDescriptor>().ToResponse()
                    : Rest.Value(desc).ToResponse())
                    .WithMetadata(CACHING).ToAsync();
            }
        }
    }
}
