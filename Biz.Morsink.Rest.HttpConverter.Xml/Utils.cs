using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// Utility methods
    /// </summary>
    static class Utils
    {
        internal static IEnumerable<T> Iterate<T>(this T seed, Func<T, T> next)
        {
            while (true)
            {
                yield return seed;
                seed = next(seed);
            }
        }
        public static object GetContent(this XElement element)
            => element.HasElements
                ? (object)element.Elements()
                : element.Value;
    }
}
