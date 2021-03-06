﻿using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface that provides traversable links for some resource of type T.
    /// This interface does not need an actual instance of the resource to determine links.
    /// </summary>
    /// <typeparam name="T">The type of resource links are provided for.</typeparam>
    public interface ILinkProvider<T>
    {
        /// <summary>
        /// Gets a list of links for some resource.
        /// </summary>
        /// <param name="id">The identity value for the resource.</param>
        /// <returns>A list of links.</returns>
        IReadOnlyList<Link> GetLinks(IIdentity<T> id);
    }
}
