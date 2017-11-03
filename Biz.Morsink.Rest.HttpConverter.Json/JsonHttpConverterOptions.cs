using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        public NamingStrategy NamingStrategy { get; set; }
        public JsonHttpConverterOptions ApplyDefaultNamingStrategy()
        {
            NamingStrategy = new DefaultNamingStrategy();
            return this;
        }
        public JsonHttpConverterOptions ApplyCamelCaseNamingStrategy()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
            return this;
        }
    }
}