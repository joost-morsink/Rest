﻿using Biz.Morsink.Rest.Metadata;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// An interface for token providers. 
    /// Token providers generate a version token for resources.
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Gets a token for the specified item.
        /// </summary>
        /// <param name="item">The item to create a version token for.</param>
        /// <returns>A version token for the specified item.</returns>
        VersionToken GetTokenFor(object item);
    }
    /// <summary>
    /// An interface for token providers. 
    /// Token providers generate a version token for resources.
    /// </summary>
    /// <typeparam name="T">The type to provider version tokens for.</typeparam>
    public interface ITokenProvider<T> : ITokenProvider
    {
        /// <summary>
        /// Gets a token for the specified item.
        /// </summary>
        /// <param name="item">The item to create a version token for.</param>
        /// <returns>A version token for the specified item.</returns>
        VersionToken GetTokenFor(T item);
    }
}
