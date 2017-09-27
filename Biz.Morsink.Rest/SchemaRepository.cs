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
        private readonly Dictionary<string, TypeDescriptor> data;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repositories">All repositories to get type information from.</param>
        public SchemaRepository(IEnumerable<IRestRepository> repositories)
        {
            var types = repositories.SelectMany(repo => repo.SchemaTypes).Distinct();
            data = types.ToDictionary(type => type.FullName, type => type.GetDescriptor());
        }
        /// <summary>
        /// Gets a TypeDescriptor for some reference.
        /// </summary>
        /// <param name="id">The identity value of the TypeDescriptor.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous RestResponse that may contain a TypeDescriptor.</returns>
        public ValueTask<RestResponse<TypeDescriptor>> Get(IIdentity<TypeDescriptor> id, NoParameters parameters)
        {
            var name = id.Value?.ToString();
            return data.TryGetValue(name, out var value)
                ? Rest.Value(value).ToResponseAsync()
                : RestResult.NotFound<TypeDescriptor>().ToResponseAsync();
        }
    }
}
