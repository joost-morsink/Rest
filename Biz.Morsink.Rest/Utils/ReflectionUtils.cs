using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest.Utils
{
    public static class ReflectionUtils
    {
        public static IEnumerable<T> Iterate<T>(this T seed, Func<T, T> next)
        {
            while (true)
            {
                yield return seed;
                seed = next(seed);
            }
        }

        public static Type GetGeneric(this Type type, Type interf)
            => type.GetTypeInfo().ImplementedInterfaces.Concat(type.Iterate(t => t.BaseType).TakeWhile(t => t != null))
                .Select(i => i.GetTypeInfo())
                .Where(i => i.GenericTypeArguments.Length == 1 && i.GetGenericTypeDefinition() == interf)
                .Select(i => i.GenericTypeArguments[0])
                .FirstOrDefault();
        public static (Type, Type) GetGenerics2(this Type type, Type interf)
            => type.GetTypeInfo().ImplementedInterfaces.Concat(type.Iterate(t => t.BaseType).TakeWhile(t => t != null))
                .Select(i => i.GetTypeInfo())
                .Where(i => i.GenericTypeArguments.Length == 2 && i.GetGenericTypeDefinition() == interf)
                .Select(i => (i.GenericTypeArguments[0], i.GenericTypeArguments[1]))
                .FirstOrDefault();


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
