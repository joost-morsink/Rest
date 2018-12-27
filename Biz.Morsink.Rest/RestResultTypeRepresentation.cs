using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A Type representation for RestResults.
    /// The class constructs a specific type representation for each generic parameter and delegates all calls to that instance.
    /// </summary>
    public class RestResultTypeRepresentation : ITypeRepresentation
    {
        /// <summary>
        /// A singleton instance.
        /// </summary>
        public static RestResultTypeRepresentation Instance { get; } = new RestResultTypeRepresentation();

        /// <summary>
        /// Constructor.
        /// </summary>
        private RestResultTypeRepresentation() { }
        private ConcurrentDictionary<Type, ITypeRepresentation> typeReprs = new ConcurrentDictionary<Type, ITypeRepresentation>();
        private ITypeRepresentation GetByRepresentation(Type representationType)
        {
            var key = representationType?.GetGeneric(typeof(RestResultTypeRepresentation<>.Representation));
            if (key == null)
                return null;
            return typeReprs.GetOrAdd(key, k => (ITypeRepresentation)Activator.CreateInstance(typeof(RestResultTypeRepresentation<>).MakeGenericType(k)));
        }
        private ITypeRepresentation GetByRepresentable(Type representationType)
        {
            var key = representationType?.GetGeneric(typeof(RestResult<>));
            if (key == null)
                return null;
            return typeReprs.GetOrAdd(key, k => (ITypeRepresentation)Activator.CreateInstance(typeof(RestResultTypeRepresentation<>).MakeGenericType(k)));
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
    /// A type representation class for RestResults of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the result in the RestResult.</typeparam>
    public class RestResultTypeRepresentation<T> : TaggedUnionTypeRepresentation<RestResult<T>, RestResultTypeRepresentation<T>.Representation>
    {
        /// <summary>
        /// An actual representation class for RestResult&lt;T&gt;.
        /// </summary>
        public class Representation : TaggedUnionRepresentationType
        {
            /// <summary>
            /// Constructor. 
            /// Calls the base constructor with all type and tag information.
            /// </summary>
            public Representation(): base(typeof(RestResult<T>),
                ("Success", typeof(RestResult<T>.Success)),
                ("BadRequest", typeof(RestResult<T>.Failure.BadRequest)),
                ("Error", typeof(RestResult<T>.Failure.Error)),
                ("NotExecuted", typeof(RestResult<T>.Failure.NotExecuted)),
                ("NotFound", typeof(RestResult<T>.Failure.NotFound)),
                ("NotNecessary", typeof(RestResult<T>.Redirect.NotNecessary)),
                ("Permanent", typeof(RestResult<T>.Redirect.Permanent)),
                ("Temporary", typeof(RestResult<T>.Redirect.Temporary)))
            { }
        }
    }

}
