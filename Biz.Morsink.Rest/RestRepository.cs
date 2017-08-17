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

        private static ImmutableDictionary<RestCapabilityDescriptorKey, ImmutableList<RestCapability<T>>> staticCapabilities;
        private ImmutableDictionary<RestCapabilityDescriptorKey, ImmutableList<RestCapability<T>>> capabilities;
        protected RestRepository()
        {
            staticCapabilities = staticCapabilities
                ?? getStaticCapabilities()
                    .GroupBy(x => (RestCapabilityDescriptorKey)x.Descriptor)
                    .ToImmutableDictionary(g => g.Key, g => g.ToImmutableList());
            capabilities = staticCapabilities;

            IEnumerable<RestCapability<T>> getStaticCapabilities()
            {
                var ti = this.GetType().GetTypeInfo();
                return from itf in ti.ImplementedInterfaces
                       where CAPABILITY_TYPEINFO.IsAssignableFrom(itf) && itf.GetTypeInfo() != CAPABILITY_TYPEINFO
                       let desc = RestCapabilityDescriptor.Create(itf)
                       where desc != null
                       select new RestCapability<T>(desc, (IRestCapability<T>)this);
            }
        }
        protected void Register<C>(C capability)
            where C : IRestCapability<T>
        {
            var capGroups = from itf in typeof(C).GetTypeInfo().ImplementedInterfaces
                                where itf.GetTypeInfo() != CAPABILITY_TYPEINFO && CAPABILITY_TYPEINFO.IsAssignableFrom(itf.GetTypeInfo())
                                let cap = RestCapabilityDescriptor.Create(itf)
                                group cap by (RestCapabilityDescriptorKey)cap into g
                                select new { Key = g.Key, Value = g.Select(c => new RestCapability<T>(c, capability)) };
            foreach (var capGroup in capGroups)
            {
                if (capabilities.TryGetValue(capGroup.Key, out var lst))
                    capabilities = capabilities.SetItem(capGroup.Key, lst.AddRange(capGroup.Value));
                else
                    capabilities = capabilities.Add(capGroup.Key, capGroup.Value.ToImmutableList());
            }
        }
        public virtual IEnumerable<RestCapabilityDescriptor> GetCapabilities()
        {
            return capabilities.Keys.Cast<RestCapabilityDescriptor>();
        }

        public virtual IReadOnlyList<RestCapability<T>> GetCapabilities(RestCapabilityDescriptorKey key)
            => capabilities.TryGetValue(key, out var res) ? res : ImmutableList<RestCapability<T>>.Empty;

        public virtual C GetCapability<C>() where C : class, IRestCapability<T>
            => GetCapabilities(RestCapabilityDescriptorKey.Create(typeof(C))) as C;

        Type IRestRepository.EntityType => typeof(T);
    }
}
