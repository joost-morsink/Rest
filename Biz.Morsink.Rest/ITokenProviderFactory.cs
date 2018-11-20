using System;
using Biz.Morsink.Rest.Metadata;

namespace Biz.Morsink.Rest
{
    public interface ITokenProviderFactory
    {
        ITokenProvider<T> GetTokenProvider<T>();
        ITokenProvider GetTokenProvider(Type type);
    }
}
