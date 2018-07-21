using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Schema translators translate schemas for a certain type and technology.
    /// </summary>
    /// <typeparam name="S">A type indicating the Schema type for a specific technology.</typeparam>
    public interface ISchemaTranslator<S>
    {
        /// <summary>
        /// Gets the schema for this translator.
        /// </summary>
        /// <returns></returns>
        S GetSchema(Type type);
    }
    /// <summary>
    /// Schema translators translate schemas for a certain type and technology.
    /// </summary>
    /// <typeparam name="T">The type to which the schema applies.</typeparam>
    /// <typeparam name="S">A type indicating the Schema type for a specific technology.</typeparam>
    public interface ISchemaTranslator<T, S> : ISchemaTranslator<S>
    {

    }
}
