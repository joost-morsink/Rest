﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface to indicate a Rest repository.
    /// </summary>
    public interface IRestRepository
    {
        /// <summary>
        /// Gets capability descriptors for all the capabilities the repository provides.
        /// </summary>
        /// <returns>Capability descriptors for all the capabilities the repository provides.</returns>
        IEnumerable<RestCapabilityDescriptor> GetCapabilities();
        /// <summary>
        /// The entity/resource type for the repository.
        /// </summary>
        Type EntityType { get; }
        /// <summary>
        /// This property should return a collection of types that are used by this repository.
        /// This information can be used to populate schema information.
        /// </summary>
        IEnumerable<Type> SchemaTypes { get; }

        ValueTask<RestResponse> ProcessResponse(RestResponse response); 
    }
    /// <summary>
    /// Generic interface to indicate a Rest repository of a certain resource type.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    public interface IRestRepository<T> : IRestRepository
    {
        /// <summary>
        /// Gets a list of capability descriptors that match the given capability descriptor key.
        /// </summary>
        /// <param name="capability">The key for capability retrieval.</param>
        /// <returns>A list of capability descriptors that match the given capability descriptor key.</returns>
        IReadOnlyList<RestCapability<T>> GetCapabilities(RestCapabilityDescriptorKey capability);
        /// <summary>
        /// Gets a typed capability for the repository.
        /// </summary>
        /// <typeparam name="C">The capability type.</typeparam>
        /// <returns>An instance of the capability if it is supported, or null otherwise.</returns>
        C GetCapability<C>()
            where C : class, IRestCapability<T>;
    }

}
