using System;
using Biz.Morsink.Rest.Metadata;

namespace Biz.Morsink.Rest
{
    public interface ITokenProviderFactory
    {
        ITokenProvider<T> GetTokenProvider<T>();
        ITokenProvider GetTokenProvider(Type type);
    }
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
