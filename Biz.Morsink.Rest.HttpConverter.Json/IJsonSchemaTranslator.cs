using Biz.Morsink.Rest.AspNetCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// This interface specifies types that convert objects to and from Json as well as providing a schema for those types.
    /// </summary>
    public interface IJsonSchemaTranslator : ISchemaTranslator<JsonSchema>
    {
        /// <summary>
        /// Gets the JsonConverter for conversion of the type to and from Json.
        /// </summary>
        JsonConverter GetConverter();
    }
    /// <summary>
    /// This interface specifies types that convert objects to and from Json as well as providing a schema for those types.
    /// </summary>
    /// <typeparam name="T">The type the translator applies to.</typeparam>
    public interface IJsonSchemaTranslator<T> : ISchemaTranslator<T, JsonSchema>, IJsonSchemaTranslator
    {
    }
}
