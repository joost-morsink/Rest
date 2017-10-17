using Biz.Morsink.Identity;
using Biz.Morsink.Identity.PathProvider;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A class that helps construct TypeDescriptor objects for CLR types.
    /// </summary>
    public class TypeDescriptorCreator
    {
        private ConcurrentDictionary<Type, TypeDescriptor> descriptors;
        private ConcurrentDictionary<string, TypeDescriptor> byString;
        private IEnumerable<ITypeRepresentation> representations;
        public ICollection<Type> RegisteredTypes => descriptors.Keys;
        public TypeDescriptorCreator(IEnumerable<ITypeRepresentation> representations = null)
        {
            this.representations = representations ?? Enumerable.Empty<ITypeRepresentation>();
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

            d[typeof(bool)] = TypeDescriptor.Primitive.Boolean.Instance;

            d[typeof(DateTime)] = TypeDescriptor.Primitive.DateTime.Instance;

            d[typeof(object)] = new TypeDescriptor.Record(typeof(object).ToString(), Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>());

            descriptors = d;
            byString = new ConcurrentDictionary<string, TypeDescriptor>(descriptors.Select(e => new KeyValuePair<string, TypeDescriptor>(e.Key.ToString(), e.Value)));
        }

        /// <summary>
        /// Gets a TypeDescriptor for this type.
        /// </summary>
        /// <param name="type">The type to get a TypeDescriptor for.</param>
        /// <returns>A TypeDescriptor for the type.</returns>
        public TypeDescriptor GetDescriptor(Type type)
            => type == null ? null : GetDescriptor(type, null, null);
        private TypeDescriptor GetDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            enclosing = enclosing ?? ImmutableStack<Type>.Empty;
            if (enclosing.Contains(type))
                return new TypeDescriptor.Reference(type.ToString());
            return descriptors.GetOrAdd(type, ty =>
            {
                ty = representations.Where(rep => rep.IsRepresentable(ty)).Select(rep => rep.GetRepresentationType(ty)).FirstOrDefault() ?? ty;
                var desc = GetNullableDescriptor(ty, cutoff, enclosing.Push(type)) // Check for nullability
                ?? GetArrayDescriptor(ty, cutoff, enclosing.Push(type)) // Check for collections
                ?? GetUnionDescriptor(ty, cutoff, enclosing.Push(type)) // Check for disjunct union types
                ?? GetRecordDescriptor(ty, cutoff, enclosing.Push(type)) // Check for records (regular objects)
                ?? GetUnitDescriptor(ty, cutoff, enclosing.Push(type)); // Check form empty types
                byString.AddOrUpdate(GetTypeName(type), desc, (_, __) => desc);
                return desc;
            });
        }
        /// <summary>
        /// Gets a TypeDescriptor with a specified name.
        /// </summary>
        /// <param name="name">The name of the TypeDescriptor.</param>
        /// <returns></returns>
        public TypeDescriptor GetDescriptorByName(string name)
            => byString.TryGetValue(name, out var res) ? res : null;
        /// <summary>
        /// Gets the 'name' for a Type.
        /// The name is used as a key to lookup TypeDescriptors.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The name for a type.</returns>
        public string GetTypeName(Type type)
            => type.ToString();

        private TypeDescriptor GetNullableDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            var ti = type.GetTypeInfo();
            var ga = ti.GetGenericArguments();
            if (ga.Length == 1)
            {
                if (ti.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var t = GetDescriptor(ga[0], cutoff, enclosing);
                    return t == null ? null : new TypeDescriptor.Union(t.ToString() + "?", new TypeDescriptor[] { t, TypeDescriptor.Null.Instance });
                }
            }
            return null;
        }
        private TypeDescriptor GetArrayDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                var q = from itf in type.GetTypeInfo().ImplementedInterfaces.Concat(new[] { type })
                        let iti = itf.GetTypeInfo()
                        let ga = iti.GetGenericArguments()
                        where ga.Length == 1 && iti.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        select ga[0];
                var inner = GetDescriptor((q.FirstOrDefault() ?? typeof(object)), null, enclosing);
                return new TypeDescriptor.Array(inner);
            }
            else
                return null;
        }
        private TypeDescriptor GetRecordDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            var ti = type.GetTypeInfo();
            if (ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.GetParameters().Length == 0).Any())
            {
                var props = from p in type.GetTypeInfo().DeclaredProperties
                            where p.CanRead && p.CanWrite
                            let req = p.GetCustomAttributes<RequiredAttribute>().Any()
                            select new PropertyDescriptor<TypeDescriptor>(p.Name, GetDescriptor(p.PropertyType, null, enclosing), req);
                return props.Any()
                    ? new TypeDescriptor.Record(type.ToString(), props)
                    : null;
            }
            else
            {
                var props = Iterate(ti, x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != cutoff && x != null).SelectMany(x => x.DeclaredProperties).ToArray();
                if (!props.All(pi => pi.CanRead && !pi.CanWrite))
                    return null;
                var properties = from ci in ti.DeclaredConstructors
                                 let ps = ci.GetParameters()
                                 where !ci.IsStatic && ps.Length > 0 && ps.Length >= props.Count()
                                     && ps.Join(props, p => p.Name, p => p.Name, (_, __) => 1, CaseInsensitiveEqualityComparer.Instance).Count() == props.Length
                                 from p in ps.Join(props, p => p.Name, p => p.Name,
                                     (par, prop) => new PropertyDescriptor<TypeDescriptor>(prop.Name, GetDescriptor(prop.PropertyType, null, enclosing), !par.GetCustomAttributes<OptionalAttribute>().Any()),
                                     CaseInsensitiveEqualityComparer.Instance)
                                 select p;

                return properties.Any() ? new TypeDescriptor.Record(type.ToString(), properties) : null;
            }

        }
        private TypeDescriptor GetUnitDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            var ti = type.GetTypeInfo();
            var parameterlessConstructors = ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.GetParameters().Length == 0);
            return parameterlessConstructors.Any()
                && !Iterate(ti, x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != cutoff && x != null).SelectMany(x => x.DeclaredProperties.Where(p => !p.GetAccessors()[0].IsStatic)).Any()
                ? new TypeDescriptor.Record(type.ToString(), Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>())
                : null;
        }
        private TypeDescriptor GetUnionDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            var ti = type.GetTypeInfo();
            if (ti.IsAbstract && ti.DeclaredNestedTypes.Any(nt => nt.BaseType == type))
            {
                var rec = GetRecordDescriptor(type, cutoff, enclosing);
                TypeDescriptor res = new TypeDescriptor.Union(rec == null ? type.ToString() : "", ti.DeclaredNestedTypes.Where(nt => nt.BaseType == type).Select(ty => GetDescriptor(ty, type, enclosing)));

                if (rec != null)
                    res = new TypeDescriptor.Intersection(type.ToString(), new[] { rec, res });

                return res;
            }
            else
                return null;
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
