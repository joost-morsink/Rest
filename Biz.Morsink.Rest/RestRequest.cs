using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class represents a Rest request
    /// </summary>
    public class RestRequest
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="capability">The requested capability.</param>
        /// <param name="address">An resource identity value is used to 'address' the capability on some resource.</param>
        /// <param name="parameters">Parameters for the request.</param>
        /// <param name="bodyParser">A function that is capable of parsing the body, given the body type is known.</param>
        /// <param name="metadata">Metadata for the Rest request.</param>
        public RestRequest(string capability, IIdentity address, RestParameterCollection parameters, Func<Type, object> bodyParser, TypeKeyedDictionary metadata)
        {
            Capability = capability;
            Address = address;
            Parameters = parameters ?? RestParameterCollection.Empty;
            BodyParser = bodyParser ?? (ty => new object());
            Metadata = metadata ?? TypeKeyedDictionary.Empty;
        }
        /// <summary>
        /// Creates a new RestRequest.
        /// </summary>
        /// <param name="capability">The requested capability.</param>
        /// <param name="address">An resource identity value is used to 'address' the capability on some resource.</param>
        /// <param name="parameters">Parameters for the request.</param>
        /// <param name="bodyParser">A function that is capable of parsing the body, given the body type is known.</param>
        /// <param name="metadata">Metadata for the Rest request.</param>
        /// <returns>A new RestRequest.</returns>
        public static RestRequest Create(string capability, IIdentity address, IEnumerable<KeyValuePair<string, string>> parameters = null, Func<Type, object> bodyParser = null, TypeKeyedDictionary metadata = null)
            => new RestRequest(capability, address, RestParameterCollection.Create(parameters), bodyParser, metadata);
        /// <summary>
        /// Creates a new RestRequest.
        /// </summary>
        /// <param name="capability">The requested capability.</param>
        /// <param name="address">An resource identity value is used to 'address' the capability on some resource.</param>
        /// <param name="parameters">Parameters for the request.</param>
        /// <param name="bodyParser">A function that is capable of parsing the body, given the body type is known.</param>
        /// <param name="metadata">Metadata for the Rest request.</param>
        /// <returns>A new RestRequest.</returns>        
        public static RestRequest CreateWithTuples(string capability, IIdentity address, IEnumerable<(string, string)> parameters = null, Func<Type, object> bodyParser = null, TypeKeyedDictionary metadata = null)
            => new RestRequest(capability, address, RestParameterCollection.Create(parameters), bodyParser, metadata);

        /// <summary>
        /// Gets the capability name.
        /// </summary>
        public string Capability { get; }
        /// <summary>
        /// Gets the address resource's identity value.
        /// </summary>
        public IIdentity Address { get; }
        /// <summary>
        /// Gets the body parser.
        /// </summary>
        public Func<Type, object> BodyParser { get; }
        /// <summary>
        /// Gets the parameters for the Rest request. 
        /// The concept of parameters map to query string parameters.
        /// </summary>
        public RestParameterCollection Parameters { get; }
        /// <summary>
        /// Gets the metadata for the Rest request.
        /// The concept of metadata map to HTTP headers.
        /// </summary>
        public TypeKeyedDictionary Metadata { get; }
        /// <summary>
        /// Creates a new RestRequest with added metadata.
        /// </summary>
        /// <typeparam name="X">The type of metadata to include in the request.</typeparam>
        /// <param name="data">An instance of metadata to include in the request.</param>
        /// <returns>A new RestRequest with added metadata.</returns>
        public RestRequest AddMetadata<X>(X data)
            => new RestRequest(Capability, Address, Parameters, BodyParser, Metadata.Add(data));
    }
}