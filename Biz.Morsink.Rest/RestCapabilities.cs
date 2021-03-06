﻿using Biz.Morsink.Rest.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A dictionary of method to request descriptions for usage in OPTIONS requests.
    /// </summary>
    public class RestCapabilities : Dictionary<string, RequestDescription[]>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public RestCapabilities() { }
        /// <summary>
        /// Constructor.
        /// Populates the dictionary based on the capabilities of a specified IRestRepository interface.
        /// </summary>
        /// <param name="repo">The repository to provide options for.</param>
        public RestCapabilities(IRestRepository repo, ITypeDescriptorCreator typeDescriptorCreator)
        {
            foreach (var capGroup in repo.GetCapabilities().GroupBy(c => c.Name))
            {
                this[capGroup.Key] = capGroup.Select(cap => new RequestDescription(
                     typeDescriptorCreator.GetDescriptor(cap.BodyType),
                     typeDescriptorCreator.GetDescriptor(cap.ParameterType),
                     typeDescriptorCreator.GetDescriptor(cap.ResultType)
                )).ToArray();
            }
        }
    }
}
