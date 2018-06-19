using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    internal static class Utils
    {
        internal static Type GetGeneric(this Type type, Type interf)
            => type.GetTypeInfo().ImplementedInterfaces.Concat(type.Iterate(t => t.BaseType).TakeWhile(t => t != null))
                .Where(i => i.GetGenericArguments().Length == 1 && i.GetGenericTypeDefinition() == interf)
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();

        internal static IEnumerable<T> Iterate<T>(this T seed, Func<T, T> next)
        {
            while (true)
            {
                yield return seed;
                seed = next(seed);
            }
        }
    }
}
