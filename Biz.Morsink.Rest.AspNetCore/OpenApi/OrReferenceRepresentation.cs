using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// Type representation for instances of OrReference&lt;T&gt;
    /// </summary>
    public class OrReferenceRepresentation : ITypeRepresentation
    {
        /// <summary>
        /// Gets a OrReference&lt;T&gt; for the UnionRepresentation&lt;Reference, T&gt;.
        /// </summary>
        /// <param name="rep">The 'representation' instance of type UnionRepresentation.</param>
        /// <returns>A 'representable' instance of type OrReference&lt;T&gt;.</returns>
        public object GetRepresentable(object rep)
        {
            var u = (UnionRepresentation)rep;
            var item = u.GetItem();
            if (item is Reference)
                return Activator.CreateInstance(typeof(OrReference<>.ReferenceImpl).MakeGenericType(u.GetTypes().ElementAt(1)), item);
            else
                return Activator.CreateInstance(typeof(OrReference<>.ItemImpl).MakeGenericType(u.GetTypes().ElementAt(1)), item);
        }
        public Type GetRepresentableType(Type type)
        {
            var (reference, t) = type.GetGenerics2(typeof(UnionRepresentation<,>));
            if (reference != null && reference == typeof(Dictionary<string, string>))
                return typeof(OrReference<>).MakeGenericType(t);
            else
                return null;
        }
        /// <summary>
        /// Gets a UnionRepresentation&lt;Reference, T&gt; for the OrReference&lt;T&gt;.
        /// </summary>
        /// <param name="rep">A 'representable' instance of type OrReference&lt;Tgt;.</param>
        /// <returns>A 'representation' instance of type UnionRepresentation.</returns>
        public object GetRepresentation(object obj)
        {
            var gen = obj?.GetType().GetGeneric(typeof(OrReference<>));
            if (gen == null)
                return null;
            if (obj.GetType().GetGenericTypeDefinition() == typeof(OrReference<>.ReferenceImpl))
                return UnionRepresentation.FromOptions(typeof(Reference), gen).Create(obj.GetType().GetProperty(nameof(OrReference<object>.ReferenceImpl.Reference)).GetValue(obj));
            else
                return UnionRepresentation.FromOptions(typeof(Reference), gen).Create(obj.GetType().GetProperty(nameof(OrReference<object>.ItemImpl.Item)).GetValue(obj));
        }

        public Type GetRepresentationType(Type type)
        {
            var typeParam = type.GetGeneric(typeof(OrReference<>));
            if (typeParam == null)
                return null;
            return typeof(UnionRepresentation<,>).MakeGenericType(typeof(Reference), typeParam);
        }

        public bool IsRepresentable(Type type)
            => GetRepresentationType(type) != null;

        public bool IsRepresentation(Type type)
            => GetRepresentableType(type) != null;
    }
}
