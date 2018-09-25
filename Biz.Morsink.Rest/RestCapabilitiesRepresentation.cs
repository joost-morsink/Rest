using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Representation class for RestCapabilities.
    /// </summary>
    public class RestCapabilitiesRepresentation : SimpleTypeRepresentation<RestCapabilities, Dictionary<string, RequestDescription[]>>
    {
        public override RestCapabilities GetRepresentable(Dictionary<string, RequestDescription[]> representation)
        {
            throw new NotSupportedException();
        }

        public override Dictionary<string, RequestDescription[]> GetRepresentation(RestCapabilities item)
            => item;
    }
}
