using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    internal static class Collections
    {
        public static T[] ToArray<T>(this IReadOnlyCollection<T> collection)
        {
            var result = new T[collection.Count];
            var i = 0;
            foreach (var element in collection)
                result[i++] = element;
            return result;
        }
        public static T[] ToArray<T>(this IReadOnlyList<T> list)
        {
            var result = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = list[i];
            return result;
        }
        public static IReadOnlyCollection<U> Select<T,U>(this IReadOnlyCollection<T> collection, Func<T,U> projection)
        {
            var result = new U[collection.Count];
            var i = 0;
            foreach (var element in collection)
                result[i++] = projection(element);
            return result;
        }
        public static IReadOnlyList<U> Select<T,U>(this IReadOnlyList<T> list, Func<T,U> projection)
        {
            var result = new U[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = projection(list[i]);
            return result;
        }
    }
}
