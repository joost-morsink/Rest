using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public abstract class UnionRepresentation
    {
        public static Type[] GetTypeParameters(Type type)
        {
            if (type.BaseType != typeof(UnionRepresentation) && type.BaseType?.BaseType != typeof(UnionRepresentation))
                return null;
            else
                return type.GetGenericArguments();
        }
        internal UnionRepresentation() { }
        public abstract IReadOnlyList<Type> GetTypes();
        public abstract object GetItem();

        public static RepresentationCreator FromOptions(params Type[] types)
            => new RepresentationCreator(types);

        public struct RepresentationCreator
        {
            private readonly Type[] types;

            public RepresentationCreator(Type[] types)
            {
                this.types = types;
            }
            public RepresentationCreator(IEnumerable<Type> types)
            {
                this.types = types.ToArray();
            }
            public UnionRepresentation Create(object val)
            {
                if (val == null)
                    return null;
                var types = this.types;
                var baseType = typeof(UnionRepresentation).Assembly.GetType($"{typeof(UnionRepresentation).Namespace}.{nameof(UnionRepresentation)}`{types.Length}");
                var actualType = GetNestedTypes(baseType.MakeGenericType(types))
                    .FirstOrDefault(n => n.GetConstructors().Any(c => c.GetParameters()[0].ParameterType.IsAssignableFrom(val.GetType())));
                return (UnionRepresentation)Activator.CreateInstance(actualType, val);
            }
            private IEnumerable<Type> GetNestedTypes(Type type)
            {
                var generics = type.GetGenericArguments();
                if (generics.Length == 0)
                    return type.GetNestedTypes();
                else
                    return type.GetNestedTypes().Select(nt => nt.MakeGenericType(generics));
            }
        }
    }
    public abstract class UnionRepresentation<T, U> : UnionRepresentation
    {
        protected UnionRepresentation() { }
        private static readonly Type[] typeParams = new[] { typeof(T), typeof(U) };
        public static IReadOnlyList<Type> GetTypeParameters()
            => typeParams;
        public override IReadOnlyList<Type> GetTypes()
            => GetTypeParameters();

        public sealed class Option1 : UnionRepresentation<T, U>
        {
            public Option1(T item)
            {
                Item = item;
            }
            public T Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option2 : UnionRepresentation<T, U>
        {
            public Option2(U item)
            {
                Item = item;
            }
            public U Item { get; }
            public override object GetItem() => Item;
        }
    }
    public abstract class UnionRepresentation<T, U, V> : UnionRepresentation
    {
        private static readonly Type[] typeParams = new[] { typeof(T), typeof(U), typeof(V) };
        public static IReadOnlyList<Type> GetTypeParameters()
            => typeParams;
        public override IReadOnlyList<Type> GetTypes()
            => GetTypeParameters();

        public sealed class Option1 : UnionRepresentation<T, U, V>
        {
            public Option1(T item)
            {
                Item = item;
            }
            public T Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option2 : UnionRepresentation<T, U, V>
        {
            public Option2(U item)
            {
                Item = item;
            }
            public U Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option3 : UnionRepresentation<T, U, V>
        {
            public Option3(V item)
            {
                Item = item;
            }
            public V Item { get; }
            public override object GetItem() => Item;
        }
    }
    public abstract class UnionRepresentation<T, U, V, W> : UnionRepresentation
    {
        private static readonly Type[] typeParams = new[] { typeof(T), typeof(U), typeof(V), typeof(W) };
        public static IReadOnlyList<Type> GetTypeParameters()
            => typeParams;
        public override IReadOnlyList<Type> GetTypes()
            => GetTypeParameters();

        public sealed class Option1 : UnionRepresentation<T, U, V, W>
        {
            public Option1(T item)
            {
                Item = item;
            }
            public T Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option2 : UnionRepresentation<T, U, V, W>
        {
            public Option2(U item)
            {
                Item = item;
            }
            public U Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option3 : UnionRepresentation<T, U, V, W>
        {
            public Option3(V item)
            {
                Item = item;
            }
            public V Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option4 : UnionRepresentation<T, U, V, W>
        {
            public Option4(W item)
            {
                Item = item;
            }
            public W Item { get; }
            public override object GetItem() => Item;
        }
    }
    public abstract class UnionRepresentation<T, U, V, W, X> : UnionRepresentation
    {
        private static readonly Type[] typeParams = new[] { typeof(T), typeof(U), typeof(V), typeof(W), typeof(X) };
        public static IReadOnlyList<Type> GetTypeParameters()
            => typeParams;
        public override IReadOnlyList<Type> GetTypes()
            => GetTypeParameters();

        public sealed class Option1 : UnionRepresentation<T, U, V, W, X>
        {
            public Option1(T item)
            {
                Item = item;
            }
            public T Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option2 : UnionRepresentation<T, U, V, W, X>
        {
            public Option2(U item)
            {
                Item = item;
            }
            public U Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option3 : UnionRepresentation<T, U, V, W, X>
        {
            public Option3(V item)
            {
                Item = item;
            }
            public V Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option4 : UnionRepresentation<T, U, V, W, X>
        {
            public Option4(W item)
            {
                Item = item;
            }
            public W Item { get; }
            public override object GetItem() => Item;
        }
        public sealed class Option5 : UnionRepresentation<T, U, V, W, X>
        {
            public Option5(X item)
            {
                Item = item;
            }
            public X Item { get; }
            public override object GetItem() => Item;
        }
    }
}


