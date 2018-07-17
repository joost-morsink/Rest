using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.HttpConverter.Json;
using Biz.Morsink.Rest.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter
{
    public class IdentityJsonSchemaTranslator : IJsonSchemaTranslator
    {
        private readonly IJsonSchemaProvider schemaProvider;
        private readonly IRestRequestScopeAccessor scopeAccessor;
        private readonly IOptions<JsonHttpConverterOptions> jsonOptions;
        private readonly IdentityRepresentation representation;

        public IdentityJsonSchemaTranslator(IJsonSchemaProvider schemaProvider, IRestRequestScopeAccessor scopeAccessor, IRestIdentityProvider identityProvider, IRestPrefixContainerAccessor prefixContainerAccessor, IOptions<RestAspNetCoreOptions> options, IOptions<JsonHttpConverterOptions> jsonOptions, ICurrentHttpRestConverterAccessor currentHttpRestConverterAccessor)
        {
            this.schemaProvider = schemaProvider;
            this.scopeAccessor = scopeAccessor;
            this.jsonOptions = jsonOptions;
            representation = new IdentityRepresentation(identityProvider, prefixContainerAccessor, options, currentHttpRestConverterAccessor);
        }
        public JsonConverter GetConverter(Type type)
            => typeof(IIdentity).IsAssignableFrom(type) ? new Converter(this, type) : null;

        public JsonSchema GetSchema(Type type)
            => typeof(IIdentity).IsAssignableFrom(type) ? schemaProvider.GetSchema(representation.GetRepresentationType(typeof(IIdentity))) : null;

        private class Converter : JsonConverter
        {
            private readonly IdentityJsonSchemaTranslator parent;
            private readonly Type identityType;
            private readonly Type entityType;

            public Converter(IdentityJsonSchemaTranslator parent, Type identityType)
            {
                this.parent = parent;
                this.identityType = identityType;
                entityType = identityType.GetGeneric(typeof(IIdentity<>)) ?? typeof(object);
            }

            public override bool CanConvert(Type objectType)
                => identityType == objectType;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var rep = serializer.Deserialize(reader, parent.representation.GetRepresentationType(identityType));
                return rep == null ? null : parent.representation.GetRepresentable(rep);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var scope = parent.scopeAccessor.Scope;
                if (parent.jsonOptions.Value.EmbedEmbeddings
                    && scope.TryGetScopeItem<SerializationContext>(out var ctx)
                    && ctx.TryGetEmbedding((IIdentity)value, out var embedding))
                    scope.With(ctx.Without((IIdentity)value))
                        .Run(() => serializer.Serialize(writer, embedding));
                else
                    serializer.Serialize(writer, parent.representation.GetRepresentation(value));
            }
        }
    }
}
