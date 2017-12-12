using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        internal static Type GetGeneric(this Type type, Type interf)
            => type.GetTypeInfo().ImplementedInterfaces.Concat(type.Iterate(t => t.BaseType).TakeWhile(t => t != null))
                .Where(i => i.GetGenericArguments().Length == 1 && i.GetGenericTypeDefinition() == interf)
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();
        internal static (Type,Type) GetGenerics2(this Type type, Type interf)
            => type.GetTypeInfo().ImplementedInterfaces.Concat(type.Iterate(t => t.BaseType).TakeWhile(t => t != null))
                .Where(i => i.GetGenericArguments().Length == 2 && i.GetGenericTypeDefinition() == interf)
                .Select(i => (i.GetGenericArguments()[0],i.GetGenericArguments()[1]))
                .FirstOrDefault();
        public static object GetContent(this XElement element)
            => element == null
                ? null
                : element.HasElements
                    ? (object)element.Elements()
                    : element.Value;
    }
}
