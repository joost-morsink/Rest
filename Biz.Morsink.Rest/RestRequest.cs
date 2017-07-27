using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest
{
    public class RestRequest
    {
        public RestRequest(IIdentity address, Func<Type, object> bodyParser, IEnumerable<KeyValuePair<string,string>> extraData)
        {
            Address = address;
            BodyParser = bodyParser;
            ExtraData = extraData.ToLookup(x => x.Key, x => x.Value); 
        }

        public IIdentity Address { get; }
        public object BodyParser { get; }
        public ILookup<string, string> ExtraData { get; }
    }
}