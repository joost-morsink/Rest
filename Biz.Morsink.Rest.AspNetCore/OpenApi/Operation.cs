using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    public class Operation
    {
        public string Summary { get; set; }
        public string Description { get; set; }
        public List<OrReference<Parameter>> Parameters { get; set; } = new List<OrReference<Parameter>>();
        public RequestBody RequestBody { get; set; }
        public Dictionary<string, Response> Responses { get; set; }
    }
}