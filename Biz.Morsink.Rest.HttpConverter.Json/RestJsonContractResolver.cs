﻿using Biz.Morsink.Identity;
using Biz.Morsink.Identity.PathProvider;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    using static Biz.Morsink.Rest.FSharp.Utils;
    /// <summary>
    /// A Json Contract Resolver for Rest. 
    /// It knows how to serialize some Rest specific types like IIdentity values.
    /// </summary>
    public class RestJsonContractResolver : DefaultContractResolver
    {
        private readonly IJsonSchemaTranslator[] translators;
        private readonly ITypeRepresentation[] typeRepresentations;
        private readonly TypeDescriptorCreator typeDescriptorCreator;
        private readonly IRestRequestScopeAccessor restRequestScopeAccessor;
        private readonly IOptions<JsonHttpConverterOptions> options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider to instantiate dependencies.</param>
        public RestJsonContractResolver(IEnumerable<IJsonSchemaTranslator> translators, IEnumerable<ITypeRepresentation> typeRepresentations, TypeDescriptorCreator typeDescriptorCreator, IRestRequestScopeAccessor restRequestScopeAccessor, IOptions<JsonHttpConverterOptions> options)
        {
            this.translators = translators.ToArray();
            this.typeRepresentations = typeRepresentations.ToArray();
            this.typeDescriptorCreator = typeDescriptorCreator;
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
                    ?? new DefaultJsonConverterForObjectContract(objContract, objectType, typeDescriptorCreator.GetDescriptor(objectType), this.CreateProperties(objectType, MemberSerialization.OptOut));
                if (typeof(IHasIdentity).IsAssignableFrom(objectType))
                    contract.Converter = new HasIdentityConverterDecorator(contract.Converter, restRequestScopeAccessor, typeRepresentations);
            }
            return contract;
        }
        /// <summary>
        /// An implementation of the JsonConverter class that should implement the basic property-based serialization of regular objects.
        /// </summary>
        private class DefaultJsonConverterForObjectContract : JsonConverter
        {
            private readonly IList<JsonProperty> properties;
            private readonly Dictionary<string, JsonProperty> indexedProperties;
            private readonly JsonObjectContract contract;
            private readonly Type type;
            private readonly TypeDescriptor typeDescriptor;

            public DefaultJsonConverterForObjectContract(JsonObjectContract contract, Type type, TypeDescriptor typeDescriptor, IList<JsonProperty> properties)
            {
                this.contract = contract;
                this.type = type;
                this.typeDescriptor = typeDescriptor;
                this.properties = properties;
                indexedProperties = properties.ToDictionary(p => p.PropertyName, CaseInsensitiveEqualityComparer.Instance);
            }

            public override bool CanConvert(Type objectType)
                => type == objectType;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                object res;

                if (reader.TokenType == JsonToken.Null)
                    return null;
                if (reader.TokenType != JsonToken.StartObject)
                    throw new JsonSerializationException();
                reader.Read();

                res = DefaultRead(reader, serializer) ?? ImmutableRead(reader, serializer);

                if (reader.TokenType != JsonToken.EndObject)
                    throw new JsonSerializationException();

                return res;
            }

            private object ImmutableRead(JsonReader reader, JsonSerializer serializer)
            {
                object res;
                if (contract.OverrideCreator == null)
                    contract.OverrideCreator = parameters => Activator.CreateInstance(contract.UnderlyingType, parameters);
                var values = new Dictionary<string, object>(CaseInsensitiveEqualityComparer.Instance);
                while (reader.TokenType == JsonToken.PropertyName)
                {
                    if (indexedProperties.TryGetValue(reader.Value.ToString(), out var prop))
                    {
                        reader.Read();
                        var val = serializer.Deserialize(reader, prop.PropertyType);
                        values[prop.PropertyName] = val;
                        reader.Read();
                    }
                    else
                    {
                        reader.Read();
                        reader.Skip();
                        reader.Read();
                    }
                }
                res = contract.OverrideCreator(contract.CreatorParameters.Select(cp => GetOrDefault(values, cp.PropertyName, cp.DefaultValue)).ToArray());
                foreach (var cp in contract.CreatorParameters)
                {
                    values.Remove(cp.PropertyName);
                }
                foreach (var kvp in values)
                    indexedProperties[kvp.Key].ValueProvider.SetValue(res, kvp.Value);
                return res;

                V GetOrDefault<K, V>(Dictionary<K, V> dictionary, K key, V defaultValue)
                    => dictionary.TryGetValue(key, out var x) ? x : defaultValue;
            }

            private object DefaultRead(JsonReader reader, JsonSerializer serializer)
            {
                if (contract.DefaultCreator == null)
                    return null;
                object res = contract.DefaultCreator();
                while (reader.TokenType == JsonToken.PropertyName)
                {
                    if (indexedProperties.TryGetValue(reader.Value.ToString(), out var prop))
                    {
                        reader.Read();
                        var val = serializer.Deserialize(reader, prop.PropertyType);
                        prop.ValueProvider.SetValue(res, val);
                        reader.Read();
                    }
                    else
                    {
                        reader.Read();
                        reader.Skip();
                        reader.Read();
                    }
                }

                return res;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                foreach (var prop in properties)
                {
                    var val = prop.ValueProvider.GetValue(value);
                    if (val != null)
                    {
                        writer.WritePropertyName(prop.PropertyName);
                        serializer.Serialize(writer, val);
                    }
                }
                writer.WriteEndObject();
            }
        }
    }
}
