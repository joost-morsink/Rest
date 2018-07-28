using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public abstract class SimpleTypeRepresentation<T, R> : ITypeRepresentation
    {
        public abstract R GetRepresentation(T item);
        public abstract T GetRepresentable(R representation);

        public virtual Type GetRepresentableType(Type type)
            => type == typeof(R) ? typeof(T) : null;
        public virtual Type GetRepresentationType(Type type)
            => typeof(T).IsAssignableFrom(type) ? typeof(R) : null;

        object ITypeRepresentation.GetRepresentable(object rep)
            => rep is R r ? GetRepresentable(r) : default;

        object ITypeRepresentation.GetRepresentation(object obj)
            => obj is T t ? GetRepresentation(t) : default;

        bool ITypeRepresentation.IsRepresentable(Type type)
            => GetRepresentationType(type) != null;

        bool ITypeRepresentation.IsRepresentation(Type type)
            => GetRepresentableType(type) != null;
    }
}
