using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Caching
{
    public class CacheSerializer : Serializer<Serialization.SerializationContext>
    {
        private readonly IRestIdentityProvider identityProvider;

        public CacheSerializer(ITypeDescriptorCreator typeDescriptorCreator, IRestIdentityProvider identityProvider,IDataConverter converter = null) : base(typeDescriptorCreator, converter)
        {
            this.identityProvider = identityProvider;
        }
        protected override IForType CreateSerializer(Type ty)
        {
            var ser = base.CreateSerializer(ty);
            if (typeof(IHasIdentity).IsAssignableFrom(ty))
                return CreateHasIdentitySerializer(ser, ty);
            else
                return ser;
        }

        private IForType CreateHasIdentitySerializer(IForType ser, Type ty)
            => (IForType)Activator.CreateInstance(typeof(HasIdentityType<>).MakeGenericType(ty), this, ser);

        private class HasIdentityType<T> : Typed<T>
            where T : IHasIdentity
        {
            private readonly Typed<T> inner;
            public new CacheSerializer Parent => (CacheSerializer)base.Parent;
            public HasIdentityType(CacheSerializer parent, Typed<T> inner) : base(parent)
            {
                this.inner = inner;
            }
            public override T Deserialize(Serialization.SerializationContext context, SItem item)
            {
                return inner.Deserialize(context, item);
            }
            public override SItem Serialize(Serialization.SerializationContext context, T item)
            {
                if (context.IsInParentChain(item.Id))
                    return Parent.Serialize(context.WithParent(item.Id), item.Id);
                else
                    return inner.Serialize(context.WithParent(item.Id), item);
            }
        }

    }
}
