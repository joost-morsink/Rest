using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    /// <summary>
    /// Easily composable readonly list implementation.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public abstract class ReadOnlyList<T> : IReadOnlyList<T>
    {
        private const int MERGE_THRESHOLD = 8;
        /// <summary>
        /// Contain an empty readonly list.
        /// </summary>
        public static ReadOnlyList<T> Empty { get; } = EmptyImpl.Instance;
        /// <summary>
        /// Creates a single element list.
        /// </summary>
        /// <param name="item">The element.</param>
        /// <returns>A single element readonly list.</returns>
        public static ReadOnlyList<T> Create(T item)
            => new FromSingle(item);
        /// <summary>
        /// Creates a decorated readonly list.
        /// </summary>
        /// <param name="list">The to be decorated list.</param>
        /// <returns>A readonly list.</returns>
        public static ReadOnlyList<T> Create(IReadOnlyList<T> list)
            => list as ReadOnlyList<T> ?? new FromList(list);
        /// <summary>
        /// Creates a new readonly list by concatenating a number of readonly lists.
        /// </summary>
        /// <param name="lists">The lists to concatenate.</param>
        /// <returns>A concatenated readonly list.</returns>
        public static ReadOnlyList<T> Create(IEnumerable<IReadOnlyList<T>> lists)
        {
            var n = lists.Sum(l => l.Count);
            if (n <= MERGE_THRESHOLD)
                return Create(lists.SelectMany(l => l).ToArray());
            else
                return new FromMulti(lists);
        }
        /// <summary>
        /// Creates a new readonly list by concatenating a number of readonly lists.
        /// </summary>
        /// <param name="lists">The lists to concatenate.</param>
        /// <returns>A concatenated readonly list.</returns>
        public static ReadOnlyList<T> Create(params IReadOnlyList<T>[] lists)
            => Create((IEnumerable<IReadOnlyList<T>>)lists);

        /// <summary>
        /// Concatenates another list on the current one, creating a new list.
        /// </summary>
        /// <param name="other">The other list.</param>
        /// <returns>A concatenated readonly list.</returns>
        public virtual ReadOnlyList<T> Concat(IReadOnlyList<T> other)
            => Create(this, other);
        /// <summary>
        /// Prepends an element to the current list, creating a new list.
        /// </summary>
        /// <param name="item">The item to prepend.</param>
        /// <returns>A new readonly list containing the prepended item.</returns>
        public virtual ReadOnlyList<T> Preprend(T item)
            => Create(Create(item), this);
        /// <summary>
        /// Appends an element to the current list, creating a new list.
        /// </summary>
        /// <param name="item">The item to append.</param>
        /// <returns>A new readonly list containing the appended item.</returns>
        public virtual ReadOnlyList<T> Append(T item)
            => Create(this, Create(item));

        /// <summary>
        /// Gets the element at the specified position.
        /// </summary>
        /// <param name="index">A position within the list.</param>
        /// <returns>The element at the specified position.</returns>
        public abstract T this[int index] { get; }
        /// <summary>
        /// Contains the number of elements in the readonly list.
        /// </summary>
        public abstract int Count { get; }
        /// <summary>
        /// Gets an enumerator for the readonly list.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private class EmptyImpl : ReadOnlyList<T>
        {
            public static EmptyImpl Instance { get; } = new EmptyImpl();
            private EmptyImpl() { }
            public override T this[int index] => throw new IndexOutOfRangeException();
            public override int Count => 0;
            public override ReadOnlyList<T> Concat(IReadOnlyList<T> other)
                => Create(other);
            public override ReadOnlyList<T> Append(T item)
                => Create(item);
            public override ReadOnlyList<T> Preprend(T item)
                => Create(item);

            public override IEnumerator<T> GetEnumerator()
                => Enumerable.Empty<T>().GetEnumerator();
        }
        private class FromSingle : ReadOnlyList<T>
        {
            private readonly T item;
            public FromSingle(T item)
            {
                this.item = item;
            }

            public override T this[int index]
            {
                get
                {
                    if (index != 0)
                        throw new IndexOutOfRangeException();
                    return item;
                }
            }

            public override int Count => 1;

            public override IEnumerator<T> GetEnumerator()
            {
                yield return item;
            }
        }
        private class FromList : ReadOnlyList<T>
        {
            private readonly IReadOnlyList<T> items;
            public FromList(IReadOnlyList<T> items)
            {
                this.items = items;
            }

            public override T this[int index] => items[index];

            public override int Count => items.Count;

            public override IEnumerator<T> GetEnumerator()
                => items.GetEnumerator();
        }
        private class FromMulti : ReadOnlyList<T>
        {
            private readonly IReadOnlyList<T>[] lists;

            public FromMulti(IEnumerable<IReadOnlyList<T>> lists)
            {
                this.lists = lists.ToArray();
                this.Count = lists.Sum(l => l.Count);
            }
            public override T this[int index]
            {
                get
                {
                    var n = 0;
                    while (n < lists.Length && index >= lists[n].Count)
                        index -= lists[n++].Count;
                    return lists[n][index];
                }
            }
            public override int Count { get; }

            public override IEnumerator<T> GetEnumerator()
                => lists.SelectMany(list => list).GetEnumerator();
        }
    }
}
