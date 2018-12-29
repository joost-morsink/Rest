using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// A contract resolver for Json serialization.
    /// Most resolving is done in the intermediate serialization object layer, but DateTime is considered a primitive.
    /// </summary>
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
