using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest
{
    public abstract class RestRepository<T> : IRestRepository<T>
    {
        private static readonly TypeInfo CAPABILITY_TYPEINFO = typeof(IRestCapability<T>).GetTypeInfo();

        private static ImmutableDictionary<Type, IRestCapability<T>> staticCapabilities;
        private ImmutableDictionary<Type, IRestCapability<T>> capabilities;
        protected RestRepository()
        {
            staticCapabilities = staticCapabilities ?? getStaticCapabilities().ToImmutableDictionary(x => x.Item1, x => x.Item2);
            capabilities = staticCapabilities;

            IEnumerable<(Type, IRestCapability<T>)> getStaticCapabilities()
            {
                var ti = this.GetType().GetTypeInfo();
                return from itf in ti.ImplementedInterfaces
                       where CAPABILITY_TYPEINFO.IsAssignableFrom(itf)
                       select (itf, (IRestCapability<T>)this);
            }
        }
        protected void Register<C>(C capability)
            where C : IRestCapability<T>
        {
            var capitf = typeof(C).GetTypeInfo().ImplementedInterfaces
                .Where(itf => itf.GetTypeInfo() != CAPABILITY_TYPEINFO && CAPABILITY_TYPEINFO.IsAssignableFrom(itf.GetTypeInfo()))
                .Select(itf => new KeyValuePair<Type, IRestCapability<T>>(itf, capability));
            capabilities = capabilities.AddRange(capitf);
        }
        public virtual IEnumerable<Type> GetCapabilities()
        {
            return capabilities.Keys;
        }

        public virtual IRestCapability<T> GetCapability(Type capability)
            => capabilities.TryGetValue(capability, out var res) ? res : null;

        public virtual C GetCapability<C>() where C : class, IRestCapability<T>
            => GetCapability(typeof(C)) as C;
    }
}
