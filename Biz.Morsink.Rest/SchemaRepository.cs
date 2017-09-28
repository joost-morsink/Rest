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
        private Dictionary<string, TypeDescriptor> data;
        private Dictionary<string, TypeDescriptor> Data
            => data = data == null || data.Count < TypeDescriptorCreator.RegisteredTypes.Count
                ? TypeDescriptorCreator.RegisteredTypes.ToDictionary(StringRepresentation, t => t.GetDescriptor())
                : data;

        /// <summary>
        /// Gets the string representation for a Type.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>A string representation for the specified Type.</returns>
        public string StringRepresentation(Type t)
            => t.ToString();
        
        private string StringRepresentation(object o)
            => o as string ?? (o is Type t ? StringRepresentation(t) : null);
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repositories">All repositories to get type information from.</param>
        public SchemaRepository()
        {
            data = null;
        }

        /// <summary>
        /// Gets a TypeDescriptor for some reference.
        /// </summary>
        /// <param name="id">The identity value of the TypeDescriptor.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous RestResponse that may contain a TypeDescriptor.</returns>
        public ValueTask<RestResponse<TypeDescriptor>> Get(IIdentity<TypeDescriptor> id, NoParameters parameters)
        {
            var name = StringRepresentation(id.Value);
            if (Data.TryGetValue(name, out var value))
                return Rest.Value(value).ToResponseAsync();
            else
            {
                var res = (id.Value as Type)?.GetDescriptor();
                return res == null
                    ? RestResult.NotFound<TypeDescriptor>().ToResponseAsync()
                    : Rest.Value(res).ToResponseAsync();
            }
        }
    }
}
