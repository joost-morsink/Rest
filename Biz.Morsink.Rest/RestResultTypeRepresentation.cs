using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class RestResultTypeRepresentation : ITypeRepresentation
    {
        public static RestResultTypeRepresentation Instance { get; } = new RestResultTypeRepresentation();
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
    public class RestResultTypeRepresentation<T> : TaggedUnionTypeRepresentation<RestResult<T>, RestResultTypeRepresentation<T>.Representation>
    {
        public class Representation : TaggedUnionRepresentationType
        {
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
