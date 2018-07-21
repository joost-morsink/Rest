using Microsoft.Extensions.Options;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    public class TestOptions : IOptions<JsonHttpConverterOptions>
    {
        public JsonHttpConverterOptions Value => new JsonHttpConverterOptions().ApplyDefaultNamingStrategy().UseEmbeddings().UseLinkLocation("_Links");
    }
}
