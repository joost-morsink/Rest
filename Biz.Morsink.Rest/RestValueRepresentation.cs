//using Biz.Morsink.Rest.Schema;
//using Biz.Morsink.Rest.Serialization;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Biz.Morsink.Rest
//{
//    public class RestValueRepresentation<C> : ITypeRepresentation
//        where C : SerializationContext<C>
//    {
//        public RestValueRepresentation(Func<Serializer<C>> serializer, Func<C> contextCreator)
//        {
//            this.serializer = new Lazy<Serializer<C>>(serializer);
//            this.contextCreator = contextCreator;
//        }

//        private readonly Lazy<Serializer<C>> serializer;
//        private readonly Func<C> contextCreator;

//        public object GetRepresentable(object rep)
//        {
//            throw new NotSupportedException();
//        }

//        public Type GetRepresentableType(Type type)
//            => null;

//        public object GetRepresentation(object obj)
//        {
//            var rv = (IRestValue)obj;
//            var res = serializer.Value.Serialize(contextCreator().With(rv), rv.Value);
//            return res;
//        }

//        public Type GetRepresentationType(Type type)
//            => typeof(IRestValue).IsAssignableFrom(type) ? typeof(SObject) : null;

//        public bool IsRepresentable(Type type)
//            => GetRepresentationType(type) != null;

//        public bool IsRepresentation(Type type)
//            => false;
//    }
//}
