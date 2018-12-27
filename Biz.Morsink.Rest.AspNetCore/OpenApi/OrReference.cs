using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// Helper class that either contains an object of some type, or a reference to an object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    public abstract class OrReference<T>
    {
        private OrReference() { }
        /// <summary>
        /// Returns true if this represents a reference.
        /// </summary>
        public abstract bool IsReference();
        /// <summary>
        /// Implements the Reference option of the OrReference&lt;T&gt; type.
        /// </summary>
        public sealed class ReferenceImpl : OrReference<T>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="reference">A reference.</param>
            public ReferenceImpl(Reference reference)
            {
                Reference = reference;
            }
            /// <summary>
            /// Gets the reference if this is a reference case, null otherwise.
            /// </summary>
            public Reference Reference { get; }
            public override bool IsReference() => true;
        }
        /// <summary>
        /// Implements the Item option of the OrReference&lt;T&gt; type.
        /// </summary>
        public sealed class ItemImpl : OrReference<T>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="item"></param>
            public ItemImpl(T item)
            {
                Item = item;
            }
            /// <summary>
            /// Gets the object if this is an object case, default otherwise.
            /// </summary>
            public T Item { get; }
            public override bool IsReference() => false;
        }

        /// <summary>
        /// Implicit conversion operator of objects of type T to an OrReference&lt;T&gt;.
        /// </summary>
        /// <param name="item">The object to convert.</param>
        public static implicit operator OrReference<T>(T item) 
            => new ItemImpl(item);
        /// <summary>
        /// Implicit conversion operator of references to objects to an OrReference&lt;T&gt;.
        /// </summary>
        /// <param name="reference">The reference to convert.</param>
        public static implicit operator OrReference<T>(Reference reference)
            => new ReferenceImpl(reference);
    }
}
