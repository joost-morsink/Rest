using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Value class for links.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// Creates a new Link.
        /// </summary>
        /// <param name="relType">Identifier for the relation type.</param>
        /// <param name="target">The identity value of the target of the link.</param>
        /// <param name="parameters">Optional parameters needed to retrieve the link.</param>
        /// <param name="capability">The capability needed to traverse the link.</param>
        /// <returns>A Link object.</returns>
        public static Link Create(string relType, IIdentity target, object parameters = null, Type capability = null)
            => new Link(relType, target, parameters, capability);
        /// <summary>
        /// Creates a new typed Link.
        /// </summary>
        /// <typeparam name="T">The resource type of the link target.</typeparam>
        /// <param name="relType">Identifier for the relation type.</param>
        /// <param name="target">The identity value of the target of the link.</param>
        /// <param name="parameters">Optional parameters needed to retrieve the link.</param>
        /// <param name="capability">The capability needed to traverse the link.</param>
        /// <returns>A Link&lt;T&gt; object.</returns>
        public static Link<T> Create<T>(string relType, IIdentity<T> target, object parameters = null, Type capability = null)
            where T : class
            => new Link<T>(relType, target, parameters, capability);
        
        internal Link(string relType, IIdentity target, object parameters, Type capability)
        {
            RelType = relType;
            Target = target;
            Parameters = parameters;
            Capability = capability ?? typeof(IRestGet<,>).MakeGenericType(target.ForType, parameters?.GetType() ?? typeof(Empty));
        }
        /// <summary>
        /// Gets the relation type.
        /// </summary>
        public string RelType { get; }
        /// <summary>
        /// Gets the identity value for the target of the link.
        /// </summary>
        public IIdentity Target { get; }
        /// <summary>
        /// Gets the parameters needed to traverse the link.
        /// </summary>
        public object Parameters { get; }
        /// <summary>
        /// Gets the capability type needed to traverse the link.
        /// </summary>
        public Type Capability { get; }
        /// <summary>
        /// Tries to upcast the Link object to a typed Link&lt;T&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Link<T> As<T>()
            where T : class
        {
            if (this is Link<T> res)
                return res;
            var id = Target as IIdentity<T>;
            return id == null ? null : new Link<T>(RelType, id, Parameters, Capability);
        }
    }
    /// <summary>
    /// Value class for typed links.
    /// </summary>
    /// <typeparam name="T">The resource type of the link target.</typeparam>
    public class Link<T> : Link
        where T : class
    {
        internal Link(string relType, IIdentity<T> target, object parameters, Type capability) : base(relType, target, parameters, capability ?? typeof(IRestGet<T, Empty>))
        { }
        /// <summary>
        /// Gets the identity value for the target of the link.
        /// </summary>
        public new IIdentity<T> Target => (IIdentity<T>)base.Target;
    }
}