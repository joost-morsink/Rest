using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This struct describes a property of a type.
    /// </summary>
    /// <typeparam name="T">The type of the property type.</typeparam>
    public struct PropertyDescriptor<T> : IEquatable<PropertyDescriptor<T>>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="type">The property type.</param>
        /// <param name="required">True if the property is required on the containing type.</param>
        public PropertyDescriptor(string name, T type, bool required = false)
        {
            Name = name;
            Type = type;
            Required = required;
        }
        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the property type.
        /// </summary>
        public T Type { get; }
        /// <summary>
        /// True if the property is required on the containing type.
        /// </summary>
        public bool Required { get; }

        public override bool Equals(object obj)
            => obj is PropertyDescriptor<T> && Equals((PropertyDescriptor<T>)obj);
        public bool Equals(PropertyDescriptor<T> other)
            => Name == other.Name && Required == other.Required && EqualityComparer<T>.Default.Equals(Type, other.Type);
        public override int GetHashCode()
            => Name.GetHashCode() ^ Type.GetHashCode() ^ Required.GetHashCode();
        
        /// <summary>
        /// Transforms the property desecriptor's type property.
        /// </summary>
        public PropertyDescriptor<U> Select<U>(Func<T, U> f)
            => new PropertyDescriptor<U>(Name, f(Type), Required);
    }
}
