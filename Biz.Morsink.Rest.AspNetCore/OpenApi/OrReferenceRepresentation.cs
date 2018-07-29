using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    public class OrReferenceRepresentation : ITypeRepresentation
    {
        public object GetRepresentable(object rep)
        {
            var u = (UnionRepresentation)rep;
            var item = u.GetItem();
            if(item is Reference)
                return Activator.CreateInstance(typeof(OrReference<>.ReferenceImpl).MakeGenericType(u.GetTypes().ElementAt(1)), item);
            else
                return Activator.CreateInstance(typeof(OrReference<>.ItemImpl).MakeGenericType(u.GetTypes().ElementAt(1)),item);
        }
        public Type GetRepresentableType(Type type)
        {
            var (reference, t) = type.GetGenerics2(typeof(UnionRepresentation<,>));
            if (reference != null && reference == typeof(Reference))
                return typeof(OrReference<>).MakeGenericType(t);
            else
                return null;
        }

        public object GetRepresentation(object obj)
        {
            var gen = obj?.GetType().GetGeneric(typeof(OrReference<>));
            if (gen == null)
                return null;
            return UnionRepresentation.FromOptions(typeof(Reference), gen).Create(obj);
        }
       
        public Type GetRepresentationType(Type type)
        {
            var typeParam = type.GetGeneric(typeof(OrReference<>));
            if (typeParam == null)
                return null;
            return typeof(UnionRepresentation<,>).MakeGenericType(typeof(Reference), typeParam);
        }

        public bool IsRepresentable(Type type)
            => GetRepresentationType(type) != null;

        public bool IsRepresentation(Type type)
            => GetRepresentableType(type) != null;
    }
}
