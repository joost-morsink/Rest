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
        /// Gets the 'static' Type of the Value in this Rest value instance.
        /// </summary>
        Type ValueType { get; }
        /// <summary>
        /// Optional list of provided links for the value.
        /// </summary>
        IReadOnlyList<Link> Links { get; }
        /// <summary>
        /// Optional list of embedded values for the main value.
        /// </summary>
        IReadOnlyList<Embedding> Embeddings { get; }
        /// <summary>
        /// A method to manupulate the Rest value into a new one.
        /// </summary>
        /// <param name="links">Function to manipulate the links.</param>
        /// <param name="embeddings">Function to manipulate the embeddings.</param>
        /// <returns>A new Rest value with manipulated links and/or embeddings collections.</returns>
        IRestValue Manipulate(Func<IRestValue, IEnumerable<Link>> links = null, Func<IRestValue, IEnumerable<Embedding>> embeddings = null);
    }
    /// <summary>
    /// Typed interface for Rest values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRestValue<T> : IRestValue
    {
        /// <summary>
        /// Gets the actual underlying value.
        /// </summary>
        new T Value { get; }
        /// <summary>
        /// A method to manupulate the Rest value into a new one.
        /// </summary>
        /// <param name="links">Function to manipulate the links.</param>
        /// <param name="embeddings">Function to manipulate the embeddings.</param>
        /// <returns>A new Rest value with manipulated links and/or embeddings collections.</returns>
        IRestValue<T> Manipulate(Func<IRestValue<T>, IEnumerable<Link>> links = null, Func<IRestValue<T>, IEnumerable<Embedding>> embeddings = null);
    }
}
