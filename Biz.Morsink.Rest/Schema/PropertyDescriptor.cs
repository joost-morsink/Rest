using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest.Schema
{
    public struct PropertyDescriptor<T> : IEquatable<PropertyDescriptor<T>>
    {
        public PropertyDescriptor(string name, T type, bool required = false)
        {
            Name = name;
            Type = type;
            Required = required;
        }
        public string Name { get; }
        public T Type { get; }
        public bool Required { get; }

        public override bool Equals(object obj)
            => obj is PropertyDescriptor<T> && Equals((PropertyDescriptor<T>)obj);
        public bool Equals(PropertyDescriptor<T> other)
            => Name == other.Name && Required == other.Required && EqualityComparer<T>.Default.Equals(Type, other.Type);
        public override int GetHashCode()
            => Name.GetHashCode() ^ Type.GetHashCode() ^ Required.GetHashCode();
        
        public PropertyDescriptor<U> Select<U>(Func<T, U> f)
            => new PropertyDescriptor<U>(Name, f(Type), Required);
    }
}
