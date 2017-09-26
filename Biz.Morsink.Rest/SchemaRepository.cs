using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biz.Morsink.Identity;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public class SchemaRepository : RestRepository<TypeDescriptor>, IRestGet<TypeDescriptor, NoParameters>
    {
        private readonly Dictionary<string, TypeDescriptor> data;

        public SchemaRepository(IEnumerable<IRestRepository> repositories)
        {
            var types = repositories.SelectMany(repo => repo.SchemaTypes).Distinct();
            data = types.ToDictionary(type => type.FullName, type => type.GetDescriptor());
        }

        public ValueTask<RestResponse<TypeDescriptor>> Get(IIdentity<TypeDescriptor> id, NoParameters parameters)
        {
            var name = id.Value?.ToString();
            return data.TryGetValue(name, out var value)
                ? Rest.Value(value).ToResponseAsync()
                : RestResult.NotFound<TypeDescriptor>().ToResponseAsync();
        }
    }
}
