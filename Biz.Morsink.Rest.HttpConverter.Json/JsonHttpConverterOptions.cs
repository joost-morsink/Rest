using Newtonsoft.Json;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    public class JsonHttpConverterOptions
    {
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings();
    }
}