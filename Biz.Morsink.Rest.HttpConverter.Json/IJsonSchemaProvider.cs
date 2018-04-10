using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Interface for a service providing Json schema's.
    /// </summary>
    public interface IJsonSchemaProvider
    {
        /// <summary>
        /// This method should return the corresponding JsonSchema object for some TypeDescriptor.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor to get a schema for.</param>
        /// <returns>A JsonSchema object that corresponds to the given TypeDescriptor.</returns>
        JsonSchema GetSchema(TypeDescriptor typeDescriptor);
    }
}
