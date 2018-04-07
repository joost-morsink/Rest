using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public class Creator
        {
            private readonly RestApiDescription apiDescription;
            private readonly IEnumerable<IRestPathMapping> mappings;
            private readonly Dictionary<Type, IRestPathMapping> mapDict;
            private readonly TypeDescriptorCreator typeDescriptorCreator;
            private readonly IRestIdentityProvider idProvider;

            public Creator(RestApiDescription apiDescription, IEnumerable<IRestPathMapping> mappings, TypeDescriptorCreator typeDescriptorCreator, IRestIdentityProvider idProvider)
            {
                this.apiDescription = apiDescription;
                this.mappings = mappings;
                mapDict = mappings.ToDictionary(m => m.ResourceType, m => m);
                this.typeDescriptorCreator = typeDescriptorCreator;
                this.idProvider = idProvider;
            }
            private static Operation GetOrNull(Dictionary<string, Operation> dict, string key)
                => dict.TryGetValue(key, out var res) ? res : null;

            private Operation GetOperationForCapability(RestCapabilityDescriptor capDesc, IRestPathMapping mapping)
            {
                var res = new Operation();
                var restPath = RestPath.Parse(mapping.RestPath);
                var typeDescriptor = typeDescriptorCreator.GetDescriptor(capDesc.ParameterType);
                if (capDesc.Method != null)
                {
                    var docAttrs = capDesc.Method.GetCustomAttributes<RestDocumentationAttribute>();
                    res.Description = docAttrs.Where(a => a.Format == "text/plain" || a.Format == "text/markdown")
                        .Select(a => a.Documentation).FirstOrDefault()
                        ?? $"Mapped to method {capDesc.Method.Name} on {capDesc.Method.DeclaringType.Name}";
                }

                processParameters();
                processBody();
                processResult();

                return res;

                // Local functions
                void processParameters()
                {
                    if (typeDescriptor is TypeDescriptor.Record r)
                    {
                        res.Parameters.AddRange(r.Properties.Select(p => new OrReference<Parameter>(new Parameter
                        {
                            Name = p.Key,
                            Description = p.Key,
                            In = "query",
                            Required = false,
                            Schema = GetSchemaForTypeDescriptor(p.Value.Type)
                        })));
                    }

                    res.Parameters.AddRange(mapping.ComponentTypes
                        .Zip(restPath.GetSegments().Where(s => s.IsComponent), (c, s) => new { s.IsWildcard, Component = c })
                        .Where(x => x.IsWildcard)
                        .Select(x => x.Component)
                        .Select((p, i) => new OrReference<Parameter>(new Parameter
                        {
                            Name = $"id{i}",
                            Description = $"Id for {p.Name}",
                            In = "path",
                            Required = true,
                            Schema = new Schema { Type = "string" }
                        })));
                }
                void processBody()
                {
                    if (capDesc.BodyType != null && capDesc.BodyType != typeof(Empty))
                    {
                        res.RequestBody = new RequestBody
                        {
                            Required = true,
                            Content = {
                                ["application/json"] = new Content {
                                    Schema = GetSchemaForType(capDesc.BodyType)
                                }
                            }
                        };
                    }
                }
                void processResult()
                {
                    if (capDesc.ResultType != null && capDesc.ResultType != typeof(Empty))
                    {
                        res.Responses["200"] = new Response
                        {
                            Content = {
                                ["application/json"] = new Content {
                                     Schema = GetSchemaForType(capDesc.ResultType)
                                }
                            }
                        };
                    }
                }
            }

            private OrReference<Schema> GetSchemaForType(Type type)
            {
                var typeDescriptor = typeDescriptorCreator.GetDescriptor(type);

                if (typeDescriptor == null)
                    return new Schema { Type = "object" };
                else if (typeDescriptor is TypeDescriptor.Primitive)
                {
                    if (typeDescriptor is TypeDescriptor.Primitive.Numeric)
                        return new Schema { Type = "number" };
                    else
                        return new Schema { Type = "string" };
                }
                else
                    return new Reference { Ref = GetTypeDescriptorPath(typeDescriptorCreator.GetTypeName(type)) };
            }
            private OrReference<Schema> GetSchemaForTypeDescriptor(TypeDescriptor typeDescriptor)
            {
                if (typeDescriptor == null)
                    return new Schema { Type = "object" };
                else if (typeDescriptor is TypeDescriptor.Primitive)
                {
                    if (typeDescriptor is TypeDescriptor.Primitive.Numeric)
                        return new Schema { Type = "number" };
                    else
                        return new Schema { Type = "string" };
                }
                else
                    return new Reference { Ref = GetTypeDescriptorPath(typeDescriptor.Name) };
            }

            private string GetTypeDescriptorPath(string type)
            {
                var id = idProvider.Creator<TypeDescriptor>().Create(type);
                return idProvider.ToPath(id);
            }

            private static string MakeApiPath(string restPath)
            {
                var rp = RestPath.Parse(restPath);
                if (rp.Count == 0)
                    return "/";

                var sb = new StringBuilder();
                int n = 0;
                for (int i = 0; i < rp.Count; i++)
                {
                    sb.Append('/');
                    var seg = rp[i];
                    if (seg.IsWildcard)
                        sb.Append($"{{id{n++}}}");
                    else
                        sb.Append(seg.Content);
                }
                return sb.ToString();
            }

            public Document Create()
            {

                var doc = new Document
                {
                    OpenApi = "3.0.0",
                    Paths = apiDescription.EntityTypes
                        .Where(et => mapDict.ContainsKey(et.Key))
                        .Select(et => new
                        {
                            Mapping = mapDict[et.Key],
                            Url = MakeApiPath(mapDict[et.Key].RestPath),
                            Dict = et.ToDictionary(e => e.Name, e => GetOperationForCapability(e, mapDict[et.Key]))
                        })
                        .Select(d => new
                        {
                            d.Url,
                            Path = new Path
                            {
                                Summary = $"For restpath {d.Mapping.RestPath}",
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

        }

        public static Document Create(RestApiDescription apiDescription, IEnumerable<IRestPathMapping> mappings, TypeDescriptorCreator typeDescriptorCreator, IRestIdentityProvider idProvider)
        {
            var c = new Creator(apiDescription, mappings, typeDescriptorCreator, idProvider);
            return c.Create();
        }

        public string OpenApi { get; set; }
        public Info Info { get; set; }
        public List<Server> Servers { get; set; } = new List<Server>();
        public Dictionary<string, Path> Paths { get; set; } = new Dictionary<string, Path>();
        public Components Components { get; set; }
    }
}
