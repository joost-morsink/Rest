using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    internal static class InternalUtils
    {
        public static JToken WriteJson(this HalJsonRestSerializer serializer, object o)
        {
            var res = new JObject();
            using (var wri = res.CreateWriter())
            {
                wri.WritePropertyName("x");
                serializer.WriteJson(wri, o);
            }
            return res["x"];
        }
        public static T ReadJson<T>(this HalJsonRestSerializer serializer, JToken o)
        {
            using (var rdr = o.CreateReader())
                return serializer.ReadJson<T>(rdr);
        }
    }
}
