using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface that provides traversable links for some resource of type T.
    /// This interface needs an actual instance of the resource to determine the links.
    /// Links may use data from the resource to generate links.
    /// </summary>
    /// <typeparam name="T">The type of resource links are provided for.</typeparam>
    public interface IDynamicLinkProvider<T>
    {
        /// <summary>
        /// Gets a list of links for some resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>A list of links.</returns>
        IReadOnlyList<Link> GetLinks(T resource);
    }
}
