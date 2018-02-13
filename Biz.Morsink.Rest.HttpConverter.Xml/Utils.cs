using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.Rest.HttpConverter.Xml.XsdConstants;
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
        internal static (Type, Type) GetGenerics2(this Type type, Type interf)
            => type.GetTypeInfo().ImplementedInterfaces.Concat(type.Iterate(t => t.BaseType).TakeWhile(t => t != null))
                .Where(i => i.GetGenericArguments().Length == 2 && i.GetGenericTypeDefinition() == interf)
                .Select(i => (i.GetGenericArguments()[0], i.GetGenericArguments()[1]))
                .FirstOrDefault();
        public static object GetContent(this XElement element)
            => element == null
                ? null
                : element.HasElements
                    ? (object)element.Elements()
                    : element.Value;
        public static object GetContentOrNil(this XElement element)
                    => element == null
                        ? (object)new XAttribute(XSI + nil, true)
                        : element.HasElements
                            ? (object)element.Elements()
                            : element.Value;

        public static Ex Foreach(this Ex enumerable, Func<Ex, Ex> body)
        {
            var elementType = enumerable.Type.GetGeneric(typeof(IEnumerable<>));
            if (elementType == null)
                throw new ArgumentException("Enumerable is not IEnumerable<T>.", nameof(enumerable));
            var start = Ex.Label();
            var end = Ex.Label();
            var enumerator = Ex.Parameter(typeof(IEnumerator<>).MakeGenericType(elementType), "enumerator");
            var current = Ex.Parameter(elementType, "current");
            var block = Ex.Block(new[] { enumerator, current },
                Ex.Assign(enumerator, Ex.Call(enumerable, typeof(IEnumerable<>).MakeGenericType(elementType).GetMethod(nameof(IEnumerable<object>.GetEnumerator)))),
                Ex.Label(start),
                Ex.IfThen(Ex.Not(Ex.Call(Ex.Convert(enumerator, typeof(IEnumerator)), nameof(IEnumerator.MoveNext), Type.EmptyTypes)),
                    Ex.Goto(end)),
                Ex.Assign(current, Ex.Property(enumerator, nameof(IEnumerator<object>.Current))),
                body(current),
                Ex.Goto(start),
                Ex.Label(end),
                Ex.Call(Ex.Convert(enumerator, typeof(IDisposable)), nameof(IDisposable.Dispose), Type.EmptyTypes),
                Ex.Default(typeof(void)));
            return block;
        }
    }
}
