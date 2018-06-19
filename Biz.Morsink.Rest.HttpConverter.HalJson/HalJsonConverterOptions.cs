using Newtonsoft.Json;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public class HalJsonConverterOptions
    {
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings();

    }
}