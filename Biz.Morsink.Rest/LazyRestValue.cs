using Biz.Morsink.Rest.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A Rest Value with lazily evaluated components.
    /// </summary>
    /// <typeparam name="T">The type of the Rest value's underlying value.</typeparam>
    public class LazyRestValue<T> : IRestValue<T>
    {
        private readonly Lazy<T> valueCreator;
        private IEnumerable<Link> links;
        private IEnumerable<Embedding> embeddings;

        private LazyRestValue(Lazy<T> valueCreator, IEnumerable<Link> links, IEnumerable<Embedding> embeddings)
        {
            this.valueCreator = valueCreator;
            this.links = links;
            this.embeddings = embeddings;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="valueCreator">A creator for the underlying value.</param>
        /// <param name="linksCreator">A creator for a Link collection.</param>
        /// <param name="embeddingsCreator">A creator for a collection of embeddings.</param>
        public LazyRestValue(Func<T> valueCreator, Func<IEnumerable<Link>> linksCreator, Func<IEnumerable<Embedding>> embeddingsCreator)
            : this(new Lazy<T>(valueCreator), new DelayedEnumerable<Link>(linksCreator), new DelayedEnumerable<Embedding>(embeddingsCreator))
        { }

        object IRestValue.Value => Value;
        /// <summary>
        /// Contains the underlying value.
        /// </summary>
        public T Value => valueCreator.Value;
        /// <summary>
        /// Contains the underlying value's type.
        /// </summary>
        public Type ValueType => typeof(T);

        /// <summary>
        /// Contains the links applicable to this Rest value.
        /// </summary>
        public IReadOnlyList<Link> Links => (IReadOnlyList<Link>)(links = links as IReadOnlyList<Link> ?? ReadOnlyList<Link>.Create(links.ToArray()));

        /// <summary>
        /// Contains the embeddings associated with this Rest value.
        /// </summary>
        public IReadOnlyList<Embedding> Embeddings => (IReadOnlyList<Embedding>)(embeddings = embeddings as IReadOnlyList<Embedding> ?? ReadOnlyList<Embedding>.Create(embeddings.ToArray()));

        /// <summary>
        /// Lazily changes the Rest value's underlying value in a new LazyRestValue.
        /// </summary>
        /// <typeparam name="U">The new underlying value's type.</typeparam>
        /// <param name="f">The manipulation function for the underlying value.</param>
        /// <returns>A new LazyRestValue representing the modified version.</returns>
        public LazyRestValue<U> Select<U>(Func<T, U> f)
            => new LazyRestValue<U>(new Lazy<U>(() => f(Value)), links, embeddings);

        IRestValue IRestValue.Manipulate(Func<IRestValue, IEnumerable<Link>> links, Func<IRestValue, IEnumerable<Embedding>> embeddings)
            => Manipulate(links == null ? (Func<LazyRestValue<T>, IEnumerable<Link>>)null : rv => links(rv),
                embeddings == null ? (Func<LazyRestValue<T>, IEnumerable<Embedding>>)null : rv => embeddings(rv));
        IRestValue<T> IRestValue<T>.Manipulate(Func<IRestValue<T>, IEnumerable<Link>> links, Func<IRestValue<T>, IEnumerable<Embedding>> embeddings)
            => Manipulate(links == null ? (Func<LazyRestValue<T>, IEnumerable<Link>>)null : rv => links(rv),
                embeddings == null ? (Func<LazyRestValue<T>, IEnumerable<Embedding>>)null : rv => embeddings(rv));

        /// <summary>
        /// Manipulates the links and embeddings into a new LazyRestValue.
        /// </summary>
        /// <param name="links">A function creating the new link collection.</param>
        /// <param name="embeddings">A function creating the new embedding collection.</param>
        /// <returns>A manipulated lazy Rest value</returns>
        public LazyRestValue<T> Manipulate(Func<LazyRestValue<T>, IEnumerable<Link>> links = null, Func<LazyRestValue<T>, IEnumerable<Embedding>> embeddings = null)
            => new LazyRestValue<T>(
                valueCreator,
                links == null ? this.links : new DelayedEnumerable<Link>(() => links(this)),
                embeddings == null ? this.embeddings : new DelayedEnumerable<Embedding>(() => embeddings(this)));
    }
}
