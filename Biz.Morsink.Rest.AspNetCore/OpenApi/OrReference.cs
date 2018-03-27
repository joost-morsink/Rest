using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    public class OrReference<T>
    {
        public OrReference(Reference r)
        {
            Item = default(T);
            Reference = r;
        }
        public OrReference(T t)
        {
            Item = t;
            Reference = null;
        }
        public bool IsReference => Reference != null;
        public Reference Reference { get; }
        public T Item { get; }

        public static implicit operator OrReference<T>(T item) 
            => new OrReference<T>(item);
        public static implicit operator OrReference<T>(Reference reference)
            => new OrReference<T>(reference);
    }
}
