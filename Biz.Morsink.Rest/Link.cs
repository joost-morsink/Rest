using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class Link
    {
        public static Link Create(string relType, IIdentity target, object parameters = null, Type capability = null)
            => new Link(relType, target, parameters, capability);
        public static Link<T> Create<T>(string relType, IIdentity<T> target, object parameters = null, Type capability = null)
            where T : class
            => new Link<T>(relType, target, parameters, capability);
        internal Link(string relType, IIdentity target, object parameters, Type capability)
        {
            RelType = relType;
            Target = target;
            Parameters = parameters;
            Capability = capability ?? typeof(IRestGet<,>).MakeGenericType(target.ForType, parameters?.GetType() ?? typeof(NoParameters));
        }
        public string RelType { get; }
        public IIdentity Target { get; }
        public object Parameters { get; }
        public Type Capability { get; }
        public Link<T> As<T>()
            where T : class
        {
            if (this is Link<T> res)
                return res;
            var id = Target as IIdentity<T>;
            return id == null ? null : new Link<T>(RelType, id, Parameters, Capability);
        }
    }
    public class Link<T> : Link
        where T : class
    {
        internal Link(string relType, IIdentity<T> target, object parameters, Type capability) : base(relType, target, parameters, capability ?? typeof(IRestGet<T, NoParameters>))
        { }
        public new IIdentity<T> Target => (IIdentity<T>)base.Target;
    }
}