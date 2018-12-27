using System;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Default implementation of a ITokenProviderFactory.
    /// </summary>
    public class TokenProviderFactory : ITokenProviderFactory
    {
        private readonly IServiceProviderAccessor serviceProviderAccessor;

        public TokenProviderFactory(IServiceProviderAccessor serviceProviderAccessor)
        {
            this.serviceProviderAccessor = serviceProviderAccessor;
        }
        /// <summary>
        /// Gets an ITokenProvider&ltT&gt;.
        /// </summary>
        /// <typeparam name="T">The type to get an ITokenProvider for.</typeparam>
        /// <returns>An instance of an ITokenProvider&lt;T&gt; if one could be found, null otherwise.</returns>
        public ITokenProvider<T> GetTokenProvider<T>()
            => (ITokenProvider<T>)serviceProviderAccessor.ServiceProvider.GetService(typeof(ITokenProvider<T>));

        /// <summary>
        /// Gets an ITokenProvider.
        /// </summary>
        /// <param name="type">The type to get an ITokenProvider for.</param>
        /// <returns>An instance of an ITokenProvider if one could be found, null otherwise.</returns>
        public ITokenProvider GetTokenProvider(Type type)
            => (ITokenProvider)serviceProviderAccessor.ServiceProvider.GetService(typeof(ITokenProvider<>).MakeGenericType(type));
    }
}
