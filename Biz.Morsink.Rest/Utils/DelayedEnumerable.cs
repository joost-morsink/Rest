using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    /// <summary>
    /// This class delays the evaluation of an IEnumerable.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public class DelayedEnumerable<T> : IEnumerable<T>
    {
        private readonly Lazy<IEnumerable<T>> innerLazy;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="creator">A function that creates the IEnumerable.</param>
        public DelayedEnumerable(Func<IEnumerable<T>> creator)
        {
            innerLazy = new Lazy<IEnumerable<T>>(creator);
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lazy">A lazy that contains the IEnumerable.</param>
        public DelayedEnumerable(Lazy<IEnumerable<T>> lazy)
        {
            innerLazy = lazy;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
            => innerLazy.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
