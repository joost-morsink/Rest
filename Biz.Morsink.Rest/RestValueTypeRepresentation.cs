using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Concurrent;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A type representation for RestValues.
    /// This class creates a specific type representation for each generic parameter and delegates all calls to that instances.
    /// </summary>
    public class RestValueTypeRepresentation : ITypeRepresentation
    {
        /// <summary>
        /// A singleton instance.
        /// </summary>
        public static RestValueTypeRepresentation Instance { get; } = new RestValueTypeRepresentation();
        /// <summary>
        /// Constructor.
        /// </summary>
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
    /// <summary>
    /// A type represenation for IRestValues of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of value contained in the IRestValue.</typeparam>
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
        /// <summary>
        /// The actual representation class for an IRestValue&lt;T&gt;
        /// </summary>
        public class Representation
        {
            /// <summary>
            /// Contains the collection of links.
            /// </summary>
            public Link[] Links { get; set; }
            /// <summary>
            /// Contains the collection of embeddings.
            /// </summary>
            public Embedding[] Embeddings { get; set; }
            /// <summary>
            /// Contains the actual value.
            /// </summary>
            public T Value { get; set; }
        }
    } 
}
