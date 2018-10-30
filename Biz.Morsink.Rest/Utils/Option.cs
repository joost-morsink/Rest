using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    public static class Option
    {
        public static Option<T> Some<T>(T value)
            => new Option<T>(value);
        public static Option<T> None<T>()
            => Option<T>.None;
    }
    public struct Option<T>
    {
        public static Option<T> None => default;

        public T Value { get; }
        public bool HasValue { get; }

        public Option(T value)
        {
            Value = value;
            HasValue = true;
        }
        public Option<U> Select<U>(Func<T, U> f)
        {
            if (HasValue)
                return Option.Some(f(Value));
            else
                return Option<U>.None;
        }
        public Option<T> Where(Func<T, bool> predicate)
            => HasValue && predicate(Value) ? this : None;
        public Option<V> SelectMany<U, V>(Func<T, Option<U>> f, Func<T, U, V> g)
        {
            if (HasValue)
            {
                var x = f(Value);
                if (x.HasValue)
                    return Option.Some(g(Value, x.Value));
                else
                    return Option<V>.None;
            }
            else
                return Option<V>.None;
        }
        public Option<U> OfType<U>()
        {
            if (HasValue && Value is U uValue)
                return Option.Some(uValue);
            else
                return Option<U>.None;
        }
    }
}
