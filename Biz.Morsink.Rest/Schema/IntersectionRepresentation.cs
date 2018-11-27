using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A representation class for intersection types.
    /// </summary>
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
        /// <summary>
        /// Constructor.
        /// </summary>
        internal IntersectionRepresentation() { }
        /// <summary>
        /// Gets the element types of the intersection.
        /// For the generic case these correspond to the type parameters.
        /// </summary>
        public abstract IEnumerable<Type> GetTypes();
        /// <summary>
        /// Gets the values of the elements of the intersection.
        /// </summary>
        public abstract IEnumerable<object> GetValues();
        /// <summary>
        /// Tries to get an object of type C from the intersection.
        /// </summary>
        /// <typeparam name="C">The type to get</typeparam>
        /// <param name="result">An object of type C, if it is found in the intersection.</param>
        /// <returns>True if an object was found, false otherwise.</returns>
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
        /// <summary>
        /// Start creating a new IntersectionRepresentation using a Builder.
        /// </summary>
        /// <returns>A new RepresentationBuilder</returns>
        public static RepresentationBuilder Create()
            => RepresentationBuilder.New();
        /// <summary>
        /// A builder struct for intersection representation objects.
        /// </summary>
        public struct RepresentationBuilder
        {
            /// <summary>
            /// Create a new builder.
            /// </summary>
            /// <returns>A new builder.</returns>
            public static RepresentationBuilder New()
                => new RepresentationBuilder(ImmutableStack<(Type, object)>.Empty);
            private readonly IImmutableStack<(Type, object)> entries;
            private RepresentationBuilder(IImmutableStack<(Type,object)> entries)
            {
                this.entries = entries;
            }
            /// <summary>
            /// Adds an object to the intersection based on the runtime type.
            /// </summary>
            /// <param name="o">The object to add.</param>
            /// <returns>A builder.</returns>
            public RepresentationBuilder Add(object o)
                => Add(o.GetType(), o);
            /// <summary>
            /// Adds an object to the intersection using a specified type.
            /// </summary>
            /// <param name="type">The type of the object.</param>
            /// <param name="o">The object to add.</param>
            /// <returns>A builder.</returns>
            public RepresentationBuilder Add(Type type, object o)
                => new RepresentationBuilder(entries.Push((type, o)));
            /// <summary>
            /// Adds an object of type T to the intersection.
            /// </summary>
            /// <typeparam name="T">The type of the object.</typeparam>
            /// <param name="o">The object to add.</param>
            /// <returns>A builder.</returns>
            public RepresentationBuilder Add<T>(T o)
                => Add(typeof(T), o);

            /// <summary>
            /// Create an instance of IntersectionRepresentation based on the current builder.
            /// </summary>
            /// <returns>An instance of the IntersectionRepresentation class.</returns>
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
    /// <summary>
    /// An intersectionRepresentation of two elements.
    /// </summary>
    /// <typeparam name="T">The type of the first element.</typeparam>
    /// <typeparam name="U">The type of the second element.</typeparam>
    public sealed class IntersectionRepresentation<T, U>
        : IntersectionRepresentation
    {
        private readonly static Type[] types = GetTypeParameters(typeof(IntersectionRepresentation<T, U>));
        private object[] values;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item1">The first element.</param>
        /// <param name="item2">The second element.</param>
        public IntersectionRepresentation(T item1, U item2)
        {
            item = (item1, item2);
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">A tuple containing the elements.</param>
        public IntersectionRepresentation((T, U) item)
        {
            this.item = item;
        }
        private (T, U) item;
        /// <summary>
        /// Gets the value of the first element.
        /// </summary>
        public T Item1 => item.Item1;
        /// <summary>
        /// Gets the value of the second element.
        /// </summary>
        public U Item2 => item.Item2;

        /// <summary>
        /// Gets the type parameters of the intersection.
        /// </summary>
        public override IEnumerable<Type> GetTypes()
            => types;

        /// <summary>
        /// Gets the values of the intersection corresponding to the type parameters.
        /// </summary>
        public override IEnumerable<object> GetValues()
            => values = values ?? new object[] { Item1, Item2 };
    }
    /// <summary>
    /// An intersectionRepresentation of two elements.
    /// </summary>
    /// <typeparam name="T">The type of the first element.</typeparam>
    /// <typeparam name="U">The type of the second element.</typeparam>
    /// <typeparam name="V">The type of the third element.</typeparam>
    public sealed class IntersectionRepresentation<T, U, V>
        : IntersectionRepresentation
    {
        private readonly static Type[] types = GetTypeParameters(typeof(IntersectionRepresentation<T, U, V>));
        private object[] values;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item1">The first element.</param>
        /// <param name="item2">The second element.</param>
        /// <param name="item3">The third element.</param>
        public IntersectionRepresentation(T item1, U item2, V item3)
        {
            item = (item1, item2, item3);
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">A tuple containing the elements.</param>
        public IntersectionRepresentation((T, U, V) item)
        {
            this.item = item;
        }
        private (T, U, V) item;
        /// <summary>
        /// Gets the value of the first element.
        /// </summary>
        public T Item1 => item.Item1;
        /// <summary>
        /// Gets the value of the second element.
        /// </summary>
        public U Item2 => item.Item2;
        /// <summary>
        /// Gets the value of the third element.
        /// </summary>
        public V Item3 => item.Item3;

        /// <summary>
        /// Gets the type parameters of the intersection.
        /// </summary>
        public override IEnumerable<Type> GetTypes()
            => types;

        /// <summary>
        /// Gets the values of the intersection corresponding to the type parameters.
        /// </summary>
        public override IEnumerable<object> GetValues()
            => values = values ?? new object[] { Item1, Item2, Item3 };
    }
    /// <summary>
    /// An intersectionRepresentation of two elements.
    /// </summary>
    /// <typeparam name="T">The type of the first element.</typeparam>
    /// <typeparam name="U">The type of the second element.</typeparam>
    /// <typeparam name="V">The type of the third element.</typeparam>
    /// <typeparam name="W">The type of the fourth element.</typeparam>
    public sealed class IntersectionRepresentation<T, U, V, W>
        : IntersectionRepresentation
    {
        private readonly static Type[] types = GetTypeParameters(typeof(IntersectionRepresentation<T, U, V, W>));
        private object[] values;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item1">The first element.</param>
        /// <param name="item2">The second element.</param>
        /// <param name="item3">The third element.</param>
        /// <param name="item4">The fourth element.</param>
        public IntersectionRepresentation(T item1, U item2, V item3, W item4)
        {
            item = (item1, item2, item3, item4);
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">A tuple containing the elements.</param>
        public IntersectionRepresentation((T, U, V, W) item)
        {
            this.item = item;
        }
        private (T, U, V, W) item;
        /// <summary>
        /// Gets the value of the first element.
        /// </summary>
        public T Item1 => item.Item1;
        /// <summary>
        /// Gets the value of the second element.
        /// </summary>
        public U Item2 => item.Item2;
        /// <summary>
        /// Gets the value of the third element.
        /// </summary>
        public V Item3 => item.Item3;
        /// <summary>
        /// Gets the value of the fourth element.
        /// </summary>
        public W Item4 => item.Item4;

        /// <summary>
        /// Gets the type parameters of the intersection.
        /// </summary>
        public override IEnumerable<Type> GetTypes()
            => types;

        /// <summary>
        /// Gets the values of the intersection corresponding to the type parameters.
        /// </summary>
        public override IEnumerable<object> GetValues()
            => values = values ?? new object[] { Item1, Item2, Item3, Item4 };
    }
    /// <summary>
    /// An intersectionRepresentation of two elements.
    /// </summary>
    /// <typeparam name="T">The type of the first element.</typeparam>
    /// <typeparam name="U">The type of the second element.</typeparam>
    /// <typeparam name="V">The type of the third element.</typeparam>
    /// <typeparam name="W">The type of the fourth element.</typeparam>
    /// <typeparam name="X">The type of the fifth element.</typeparam>
    public sealed class IntersectionRepresentation<T, U, V, W, X>
        : IntersectionRepresentation
    {
        private readonly static Type[] types = GetTypeParameters(typeof(IntersectionRepresentation<T, U, V, W>));
        private object[] values;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item1">The first element.</param>
        /// <param name="item2">The second element.</param>
        /// <param name="item3">The third element.</param>
        /// <param name="item4">The fourth element.</param>
        /// <param name="item5">The fifth element.</param>
        public IntersectionRepresentation(T item1, U item2, V item3, W item4, X item5)
        {
            item = (item1, item2, item3, item4, item5);
        }
                /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">A tuple containing the elements.</param>
        public IntersectionRepresentation((T, U, V, W, X) item)
        {
            this.item = item;
        }
        private (T, U, V, W, X) item;
        /// <summary>
        /// Gets the value of the first element.
        /// </summary>
        public T Item1 => item.Item1;
        /// <summary>
        /// Gets the value of the second element.
        /// </summary>
        public U Item2 => item.Item2;
        /// <summary>
        /// Gets the value of the third element.
        /// </summary>
        public V Item3 => item.Item3;
        /// <summary>
        /// Gets the value of the fourth element.
        /// </summary>
        public W Item4 => item.Item4;
        /// <summary>
        /// Gets the value of the fifth element.
        /// </summary>
        public X Item5 => item.Item5;

        /// <summary>
        /// Gets the type parameters of the intersection.
        /// </summary>
        public override IEnumerable<Type> GetTypes()
            => types;

        /// <summary>
        /// Gets the values of the intersection corresponding to the type parameters.
        /// </summary>
        public override IEnumerable<object> GetValues()
            => values = values ?? new object[] { Item1, Item2, Item3, Item4, Item5 };
    }
    /// <summary>
    /// This class is a dynamically typed implementation of the intersection representation pattern.
    /// </summary>
    public sealed class DynamicIntersectionRepresentation : IntersectionRepresentation
    {
        private readonly IEnumerable<Type> types;
        private readonly IEnumerable<object> values;

        /// <summary>
        /// Cosntructor.
        /// </summary>
        /// <param name="items">Type, object tuples representing the intersection's elements.</param>
        public DynamicIntersectionRepresentation(IReadOnlyList<(Type,object)> items)
        {
            if (!items.All(i => i.Item1.IsInstanceOfType(i.Item2)))
                throw new ArgumentException("Type error", nameof(items));
            types = items.Select(i => i.Item1).ToArray();
            values = items.Select(i => i.Item2).ToArray();
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">A collection of elements for the intersecion.</param>
        public DynamicIntersectionRepresentation(IReadOnlyList<object> items)
        {
            types = items.Select(i => i.GetType()).ToArray();
            values = items.ToArray();
        }

        /// <summary>
        /// Gets the types of the elements.
        /// </summary>
        public override IEnumerable<Type> GetTypes()
            => types;

        /// <summary>
        /// Gets the values of the elements.
        /// </summary>
        public override IEnumerable<object> GetValues()
            => values;
    }

}
