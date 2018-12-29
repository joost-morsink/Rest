using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    /// <summary>
    /// Options class for the Hal Json Http converter.
    /// </summary>
    public class HalJsonConverterOptions
    {
        /// <summary>
        /// Serializer settings for the Newtonsoft.Json library.
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings { ContractResolver = new HalContractResolver() };
        /// <summary>
        /// A naming strategy for json serialization.
        /// </summary>
        public NamingStrategy NamingStrategy { get; set; } = new CamelCaseNamingStrategy();
    }
}