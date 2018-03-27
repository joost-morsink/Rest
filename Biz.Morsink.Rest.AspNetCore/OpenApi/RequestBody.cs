using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    public class RequestBody
    {
        public string Description { get; set; }
        public Dictionary<string, Content> Content { get; set; }
        public bool Required { get; set; }
    }
}