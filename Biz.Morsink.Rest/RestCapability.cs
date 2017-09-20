using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Struct containing a rest capability with its descriptor.
    /// </summary>
    /// <typeparam name="T">The resource type for the capability.</typeparam>
    public struct RestCapability<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="descriptor">The capability descriptor.</param>
        /// <param name="instance">The capability instance.</param>
        public RestCapability(RestCapabilityDescriptor descriptor, IRestCapability<T> instance)
        {
            Descriptor = descriptor;
            Instance = instance;
        }
        /// <summary>
        /// Gets the capability descriptor.
        /// </summary>
        public RestCapabilityDescriptor Descriptor { get; }
        /// <summary>
        /// The capability instance.
        /// </summary>
        public IRestCapability<T> Instance { get; }
        /// <summary>
        /// Creates a Func delegate for the capability method on the specified instance.
        /// </summary>
        /// <returns>A Func delegate for the capability method on the specified instance.</returns>
        public Delegate CreateDelegate()
            => Descriptor.CreateDelegate(Instance);
    }
}
