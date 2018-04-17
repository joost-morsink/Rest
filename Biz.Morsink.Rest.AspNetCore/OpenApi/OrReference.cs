using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// Helper class that either contains an object of some type, or a reference to an object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    public class OrReference<T>
    {
        /// <summary>
        /// Constructor for the reference case.
        /// </summary>
        /// <param name="r">A reference to an object.</param>
        public OrReference(Reference r)
        {
            Item = default(T);
            Reference = r;
        }
        /// <summary>
        /// Constructor for the object case.
        /// </summary>
        /// <param name="obj">The object.</param>
        public OrReference(T obj)
        {
            Item = obj;
            Reference = null;
        }
        /// <summary>
        /// Returns true if this represents a reference.
        /// </summary>
        public bool IsReference => Reference != null;
        /// <summary>
        /// Gets the reference if this is a reference case, null otherwise.
        /// </summary>
        public Reference Reference { get; }
        /// <summary>
        /// Gets the object if this is an object case, default otherwise.
        /// </summary>
        public T Item { get; }

        /// <summary>
        /// Implicit conversion operator of objects of type T to an OrReference&lt;T&gt;.
        /// </summary>
        /// <param name="item">The object to convert.</param>
        public static implicit operator OrReference<T>(T item) 
            => new OrReference<T>(item);
        /// <summary>
        /// Implicit conversion operator of references to objects to an OrReference&lt;T&gt;.
        /// </summary>
        /// <param name="reference">The reference to convert.</param>
        public static implicit operator OrReference<T>(Reference reference)
            => new OrReference<T>(reference);
    }
}
