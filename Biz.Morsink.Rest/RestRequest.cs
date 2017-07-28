using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest
{
    public class RestRequest
    {
        public RestRequest(string capability, IIdentity address, IEnumerable<KeyValuePair<string, string>> extraData, Func<Type, object> bodyParser)
        {
            Capability = capability;
            Address = address;
            ExtraData = extraData.ToLookup(x => x.Key, x => x.Value);
            BodyParser = bodyParser;
        }
        public RestRequest(string capability, IIdentity address, IEnumerable<(string, string)> extraData, Func<Type, object> bodyParser)
        {
            Capability = capability;
            Address = address;
            ExtraData = extraData.ToLookup(x => x.Item1, x => x.Item2);
            BodyParser = bodyParser;
        }
        public string Capability { get; }
        public IIdentity Address { get; }
        public Func<Type, object> BodyParser { get; }
        public ILookup<string, string> ExtraData { get; }
    }
}