﻿using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    using Biz.Morsink.Identity.PathProvider;
    using Biz.Morsink.Rest.Utils;
    using Newtonsoft.Json;
    using System.Reflection;
    using static Biz.Morsink.Rest.FSharp.Utils;
    /// <summary>
    /// A Json Contract Resolver for Rest. 
    /// It knows how to serialize some Rest specific types like IIdentity values.
    /// </summary>
    public class RestJsonContractResolver : DefaultContractResolver
    {
        private readonly IJsonSchemaTranslator[] translators;
        private readonly ITypeRepresentation[] typeRepresentations;
        private readonly IRestRequestScopeAccessor restRequestScopeAccessor;
        private readonly IOptions<JsonHttpConverterOptions> options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider to instantiate dependencies.</param>
        public RestJsonContractResolver(IEnumerable<IJsonSchemaTranslator> translators, IEnumerable<ITypeRepresentation> typeRepresentations, IRestRequestScopeAccessor restRequestScopeAccessor, IOptions<JsonHttpConverterOptions> options)
        {
            this.translators = translators.ToArray();
            this.typeRepresentations = typeRepresentations.ToArray();
            this.restRequestScopeAccessor = restRequestScopeAccessor;
            this.options = options;
        }
        private void Initialize()
        {
            var opts = options.Value;
            var ns = opts.NamingStrategy;
            if (ns != null)
                NamingStrategy = ns;
        }
        protected override JsonContract CreateContract(Type objectType)
        {
            Initialize();
            var contract = base.CreateContract(objectType);
            foreach (var typeRep in typeRepresentations.Where(tr => tr.IsRepresentable(objectType)))
                contract.Converter = new TypeRepresentationConverter(objectType, typeRep);

            foreach (var converter in translators.Select(t => t.GetConverter(objectType)).Where(c => c?.CanConvert(objectType) == true).Take(1))
                contract.Converter = converter;

            if (SemanticStructKind.Instance.IsOfKind(objectType))
                contract.Converter = SemanticStructConverter.Create(objectType);

            if (options.Value.FSharpSupport)
            {

                if (FSharp.FSharpUnionConverter.IsFSharpUnionType(objectType))
                    contract.Converter = new FSharp.FSharpUnionConverter(GetFsharpUnionType(objectType));
                if (FSharp.FSharpOptionConverter.IsFSharpOptionType(objectType))
                    contract.Converter = new FSharp.FSharpOptionConverter(objectType);
            }
            if (contract is JsonObjectContract objContract)
            {
                contract.Converter = contract.Converter
                    ?? new DefaultJsonConverter(objContract, objectType, this.CreateProperties(objectType, MemberSerialization.OptOut));
                if (typeof(IHasIdentity).IsAssignableFrom(objectType))
                    contract.Converter = new HasIdentityConverterDecorator(contract.Converter, restRequestScopeAccessor);
            }
            return contract;
        }
        private class DefaultJsonConverter : JsonConverter
        {
            private readonly IList<JsonProperty> properties;
            private readonly Dictionary<string, JsonProperty> indexedProperties;
            private readonly JsonObjectContract contract;
            private readonly Type type;

            public DefaultJsonConverter(JsonObjectContract contract, Type type, IList<JsonProperty> properties)
            {
                this.contract = contract;
                this.type = type;
                this.properties = properties;
                indexedProperties = properties.ToDictionary(p => p.PropertyName, CaseInsensitiveEqualityComparer.Instance);
            }

            public override bool CanConvert(Type objectType)
                => type == objectType;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;
                if (reader.TokenType != JsonToken.StartObject)
                    throw new JsonSerializationException();
                reader.Read();
                var res = contract.DefaultCreator();
                while (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = indexedProperties[reader.Value.ToString()];
                    reader.Read();
                    var val = serializer.Deserialize(reader, prop.PropertyType);
                    prop.ValueProvider.SetValue(res, val);
                    reader.Read();
                }
                if (reader.TokenType != JsonToken.EndObject)
                    throw new JsonSerializationException();

                return res;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                foreach (var prop in properties)
                {
                    //var pi = type.GetTypeInfo()
                    //    .Iterate(ti => ti.BaseType.GetTypeInfo())
                    //    .TakeWhile(ti => ti!=typeof(object))
                    //    .SelectMany(ti => ti.DeclaredProperties)
                    //    .Where(p => CaseInsensitiveEqualityComparer.Instance.Equals(p.Name, prop.PropertyName) && !p.GetMethod.IsStatic && p.GetMethod.IsPublic)
                    //    .First();
                    //.GetProperty(prop.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                    var val = prop.ValueProvider.GetValue(value);
                    if (val != null)
                    {
                        writer.WritePropertyName(prop.PropertyName);
                        serializer.Serialize(writer, val);
                        //prop.Converter.WriteJson(writer, val, serializer);
                    }
                }
                writer.WriteEndObject();
            }
        }
    }
}
