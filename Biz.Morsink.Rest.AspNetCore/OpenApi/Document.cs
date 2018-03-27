using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    public class Document
    {
        private const string GET = nameof(GET);
        private const string PUT = nameof(PUT);
        private const string POST = nameof(POST);
        private const string PATCH = nameof(PATCH);
        private const string DELETE = nameof(DELETE);

        private static Operation GetOrNull(Dictionary<string, Operation> dict, string key)
            => dict.TryGetValue(key, out var res) ? res : null;
        private static Operation GetOperationForCapability(RestCapabilityDescriptor capDesc)
        {
            var res = new Operation();
            return res;
            
        }
        public static Document Create(RestApiDescription apiDescription, IEnumerable<IRestPathMapping> mappings)
        {
            var mapDict = mappings.ToDictionary(m => m.ResourceType, m => m.RestPath);
            var doc = new Document
            {
                OpenApi = new Version("3.0.0"),
                Paths = apiDescription.EntityTypes
                    .Where(et => mapDict.ContainsKey(et.Key))
                    .Select(et => new
                    {
                        Url = mapDict[et.Key],
                        Dict = et.ToDictionary(e => e.Name, e => GetOperationForCapability(e))
                    })
                    .Select(d => new
                    {
                        d.Url,
                        Path = new Path
                        {
                            Get = GetOrNull(d.Dict, GET),
                            Put = GetOrNull(d.Dict, PUT),
                            Post = GetOrNull(d.Dict, POST),
                            Patch = GetOrNull(d.Dict, PATCH),
                            Delete = GetOrNull(d.Dict, DELETE)
                        }
                    })
                    .ToDictionary(p => p.Url, p => p.Path)
            };
            return doc;
        }
        public Version OpenApi { get; set; }
        public Info Info { get; set; }
        public List<Server> Servers { get; set; } = new List<Server>();
        public Dictionary<string, Path> Paths { get; set; } = new Dictionary<string, Path>();
        public Components Components { get; set; }
    }
}
