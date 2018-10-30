using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A collection interface for all applicable ITypeRepresentation instances.
    /// </summary>
    public interface ITypeRepresentations 
    {
        /// <summary>
        /// Get all individual type representations.
        /// </summary>
        IEnumerable<ITypeRepresentation> GetTypeRepresentations();
        /// <summary>
        /// Returns a single type representation based on all underlying representations.
        /// The returned representation should implement a default (identity) representation, meaning representation is a total function.
        /// </summary>
        ITypeRepresentation AsTypeRepresentation();
    }
}
