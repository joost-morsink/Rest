using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface for Rest values.
    /// </summary>
    public interface IRestValue
    {
        /// <summary>
        /// The actual underlying value.
        /// </summary>
        object Value { get; }
        /// <summary>
        /// Optional list of provided links for the value.
        /// </summary>
        IReadOnlyList<Link> Links { get; }
        /// <summary>
        /// Optional list of embedded values for the main value.
        /// </summary>
        IReadOnlyList<object> Embeddings { get; }
        /// <summary>
        /// A method to manupulate the Rest value into a new one.
        /// </summary>
        /// <param name="links">Function to manipulate the links.</param>
        /// <param name="embeddings">Function to manipulate the embeddings.</param>
        /// <returns>A new Rest value with manipulated links and/or embeddings collections.</returns>
        IRestValue Manipulate(Func<IRestValue, IEnumerable<Link>> links = null, Func<IRestValue, IEnumerable<object>> embeddings = null);
    }
}
