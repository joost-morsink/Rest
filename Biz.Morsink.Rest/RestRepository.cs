using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// An abstract base class for the implementation of Rest repositories.
    /// This base class provides a cache for the repository's capabilities and registers all statically implemented capabilities in the cache.
    /// </summary>
    /// <typeparam name="T">The resource type for the repository.</typeparam>
    public abstract class RestRepository<T> : IRestRepository<T>, IRestRequestContainer
    {
        private static readonly TypeInfo CAPABILITY_TYPEINFO = typeof(IRestCapability<T>).GetTypeInfo();

        private static ImmutableDictionary<RestCapabilityDescriptorKey, ImmutableList<RestCapability<T>>> staticCapabilities;
        private ImmutableDictionary<RestCapabilityDescriptorKey, ImmutableList<RestCapability<T>>> capabilities;
        /// <summary>
        /// Constructor.
        /// </summary>
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
                       let map = ti.GetInterfaceMap(itf)
                       let desc = RestCapabilityDescriptor.Create(itf)
                       where desc != null
                       let meth = map.TargetMethods.Length == 1 ? map.TargetMethods[0] : null
                       select new RestCapability<T>(desc.WithMethod(meth), (IRestCapability<T>)this);
            }
        }
        /// <summary>
        /// Dynamically register a repository's capability.
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <param name="capability"></param>
        protected void Register<C>(C capability)
            where C : IRestCapability<T>
        {
            var ti = typeof(C).GetTypeInfo();
            var capGroups = from itf in ti.ImplementedInterfaces
                            where itf.GetTypeInfo() != CAPABILITY_TYPEINFO && CAPABILITY_TYPEINFO.IsAssignableFrom(itf.GetTypeInfo())
                            let map = ti.GetInterfaceMap(itf)
                            let cap = RestCapabilityDescriptor.Create(itf)
                            group (cap, map) by (RestCapabilityDescriptorKey)cap into g
                            select new { Key = g.Key, Value = g.Select(c => new RestCapability<T>(c.cap.WithMethod(c.map.TargetMethods[0]), capability)) };
            foreach (var capGroup in capGroups)
            {
                if (capabilities.TryGetValue(capGroup.Key, out var lst))
                    capabilities = capabilities.SetItem(capGroup.Key, lst.AddRange(capGroup.Value));
                else
                    capabilities = capabilities.Add(capGroup.Key, capGroup.Value.ToImmutableList());
            }
        }
        /// <summary>
        /// Dynamically register a repository's dynamic capability.
        /// </summary>
        /// <param name="capability"></param>
        protected void RegisterDynamic(IRestCapability<T> capability)
        {
            var capGroups = from itf in capability.GetType().GetTypeInfo().ImplementedInterfaces
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
        /// <summary>
        /// Register a single explicitly specified capability.
        /// </summary>
        /// <param name="capability">The capability to register.</param>
        protected void RegisterSingle(RestCapability<T> capability)
        {
            if (capabilities.TryGetValue(capability.Descriptor, out var lst))
                capabilities = capabilities.SetItem(capability.Descriptor, lst.Add(capability));
            else
                capabilities = capabilities.Add(capability.Descriptor, ImmutableList<RestCapability<T>>.Empty.Add(capability));
        }
        /// <summary>
        /// Register a single explicitly specified capability.
        /// </summary>
        /// <param name="capabilityDescriptor">A capability descriptor for the capability.</param>
        /// <param name="capability">The actual capability implementation instance.</param>
        protected void RegisterSingle(RestCapabilityDescriptor capabilityDescriptor, IRestCapability<T> capability)
        {
            RegisterSingle(new RestCapability<T>(capabilityDescriptor, capability));
        }
        /// <summary>
        /// Gets capability descriptors for all the capabilities the repository provides.
        /// </summary>
        /// <returns>Capability descriptors for all the capabilities the repository provides.</returns>
        public virtual IEnumerable<RestCapabilityDescriptor> GetCapabilities()
        {
            return capabilities.Keys.Cast<RestCapabilityDescriptor>();
        }
        /// <summary>
        /// Gets a list of capability descriptors that match the given capability descriptor key.
        /// </summary>
        /// <param name="capability">The key for capability retrieval.</param>
        /// <returns>A list of capability descriptors that match the given capability descriptor key.</returns>
        public virtual IReadOnlyList<RestCapability<T>> GetCapabilities(RestCapabilityDescriptorKey key)
            => capabilities.TryGetValue(key, out var res) ? res : ImmutableList<RestCapability<T>>.Empty;
        /// <summary>
        /// Gets a typed capability for the repository.
        /// </summary>
        /// <typeparam name="C">The capability type.</typeparam>
        /// <returns>An instance of the capability if it is supported, or null otherwise.</returns>
        public virtual C GetCapability<C>() where C : class, IRestCapability<T>
            => GetCapabilities(RestCapabilityDescriptorKey.Create(typeof(C))).Select(cap => cap.Instance as C).FirstOrDefault();
        /// <summary>
        /// The entity/resource type for the repository.
        /// </summary>
        Type IRestRepository.EntityType => typeof(T);
        /// <summary>
        /// This property returns a collection of types that are used by this repository, based on an overridable implementation.
        /// This information can be used to populate schema information
        /// </summary>
        IEnumerable<Type> IRestRepository.SchemaTypes => GetSchemaTypes();

        /// <summary>
        /// Gets the raw Rest request this repository instance was constructed for.
        /// </summary>
        public RestRequest Request { get; set; }

        /// <summary>
        /// Default implementation for schematypes. 
        /// Returns all the relevant types used by Rest capability interfaces.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<Type> GetSchemaTypes()
            => GetCapabilities()
                .SelectMany(d => new[] { d.EntityType, d.BodyType, d.ParameterType, d.ResultType })
                .Where(t => t != null)
                .Distinct();
        /// <summary>
        /// This method should implement any post processing on responses generated by this repository.
        /// </summary>
        /// <param name="response">The response as generated by this repository.</param>
        /// <returns>An asynchronous, possibly mutated, version of the response.</returns>
        public virtual ValueTask<RestResponse> ProcessResponse(RestResponse response)
            => new ValueTask<RestResponse>(response);
    }
}
