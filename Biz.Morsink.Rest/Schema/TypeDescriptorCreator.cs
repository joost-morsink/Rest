using Biz.Morsink.Identity.PathProvider;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A static class that helps construct TypeDescriptor objects for CLR types.
    /// </summary>
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

        /// <summary>
        /// Gets a TypeDescriptor for this type.
        /// </summary>
        /// <param name="type">The type to get a TypeDescriptor for.</param>
        /// <returns>A TypeDescriptor for the type.</returns>
        public static TypeDescriptor GetDescriptor(this Type type)
        {
            if (descriptors.TryGetValue(type, out var res))
                return res;


            return GetNullableDescriptor(type) // Check for nullability
                ?? GetArrayDescriptor(type) // Check for collections
                ?? GetRecordDescriptor(type) // Check for records (regular objects)
                ?? GetUnitDescriptor(type) // Check form empty types
                ;
        }
        private static TypeDescriptor GetNullableDescriptor (Type type)
        {
            var ti = type.GetTypeInfo();
            var ga = ti.GetGenericArguments();
            if (ga.Length == 1)
            {
                if(ti.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var t = ga[0].GetDescriptor();
                    return t == null ? null : new TypeDescriptor.Union(new TypeDescriptor[] { t, TypeDescriptor.Null.Instance });
                }
            }
            return null;
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
            var ti = type.GetTypeInfo();
            if (ti.DeclaredConstructors.Where(ci => ci.GetParameters().Length == 0).Any())
            {
                var props = from p in type.GetTypeInfo().DeclaredProperties
                            where p.CanRead && p.CanWrite
                            let req = p.GetCustomAttributes<RequiredAttribute>().Any()
                            select new PropertyDescriptor<TypeDescriptor>(p.Name, p.PropertyType.GetDescriptor(), req);
                return props.Any()
                    ? descriptors.GetOrAdd(type, new TypeDescriptor.Record(props))
                    : null;
            }
            else
            {
                var props = Iterate(ti, x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != null).SelectMany(x => x.DeclaredProperties).ToArray();
                if (!props.All(pi => pi.CanRead && !pi.CanWrite))
                    return null;
                var properties = from ci in ti.DeclaredConstructors
                                 let ps = ci.GetParameters()
                                 where ps.Length > 0 && ps.Length == props.Count()
                                     && ps.Join(props, p => p.Name, p => p.Name, (_, __) => 1, CaseInsensitiveEqualityComparer.Instance).Count() == ps.Length
                                 from p in ps.Join(props, p => p.Name, p => p.Name,
                                     (par, prop) => new PropertyDescriptor<TypeDescriptor>(prop.Name, prop.PropertyType.GetDescriptor(), !par.GetCustomAttributes<OptionalAttribute>().Any()),
                                     CaseInsensitiveEqualityComparer.Instance)
                                 select p;

                return properties.Any() ? descriptors.GetOrAdd(type, new TypeDescriptor.Record(properties)) : null;
            }
            
        }
        private static TypeDescriptor GetUnitDescriptor(Type type)
        {
            var ti = type.GetTypeInfo();
            return ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.GetParameters().Length == 0).Any()
                && !Iterate(ti, x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != null).SelectMany(x => x.DeclaredProperties.Where(p => !p.GetAccessors()[0].IsStatic)).Any()
                ? new TypeDescriptor.Record(Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>())
                : null;
        }
        private static IEnumerable<T> Iterate<T>(T seed, Func<T, T> next)
        {
            while (true)
            {
                yield return seed;
                seed = next(seed);
            }

        }
    }
}
