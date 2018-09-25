using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// Representation class for ExpandoObject
    /// </summary>
    public class ExpandoObjectRepresentation : SimpleTypeRepresentation<ExpandoObject, IDictionary<string, object>>
    {
        public override ExpandoObject GetRepresentable(IDictionary<string, object> representation)
        {
            var res = new ExpandoObject();
            var dict = (IDictionary<string, object>)res;
            foreach (var kvp in representation)
                dict[kvp.Key] = kvp.Value;
            return res;
        }

        public override IDictionary<string, object> GetRepresentation(ExpandoObject item)
            => item;
    }
}
