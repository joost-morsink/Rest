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
        /// This method should return the corresponding JsonSchema object for some Type.
        /// </summary>
        /// <param name="type">The type to get a schema for.</param>
        /// <returns>A JsonSchema object that corresponds to the given Type.</returns>
        JsonSchema GetSchema(Type type);
    }
}
