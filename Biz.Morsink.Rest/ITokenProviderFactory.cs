using System;
using Biz.Morsink.Rest.Metadata;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Factory interface for ITokenProvider instances.
    /// </summary>
    public interface ITokenProviderFactory
    {
        /// <summary>
        /// Gets an ITokenProvider&ltT&gt;.
        /// </summary>
        /// <typeparam name="T">The type to get an ITokenProvider for.</typeparam>
        /// <returns>An instance of an ITokenProvider&lt;T&gt; if one could be found, null otherwise.</returns>
        ITokenProvider<T> GetTokenProvider<T>();
        /// <summary>
        /// Gets an ITokenProvider.
        /// </summary>
        /// <param name="type">The type to get an ITokenProvider for.</param>
        /// <returns>An instance of an ITokenProvider if one could be found, null otherwise.</returns>
        ITokenProvider GetTokenProvider(Type type);
    }
}
