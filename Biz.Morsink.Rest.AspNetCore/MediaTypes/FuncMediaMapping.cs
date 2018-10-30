using System;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// Implementation of IMediaTypeMapping using simple Funcs.
    /// </summary>
    internal class FuncMediaMapping : IMediaTypeMapping
    {
        private readonly Func<Type, bool> applies;
        private readonly Func<Type, string> mediaType;

        public FuncMediaMapping(Func<Type, bool> applies, Func<Type, string> mediaType)
        {
            this.applies = applies;
            this.mediaType = mediaType;
        }

        public bool Applies(Type type)
            => applies(type);

        public string GetMediaType(Type type)
            => mediaType(type);
    }
}
