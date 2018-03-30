using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    public class Response
    {
        public string Description { get; set; }
        public Dictionary<string, OrReference<Header>> Headers { get; set; } = new Dictionary<string, OrReference<Header>>();
        public Dictionary<string, Content> Content { get; set; } = new Dictionary<string, Content>();
    }
}
