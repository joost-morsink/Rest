using Newtonsoft.Json;

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
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings();

    }
}