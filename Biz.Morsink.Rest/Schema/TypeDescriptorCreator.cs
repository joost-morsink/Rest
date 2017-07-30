using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public static class TypeDescriptorCreator
    {
        private static ConcurrentDictionary<Type, TypeDescriptor> descriptors;

        static TypeDescriptorCreator()
        {
            var d = new ConcurrentDictionary<Type, TypeDescriptor>();

            d[typeof(string)] = TypeDescriptor.Primitive.String.Instance;
            d[typeof(long)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(int)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(short)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(sbyte)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(ulong)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(uint)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(ushort)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;
            d[typeof(byte)] = TypeDescriptor.Primitive.Numeric.Integral.Instance;

            d[typeof(decimal)] = TypeDescriptor.Primitive.Numeric.Float.Instance;
            d[typeof(float)] = TypeDescriptor.Primitive.Numeric.Float.Instance;
            d[typeof(double)] = TypeDescriptor.Primitive.Numeric.Float.Instance;

            d[typeof(DateTime)] = TypeDescriptor.Primitive.DateTime.Instance;

            d[typeof(object)] = new TypeDescriptor.Record(Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>());

            descriptors = d;
        }

        public static TypeDescriptor GetDescriptor(this Type type)
        {
            if (descriptors.TryGetValue(type, out var res)) 
                return res;

            return GetArrayDescriptor(type) 
                ?? GetRecordDescriptor(type)
                ;
        }
        private static TypeDescriptor GetArrayDescriptor(Type type)
        {
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                var q = from itf in type.GetTypeInfo().ImplementedInterfaces.Concat(new[] { type })
                        let iti = itf.GetTypeInfo()
                        let ga = iti.GetGenericArguments()
                        where ga.Length == 1 && iti.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        select ga[0];
                var inner = (q.FirstOrDefault() ?? typeof(object)).GetDescriptor();
                return descriptors.GetOrAdd(type, new TypeDescriptor.Array(inner));
            }
            else
                return null;
        }
        private static TypeDescriptor GetRecordDescriptor(Type type)
        {
            var props = from p in type.GetTypeInfo().DeclaredProperties
                        where p.CanRead && p.CanWrite
                        select new PropertyDescriptor<TypeDescriptor>(p.Name, p.PropertyType.GetDescriptor(), false);
            return props.Any()
                ? descriptors.GetOrAdd(type, new TypeDescriptor.Record(props))
                : null;
        }
    }
}
