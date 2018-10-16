using System;
using Newtonsoft.Json.Serialization;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    internal class HalContractResolver : DefaultContractResolver
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