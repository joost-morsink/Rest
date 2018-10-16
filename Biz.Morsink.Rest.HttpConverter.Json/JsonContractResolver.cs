using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    public class JsonContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            var cntr = base.CreateContract(objectType);
            if (objectType == typeof(DateTime) || objectType == typeof(DateTime?))
                cntr.Converter = new DateConverter();
            return cntr;
        }
    }
}
