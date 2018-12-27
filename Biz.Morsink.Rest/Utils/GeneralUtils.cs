using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    /// <summary>
    /// General utility class.
    /// </summary>
    public static class GeneralUtils
    {
        /// <summary>
        /// Creates an infinite sequence by reapplying a function to a value.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="seed">The first element.</param>
        /// <param name="next">The function that calculates the next element.</param>
        /// <returns></returns>
        public static IEnumerable<T> Iterate<T>(this T seed, Func<T, T> next)
        {
            while (true)
            {
                yield return seed;
                seed = next(seed);
            }
        }
    }
}
