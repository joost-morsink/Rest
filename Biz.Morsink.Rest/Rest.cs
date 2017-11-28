using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Static class for Rest helper functions
    /// </summary>
    public static class Rest
    {
        /// <summary>
        /// Constructs a RestValue&lt;T&gt; from a value of type T.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="item">The Rest value's underlying value.</param>
        /// <returns>A RestValue containing the provided underlying value.</returns>
        public static RestValue<T> Value<T>(T item)
            => new RestValue<T>(item);
        /// <summary>
        /// Constructs a RestValue&lt;T&gt;.Builder from a value of type T.
        /// Using this builder, the other properties can be set before actual creation of the RestValue instance.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="item">The Rest value's underlying value.</param>
        /// <returns>A RestValue Builder containing the provided underlying value.</returns>
        public static RestValue<T>.Builder ValueBuilder<T>(T item)
            => RestValue<T>.Build().WithValue(item);
    }
}
