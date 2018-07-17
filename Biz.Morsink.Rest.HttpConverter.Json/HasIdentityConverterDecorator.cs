using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter
{
    public class HasIdentityConverterDecorator : JsonConverter
    {
        private readonly JsonConverter inner;
        private readonly IRestRequestScopeAccessor restRequestScopeAccessor;
        private readonly ITypeRepresentation identityRepresentation;
        public HasIdentityConverterDecorator(JsonConverter inner, IRestRequestScopeAccessor restRequestScopeAccessor, IEnumerable<ITypeRepresentation> typeRepresentations)
        {
            this.inner = inner;
            this.restRequestScopeAccessor = restRequestScopeAccessor;
            identityRepresentation = typeRepresentations.First(repr => repr.IsRepresentable(typeof(IIdentity)));
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
                var scope = restRequestScopeAccessor.Scope;
                var ctx = scope.GetScopeItem<SerializationContext>();
                if (ctx.IsInParentChain(hid.Id))
                    serializer.Serialize(writer, hid.Id);
                else 
                    scope.With(ctx.Without(hid.Id).WithParent(hid.Id))
                        .Run(() => inner.WriteJson(writer, value, serializer));
            }
            else
                inner.WriteJson(writer, value, serializer);
        }
    }
}
