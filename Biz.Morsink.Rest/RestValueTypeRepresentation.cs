using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Concurrent;

namespace Biz.Morsink.Rest
{
    public class RestValueTypeRepresentation : ITypeRepresentation
    {
        public static RestValueTypeRepresentation Instance { get; } = new RestValueTypeRepresentation();
        private RestValueTypeRepresentation() { }
        private ConcurrentDictionary<Type, ITypeRepresentation> typeReprs = new ConcurrentDictionary<Type, ITypeRepresentation>();
        private ITypeRepresentation GetByRepresentation(Type representationType)
        {
            var key = representationType?.GetGeneric(typeof(RestValueTypeRepresentation<>.Representation));
            if (key == null)
                return null;
            return typeReprs.GetOrAdd(key, k => (ITypeRepresentation)Activator.CreateInstance(typeof(RestValueTypeRepresentation<>).MakeGenericType(k)));
        }
        private ITypeRepresentation GetByRepresentable(Type representationType)
        {
            var key = representationType?.GetGeneric(typeof(IRestValue<>));
            if (key == null)
                return null;
            return typeReprs.GetOrAdd(key, k => (ITypeRepresentation)Activator.CreateInstance(typeof(RestValueTypeRepresentation<>).MakeGenericType(k)));
        }

        public object GetRepresentable(object rep, Type specific)
        {
            if (specific == null)
                return GetByRepresentation(rep.GetType())?.GetRepresentable(rep, specific);
            else
                return GetByRepresentable(specific)?.GetRepresentable(rep, specific);
        }

        public Type GetRepresentableType(Type type)
            => GetByRepresentation(type)?.GetRepresentableType(type);

        public object GetRepresentation(object obj)
            => GetByRepresentable(obj.GetType())?.GetRepresentation(obj);

        public Type GetRepresentationType(Type type)
            => GetByRepresentable(type)?.GetRepresentationType(type);

        public bool IsRepresentable(Type type)
            => GetByRepresentable(type)?.IsRepresentable(type) ?? false;

        public bool IsRepresentation(Type type)
            => GetByRepresentation(type)?.IsRepresentation(type) ?? false;
    }
    public class RestValueTypeRepresentation<T> : SimpleTypeRepresentation<IRestValue<T>, RestValueTypeRepresentation<T>.Representation>
    {
        public override IRestValue<T> GetRepresentable(Representation representation)
            => new RestValue<T>(representation.Value, representation.Links, representation.Embeddings);

        public override Representation GetRepresentation(IRestValue<T> item)
            => new Representation
            {
                Links = item.Links.ToArray(),
                Embeddings = item.Embeddings.ToArray(),
                Value = item.Value
            };

        public class Representation
        {
            public Link[] Links { get; set; }
            public Embedding[] Embeddings { get; set; }
            public T Value { get; set; }
        }
    } 
}
