using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.HttpConverter.Json;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Class to convert RestValues to Json.
    /// </summary>
    public class RestValueConverter : JsonConverter, IJsonSchemaTranslator
    {
        private readonly IJsonSchemaProvider schemaProvider;
        private readonly ITypeDescriptorCreator typeDescriptorCreator;
        private readonly IRestRequestScopeAccessor restRequestScopeAccessor;
        private readonly IRestIdentityProvider identityProvider;
        private readonly IOptions<JsonHttpConverterOptions> options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDescriptorCreator">A Type descriptor creator.</param>
        /// <param name="schemaProvider">A Json schema provider.</param>
        /// <param name="options">Options for the Json Http converter component.</param>
        public RestValueConverter(ITypeDescriptorCreator typeDescriptorCreator, IJsonSchemaProvider schemaProvider, IRestRequestScopeAccessor restRequestScopeAccessor, IRestIdentityProvider identityProvider, IOptions<JsonHttpConverterOptions> options)
        {
            this.schemaProvider = schemaProvider;
            this.typeDescriptorCreator = typeDescriptorCreator;
            this.restRequestScopeAccessor = restRequestScopeAccessor;
            this.identityProvider = identityProvider;
            this.options = options;
        }

        /// <summary>
        /// Conversion is supported for types that implement IRestValue.
        /// </summary>
        /// <param name="objectType">The type to check.</param>
        /// <returns>True if the type implements IRestValue, false otherwise.</returns>
        public override bool CanConvert(Type objectType)
            => typeof(IRestValue).IsAssignableFrom(objectType);

        /// <summary>
        /// Gets a JsonConverter for the Rest value type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>This if the type implements IRestValue, null otherwise.</returns>
        public JsonConverter GetConverter(Type type)
            => typeof(IRestValue).IsAssignableFrom(type) ? this : null;

        /// <summary>
        /// Gets a JsonSchema for the Rest value type.
        /// </summary>
        /// <returns>A schema if the type implements IRestValue, null otherwise.</returns>
        public JsonSchema GetSchema(Type type)
        {
            if (!typeof(IRestValue).IsAssignableFrom(type))
                return null;
            var valueType = type.GetGeneric(typeof(RestValue<>)) ?? typeof(object);
            return schemaProvider.GetSchema(valueType);
        }

        public override bool CanRead => false;
        /// <summary>
        /// Reading is not permitted.
        /// </summary>
        /// <returns>Throws a NotImplementedException.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Writes a Rest value to a Json stream.
        /// </summary>
        /// <param name="writer">The Json writer.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="serializer">The serializer to use for serialization.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            var type = value.GetType();
            if (!typeof(IRestValue).IsAssignableFrom(type))
                throw new ArgumentException("Type should be assignable to IRestValue", nameof(type));
            var valueType = type.GetGeneric(typeof(RestValue<>)) ?? typeof(object);

            var scope = restRequestScopeAccessor.Scope;
            var rv = (IRestValue)value;
            scope.With(ctx => ctx.With(rv), SerializationContext.Create(identityProvider))
                .Run(() => innerWrite());

            void innerWrite()
            {
                if (rv.Value is IHasIdentity hid)
                    scope.ModifyScopeItem<SerializationContext>(c => c.Without(hid.Id));

                var opts = options.Value;
                if (opts.LinkLocation != null)
                {
                    var o = JObject.FromObject(rv.Value, serializer);
                    o.Add(new JProperty(opts.LinkLocation, new JArray(rv.Links.Select(l => JObject.FromObject(l, serializer)))));
                    serializer.Serialize(writer, o);
                }
                else
                    serializer.Serialize(writer, rv.Value, rv.ValueType);
            }
        }
    }
}
