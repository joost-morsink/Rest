using System;

namespace Biz.Morsink.Rest
{
    public class TokenProviderFactory : ITokenProviderFactory
    {
        private readonly IServiceProvider serviceProvider;

        public TokenProviderFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public ITokenProvider<T> GetTokenProvider<T>()
            => (ITokenProvider<T>)serviceProvider.GetService(typeof(ITokenProvider<T>));

        public ITokenProvider GetTokenProvider(Type type)
            => (ITokenProvider)serviceProvider.GetService(typeof(ITokenProvider<>).MakeGenericType(type));
    }
}
