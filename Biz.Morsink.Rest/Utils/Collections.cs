using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    /// <summary>
    /// Helper methods for collections
    /// </summary>
    internal static class Collections
    {
        /// <summary>
        /// Converts an IReadOnlyCollection&lt;T&gt; to an array of T. 
        /// The method uses the Count property to immediately allocate an array of the correct size.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="collection">The readonly collection to convert.</param>
        /// <returns>An array of elements of T.</returns>
        public static T[] ToArray<T>(this IReadOnlyCollection<T> collection)
        {
            var result = new T[collection.Count];
            var i = 0;
            foreach (var element in collection)
                result[i++] = element;
            return result;
        }
        /// <summary>
        /// Converts an IReadOnlyList&lt;T&gt; to an array of T.
        /// The method uses the Count property to immediately allocate an array of the correct size, and traverses the list using the indexer-property.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="list">The readonly list to convert.</param>
        /// <returns>An array of elements of T.</returns>
        public static T[] ToArray<T>(this IReadOnlyList<T> list)
        {
            var result = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = list[i];
            return result;
        }
        /// <summary>
        /// Implements Linq 'Select' (eagerly) for IReadOnlyCollection&lt;T&gt;. 
        /// The resulting collection has the same count, and for every element of the original collection, the projected element is in the resulting collection.
        /// </summary>
        /// <typeparam name="T">The original element type.</typeparam>
        /// <typeparam name="U">The element type of the resulting collection.</typeparam>
        /// <param name="collection">The original collection.</param>
        /// <param name="projection">A projection function to apply to every element of the original collection.</param>
        /// <returns>An IReadOnlyCollection&lt;U&gt; of projected elements.</returns>
        public static IReadOnlyCollection<U> Select<T, U>(this IReadOnlyCollection<T> collection, Func<T, U> projection)
        {
            var result = new U[collection.Count];
            var i = 0;
            foreach (var element in collection)
                result[i++] = projection(element);
            return result;
        }
        /// <summary>
        /// Implements Linq 'Select' (eagerly) for IReadOnlyList&lt;T&gt;. 
        /// The resulting list has the same count, and for every element of the original collection, the projected element is in the resulting list at the same index.
        /// </summary>
        /// <typeparam name="T">The original element type.</typeparam>
        /// <typeparam name="U">The element type of the resulting list.</typeparam>
        /// <param name="collection">The original list.</param>
        /// <param name="projection">A projection function to apply to every element of the original list.</param>
        /// <returns>An IReadOnlyList&lt;U&gt; of projected elements.</returns>
        public static IReadOnlyList<U> Select<T, U>(this IReadOnlyList<T> list, Func<T, U> projection)
        {
            var result = new U[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = projection(list[i]);
            return result;
        }
        /// <summary>
        /// Concatenates two readonly lists.
        /// </summary>
        /// <typeparam name="T">The element type of the lists.</typeparam>
        /// <param name="src">The original list.</param>
        /// <param name="other">The list to concatenate.</param>
        /// <returns>A concatenated list.</returns>
        public static IReadOnlyList<T> Concat<T>(this IReadOnlyList<T> src, IReadOnlyList<T> other)
            => ReadOnlyList<T>.Create(src, other);
        /// <summary>
        /// Constructs a new readonly list, based on a readonly list and a single element.
        /// </summary>
        /// <typeparam name="T">The element type of the lists.</typeparam>
        /// <param name="src">The original list.</param>
        /// <param name="item">The item to append.</param>
        /// <returns>A new list.</returns>
        public static IReadOnlyList<T> Append<T>(this IReadOnlyList<T> src, T item)
            => ReadOnlyList<T>.Create(src).Append(item);
        /// <summary>
        /// Constructs a new readonly list, based on a readonly list and a single element.
        /// </summary>
        /// <typeparam name="T">The element type of the lists.</typeparam>
        /// <param name="src">The original list.</param>
        /// <param name="item">The item to prepend.</param>
        /// <returns>A new list.</returns>
        public static IReadOnlyList<T> Prepend<T>(this IReadOnlyList<T> src, T item)
            => ReadOnlyList<T>.Create(src).Append(item);
    }
}
