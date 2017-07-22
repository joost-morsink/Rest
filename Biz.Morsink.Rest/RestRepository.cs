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

        private static ImmutableDictionary<RestCapabilityDescriptorKey, (RestCapabilityDescriptor, IRestCapability<T>)> staticCapabilities;
        private ImmutableDictionary<RestCapabilityDescriptorKey, (RestCapabilityDescriptor, IRestCapability<T>)> capabilities;
        protected RestRepository()
        {
            staticCapabilities = staticCapabilities ?? getStaticCapabilities().ToImmutableDictionary(x => (RestCapabilityDescriptorKey)x.Item1, x => x);
            capabilities = staticCapabilities;

            IEnumerable<(RestCapabilityDescriptor, IRestCapability<T>)> getStaticCapabilities()
            {
                var ti = this.GetType().GetTypeInfo();
                return from itf in ti.ImplementedInterfaces
                       where CAPABILITY_TYPEINFO.IsAssignableFrom(itf)
                       select (RestCapabilityDescriptor.Create(itf), (IRestCapability<T>)this);
            }
        }
        protected void Register<C>(C capability)
            where C : IRestCapability<T>
        {
            var capitf = from itf in typeof(C).GetTypeInfo().ImplementedInterfaces
                         where itf.GetTypeInfo() != CAPABILITY_TYPEINFO && CAPABILITY_TYPEINFO.IsAssignableFrom(itf.GetTypeInfo())
                         let cap = RestCapabilityDescriptor.Create(itf)
                         select new KeyValuePair<RestCapabilityDescriptorKey, (RestCapabilityDescriptor,IRestCapability<T>)>(cap, (cap,capability));
            capabilities = capabilities.AddRange(capitf);
        }
        public virtual IEnumerable<RestCapabilityDescriptor> GetCapabilities()
        {
            return capabilities.Keys.Cast<RestCapabilityDescriptor>();
        }

        public virtual IRestCapability<T> GetCapability(RestCapabilityDescriptorKey key)
            => capabilities.TryGetValue(key, out var res) ? res.Item2 : null;

        public virtual C GetCapability<C>() where C : class, IRestCapability<T>
            => GetCapability(RestCapabilityDescriptorKey.Create(typeof(C))) as C;
    }
}
