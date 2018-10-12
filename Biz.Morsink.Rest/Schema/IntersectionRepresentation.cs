using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public abstract class IntersectionRepresentation
    {
        public static Type[] GetTypeParameters(Type type)
        {
            if (type.BaseType != typeof(IntersectionRepresentation))
                return null;
            else if (type == typeof(DynamicIntersectionRepresentation))
                return new[] { typeof(object) };
            else
                return type.GetGenericArguments();
        }
        internal IntersectionRepresentation() { }
        public abstract IEnumerable<Type> GetTypes();
        public abstract IEnumerable<object> GetValues();

        public bool TryGet<C>(out C result)
        {
            var q = GetTypes().Zip(GetValues(), (t, v) => (t, v))
                .Where(t => t.t == typeof(C))
                .Select(t => t.v)
                .FirstOrDefault();
            if (q != null)
            {
                result = (C)q;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
        public static RepresentationCreator Create()
            => RepresentationCreator.New();
        public struct RepresentationCreator
        {
            public static RepresentationCreator New()
                => new RepresentationCreator(ImmutableStack<(Type, object)>.Empty);
            private readonly IImmutableStack<(Type, object)> entries;
            private RepresentationCreator(IImmutableStack<(Type,object)> entries)
            {
                this.entries = entries;
            }
            public RepresentationCreator Add(object o)
                => Add(o.GetType(), o);
            public RepresentationCreator Add(Type type, object o)
                => new RepresentationCreator(entries.Push((type, o)));
            public RepresentationCreator Add<T>(T o)
                => Add(typeof(T), o);

            public IntersectionRepresentation Create()
            {
                var entries = this.entries.Reverse().ToArray();
                switch (entries.Length)
                {
                    case 0:
                    case 1:
                        throw new InvalidOperationException("Not enough parameters");
                    case 2:
                        return (IntersectionRepresentation)Activator.CreateInstance(typeof(IntersectionRepresentation<,>).MakeGenericType(entries.Select(e => e.Item1).ToArray()), entries.Select(e => e.Item2).ToArray());
                    case 3:
                        return (IntersectionRepresentation)Activator.CreateInstance(typeof(IntersectionRepresentation<,,>).MakeGenericType(entries.Select(e => e.Item1).ToArray()), entries.Select(e => e.Item2).ToArray());
                    case 4:
                        return (IntersectionRepresentation)Activator.CreateInstance(typeof(IntersectionRepresentation<,,,>).MakeGenericType(entries.Select(e => e.Item1).ToArray()), entries.Select(e => e.Item2).ToArray());
                    case 5:
                        return (IntersectionRepresentation)Activator.CreateInstance(typeof(IntersectionRepresentation<,,,,>).MakeGenericType(entries.Select(e => e.Item1).ToArray()), entries.Select(e => e.Item2).ToArray());
                    default:
                        return new DynamicIntersectionRepresentation(entries);
                }
            }
        }
    }

    public sealed class IntersectionRepresentation<T, U>
        : IntersectionRepresentation
    {
        private static Type[] types = GetTypeParameters(typeof(IntersectionRepresentation<T, U>));
        private object[] values;

        public IntersectionRepresentation(T item1, U item2)
        {
            item = (item1, item2);
        }
        public IntersectionRepresentation((T, U) item)
        {
            this.item = item;
        }
        private (T, U) item;
        public T Item1 => item.Item1;
        public U Item2 => item.Item2;

        public override IEnumerable<Type> GetTypes()
            => types;

        public override IEnumerable<object> GetValues()
            => values = values ?? new object[] { Item1, Item2 };
    }
    public sealed class IntersectionRepresentation<T, U, V>
        : IntersectionRepresentation
    {
        private static Type[] types = GetTypeParameters(typeof(IntersectionRepresentation<T, U, V>));
        private object[] values;

        public IntersectionRepresentation(T item1, U item2, V item3)
        {
            item = (item1, item2, item3);
        }
        public IntersectionRepresentation((T, U, V) item)
        {
            this.item = item;
        }
        private (T, U, V) item;
        public T Item1 => item.Item1;
        public U Item2 => item.Item2;
        public V Item3 => item.Item3;

        public override IEnumerable<Type> GetTypes()
            => types;

        public override IEnumerable<object> GetValues()
            => values = values ?? new object[] { Item1, Item2, Item3 };
    }
    public sealed class IntersectionRepresentation<T, U, V, W>
     : IntersectionRepresentation
    {
        private static Type[] types = GetTypeParameters(typeof(IntersectionRepresentation<T, U, V, W>));
        private object[] values;

        public IntersectionRepresentation(T item1, U item2, V item3, W item4)
        {
            item = (item1, item2, item3, item4);
        }
        public IntersectionRepresentation((T, U, V, W) item)
        {
            this.item = item;
        }
        private (T, U, V, W) item;
        public T Item1 => item.Item1;
        public U Item2 => item.Item2;
        public V Item3 => item.Item3;
        public W Item4 => item.Item4;

        public override IEnumerable<Type> GetTypes()
            => types;

        public override IEnumerable<object> GetValues()
            => values = values ?? new object[] { Item1, Item2, Item3, Item4 };
    }
    public sealed class IntersectionRepresentation<T, U, V, W, X>
        : IntersectionRepresentation
    {
        private static Type[] types = GetTypeParameters(typeof(IntersectionRepresentation<T, U, V, W>));
        private object[] values;

        public IntersectionRepresentation(T item1, U item2, V item3, W item4, X item5)
        {
            item = (item1, item2, item3, item4, item5);
        }
        public IntersectionRepresentation((T, U, V, W, X) item)
        {
            this.item = item;
        }
        private (T, U, V, W, X) item;
        public T Item1 => item.Item1;
        public U Item2 => item.Item2;
        public V Item3 => item.Item3;
        public W Item4 => item.Item4;
        public X Item5 => item.Item5;

        public override IEnumerable<Type> GetTypes()
            => types;

        public override IEnumerable<object> GetValues()
            => values = values ?? new object[] { Item1, Item2, Item3, Item4, Item5 };
    }
    public sealed class DynamicIntersectionRepresentation : IntersectionRepresentation
    {
        private readonly IEnumerable<Type> types;
        private readonly IEnumerable<object> values;

        public DynamicIntersectionRepresentation(IReadOnlyList<(Type,object)> items)
        {
            if (!items.All(i => i.Item1.IsInstanceOfType(i.Item2)))
                throw new ArgumentException("Type error", nameof(items));
            types = items.Select(i => i.Item1).ToArray();
            values = items.Select(i => i.Item2).ToArray();
        }
        public DynamicIntersectionRepresentation(IReadOnlyList<object> items)
        {
            types = items.Select(i => i.GetType()).ToArray();
            values = items.ToArray();
        }

        public override IEnumerable<Type> GetTypes()
            => types;

        public override IEnumerable<object> GetValues()
            => values;
    }

}
