using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface for Rest value containers.
    /// </summary>
    public interface IHasRestValue
    {
        /// <summary>
        /// Gets a Rest value from the container.
        /// </summary>
        IRestValue RestValue { get; }
    }
    /// <summary>
    /// Generic interface for Rest value containers
    /// </summary>
    /// <typeparam name="T">The underlying type of the Rest value.</typeparam>
    public interface IHasRestValue<T> : IHasRestValue
        where T:class
    {
        /// <summary>
        /// Gets a typed Rest value from the container.
        /// </summary>
        new RestValue<T> RestValue { get; }
    }
}
