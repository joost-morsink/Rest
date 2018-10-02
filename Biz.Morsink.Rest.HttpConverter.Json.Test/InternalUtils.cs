using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    internal static class InternalUtils
    {
        public static JObject WriteJson(this JsonRestSerializer serializer, object o)
        {
            var res = new JObject();
            using (var wri = res.CreateWriter())
            {
                wri.WritePropertyName("x");
                serializer.WriteJson(wri, o);
            }
            return res["x"] as JObject;
        }
        public static T ReadJson<T>(this JsonRestSerializer serializer, JObject o)
        {
            using (var rdr = o.CreateReader())
                return serializer.ReadJson<T>(rdr);
        }
    }
}
