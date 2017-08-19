using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest
{
    public class RestRequest
    {
        public RestRequest(string capability, IIdentity address, RestParameterCollection parameters , Func<Type,object> bodyParser, RestParameterCollection metadata)
        {
            Capability = capability;
            Address = address;
            Parameters = parameters ?? RestParameterCollection.Empty;
            BodyParser = bodyParser ?? (ty => new object());
            Metadata = metadata ?? RestParameterCollection.Empty;
        }
        public static RestRequest Create(string capability, IIdentity address, IEnumerable<KeyValuePair<string, string>> parameters = null, Func<Type, object> bodyParser = null, IEnumerable<KeyValuePair<string, string>> metadata = null)
            => new RestRequest(capability, address, RestParameterCollection.Create(parameters), bodyParser, RestParameterCollection.Create(metadata));
        public static RestRequest CreateWithTuples(string capability, IIdentity address, IEnumerable<(string, string)> parameters = null, Func<Type, object> bodyParser = null, IEnumerable<(string, string)> metadata = null)
            => new RestRequest(capability, address, RestParameterCollection.Create(parameters), bodyParser, RestParameterCollection.Create(metadata));

        public string Capability { get; }
        public IIdentity Address { get; }
        public Func<Type, object> BodyParser { get; }
        /// <summary>
        /// TODO: Maps to query string
        /// </summary>
        public RestParameterCollection Parameters { get; }
        /// <summary>
        /// TODO: Maps to HTTP headers
        /// </summary>
        public RestParameterCollection Metadata { get; }
    }
}