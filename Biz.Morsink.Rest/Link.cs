using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class Link
    {
        public static Link Create(string relType, IIdentity target, Type capability = null)
            => new Link(relType, target, capability);
        public static Link<T> Create<T>(string relType, IIdentity<T> target, Type capability = null)
            where T : class
            => new Link<T>(relType, target, capability);
        internal Link(string relType, IIdentity target, Type capability)
        {
            RelType = relType;
            Target = target;
            Capability = capability ?? typeof(IRestGet<>).MakeGenericType(target.ForType);
        }
        public string RelType { get; }
        public IIdentity Target { get; }
        public Type Capability { get; }
        public Link<T> As<T>()
            where T : class
        {
            var res = this as Link<T>;
            if (res != null)
                return res;
            var id = Target as IIdentity<T>;
            return id == null ? null : new Link<T>(RelType, id, Capability);
        }
    }
    public class Link<T> : Link
        where T : class
    {
        internal Link(string relType, IIdentity<T> target, Type capability) : base(relType, target, capability ?? typeof(IRestGet<T>))
        { }
        public new IIdentity<T> Target => (IIdentity<T>)base.Target;
    }
}