using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter
{
    public class HasIdentityConverterDecorator : JsonConverter
    {
        private readonly JsonConverter inner;
        private readonly IRestRequestScopeAccessor restRequestScopeAccessor;

        public HasIdentityConverterDecorator(JsonConverter inner, IRestRequestScopeAccessor restRequestScopeAccessor)
        {
            this.inner = inner;
            this.restRequestScopeAccessor = restRequestScopeAccessor;
        }
        public override bool CanRead => inner?.CanRead ?? true;
        public override bool CanWrite => inner?.CanWrite ?? true;

        public override bool CanConvert(Type objectType) => !(inner?.CanConvert(objectType) == false);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => inner.ReadJson(reader, objectType, existingValue, serializer);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IHasIdentity hid)
            {
                restRequestScopeAccessor.Scope
                    .With<SerializationContext>(ctx => ctx.Without(hid.Id))
                    .Run(() => inner.WriteJson(writer, value, serializer));
            }
            else
                inner.WriteJson(writer, value, serializer);
        }
    }
}
