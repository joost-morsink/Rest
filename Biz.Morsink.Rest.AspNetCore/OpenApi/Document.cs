using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Metadata;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// Class representing an OpenAPI Specification version 3.0 document.
    /// </summary>
    public class Document
    {
        private const string GET = nameof(GET);
        private const string PUT = nameof(PUT);
        private const string POST = nameof(POST);
        private const string PATCH = nameof(PATCH);
        private const string DELETE = nameof(DELETE);
        /// <summary>
        /// A nested class responsible for creating Document objects.
        /// </summary>
        public class Creator
        {
            private readonly RestApiDescription apiDescription;
            private readonly IEnumerable<IRestPathMapping> mappings;
            private readonly ILookup<string, IRestPathMapping> mapLookup;
            private readonly TypeDescriptorCreator typeDescriptorCreator;
            private readonly IRestIdentityProvider idProvider;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="apiDescription">A description of the api to generate a OAS 3 document for.</param>
            /// <param name="mappings">All rest path mappings.</param>
            /// <param name="typeDescriptorCreator">A TypeDescriptorCreator.</param>
            /// <param name="idProvider">A Rest Identity Provider.</param>
            public Creator(RestApiDescription apiDescription, IEnumerable<IRestPathMapping> mappings, TypeDescriptorCreator typeDescriptorCreator, IRestIdentityProvider idProvider)
            {
                this.apiDescription = apiDescription;
                this.mappings = mappings;
                mapLookup = mappings.ToLookup(m => m.RestPath, m => m);
                this.typeDescriptorCreator = typeDescriptorCreator;
                this.idProvider = idProvider;
            }
            private static string CamelCase(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return name;
                if (char.IsUpper(name[0]))
                    return char.ToLower(name[0]) + name.Substring(1);
                else
                    return name;
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
                    res.Description = capDesc.Method.GetRestDocumentation()
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
                            Name = CamelCase(p.Key),
                            Description = string.Join(Environment.NewLine, capDesc.ParameterType.GetProperty(p.Key)
                                ?.GetRestDocumentation()),
                            In = "query",
                            Required = p.Value.Required,
                            Schema = GetSchemaForTypeDescriptor(p.Value.Type)
                        })));
                    }
                    if (capDesc.Method != null)
                        res.Parameters.AddRange(capDesc.Method.GetRestParameterProperties().Select(p =>
                        new OrReference<Parameter>(new Parameter
                        {
                            Name = CamelCase(p.Name),
                            Description = p.GetRestDocumentation(),
                            In = "query",
                            Required = p.IsRequired(),
                            Schema = GetSchemaForType(p.PropertyType)
                        })));

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
                        res.Responses[capDesc.Method?.HasMetaDataOutAttribute<CreatedResource>() == true ? "201" : "200"] = new Response
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
                    else if (typeDescriptor is TypeDescriptor.Primitive.Boolean)
                        return new Schema { Type = "boolean" };
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
                    else if (typeDescriptor is TypeDescriptor.Primitive.Boolean)
                        return new Schema { Type = "boolean" };
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
            /// <summary>
            /// Creates the OpenAPI Specification version 3.0 document for the api description supplied on construction of the Creator.
            /// </summary>
            /// <returns>An OpenAPI Specification version 3.0 document.</returns>
            public Document Create()
            {
                var doc = new Document
                {
                    OpenApi = "3.0.0",
                    Paths = new SortedDictionary<string, Path>(mapLookup
                        .Select(grp => new
                        {
                            Path = grp.Key,
                            Mappings = grp.AsEnumerable(),
                            Descriptors = grp.SelectMany(m => apiDescription.EntityTypes[m.ResourceType].Select(capDesc => (m, capDesc)))
                                .OrderByDescending(capDesc => capDesc.m.Version.Major)
                        })
                        .Select(p => new
                        {
                            Url = MakeApiPath(p.Path),
                            p.Mappings,
                            Operations = p.Descriptors
                                .GroupBy(capDesc => capDesc.capDesc.Name)
                                .Select(grp => (
                                    grp.Key,
                                    Operation: grp.OrderByDescending(capDesc => capDesc.m.Version.Major)
                                        .Select(capDesc => GetOperationForCapability(capDesc.capDesc, capDesc.m))
                                        .First()
                                 ))
                                .ToDictionary(ops => ops.Key, ops => ops.Operation),
                            Docs = p.Descriptors.Select(d => d.capDesc.Method?.DeclaringType)
                                .Where(t => t != null)
                                .Take(1)
                                .SelectMany(t => t.GetCustomAttributes<RestDocumentationAttribute>())
                                .GetRestDocumentation()
                        })
                        .Select(d => new
                        {
                            d.Url,
                            Path = new Path
                            {
                                Summary = $"For restpath {d.Url}",
                                Description = d.Docs,
                                Get = GetOrNull(d.Operations, GET),
                                Put = GetOrNull(d.Operations, PUT),
                                Post = GetOrNull(d.Operations, POST),
                                Patch = GetOrNull(d.Operations, PATCH),
                                Delete = GetOrNull(d.Operations, DELETE)
                            }
                        })
                        .ToDictionary(p => p.Url, p => p.Path))
                };
                return doc;
            }

        }
        /// <summary>
        /// Creates an OpenAPI Specification version 3.0 document.
        /// </summary>
        /// <param name="apiDescription">The Rest API description to generate the document for.</param>
        /// <param name="mappings">All Rest Path mappings for the API.</param>
        /// <param name="typeDescriptorCreator">The TypeDescriptorCreator.</param>
        /// <param name="idProvider">The Rest Identity Provider.</param>
        /// <returns></returns>
        public static Document Create(RestApiDescription apiDescription, IEnumerable<IRestPathMapping> mappings, TypeDescriptorCreator typeDescriptorCreator, IRestIdentityProvider idProvider)
        {
            var c = new Creator(apiDescription, mappings, typeDescriptorCreator, idProvider);
            return c.Create();
        }
        /// <summary>
        /// A version string containing the version for OpenApi.
        /// Should be set to "3.0.0" 
        /// </summary>
        public string OpenApi { get; set; }
        /// <summary>
        /// An info object containing metadata for the API.
        /// </summary>
        public Info Info { get; set; }
        /// <summary>
        /// A List of servers.
        /// </summary>
        public List<Server> Servers { get; set; } = new List<Server>();
        /// <summary>
        /// Description of all the paths .
        /// </summary>
        public SortedDictionary<string, Path> Paths { get; set; } = new SortedDictionary<string, Path>();
        /// <summary>
        /// Reusable components for this document.
        /// </summary>
        public Components Components { get; set; }
    }
}
