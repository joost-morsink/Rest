using Newtonsoft.Json;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Configuration for the JsonHttpConverter
    /// </summary>
    public class JsonHttpConverterOptions
    {

        /// <summary>
        /// Get or sets the JsonSerializerSettings for the JsonHttpConverter.
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings();
    }
}