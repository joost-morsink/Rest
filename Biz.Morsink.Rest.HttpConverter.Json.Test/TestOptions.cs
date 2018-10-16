using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    public class TestOptions : IOptions<JsonHttpConverterOptions>
    {
        private readonly JsonHttpConverterOptions options;
        public TestOptions()
        {
            var x = new JsonHttpConverterOptions().ApplyDefaultNamingStrategy().UseEmbeddings().UseLinkLocation("_Links");
            x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options = x;
        }
        public JsonHttpConverterOptions Value => options;
    }
}
