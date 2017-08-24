using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    public class TypeKeyedDictionary
    {
        public static TypeKeyedDictionary Empty { get; } = new TypeKeyedDictionary(ImmutableDictionary<Type, object>.Empty);
        private readonly ImmutableDictionary<Type, object> objects;
        private TypeKeyedDictionary(ImmutableDictionary<Type, object> objects)
        {
            this.objects = objects;
        }
        public TypeKeyedDictionary Add<T>(T obj)
            => new TypeKeyedDictionary(objects.Add(typeof(T), obj));
        public bool TryGet<T>(out T value)
        {
            if (objects.TryGetValue(typeof(T), out var val))
            {
                value = (T)val;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
        public void Execute<T>(Action<T> act)
        {
            if (TryGet(out T t))
                act(t);
        }
    }
}
