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
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A class that helps construct TypeDescriptor objects for CLR types.
    /// </summary>
    public class TypeDescriptorCreator
    {
        private const string CompilationMappingAttribute = nameof(CompilationMappingAttribute);
        private const string SourceConstructFlags = nameof(SourceConstructFlags);
        private const string SumType = nameof(SumType);
        private const string UnionCase = nameof(UnionCase);
        private const string Tags = nameof(Tags);
        private const string SequenceNumber = nameof(SequenceNumber);

        private static readonly string Microsoft_FSharp_Core = nameof(Microsoft_FSharp_Core).Replace('_', '.');
        private static readonly string FSharpOption_1 = nameof(FSharpOption_1).Replace('_', '`');

        private ConcurrentDictionary<Type, TypeDescriptor> descriptors;
        private ConcurrentDictionary<string, TypeDescriptor> byString;
        private IEnumerable<ITypeRepresentation> representations;
        /// <summary>
        /// Gets a collection of all the registered types.
        /// </summary>
        public ICollection<Type> RegisteredTypes => descriptors.Keys;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="representations">A collection of type representations.</param>
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
        private static bool IsPrimitiveTypeDescriptor(TypeDescriptor desc)
        {
            if (desc is TypeDescriptor.Primitive || desc is TypeDescriptor.Null || desc is TypeDescriptor.Referable
                || desc is TypeDescriptor.Reference || desc is TypeDescriptor.Value)
                return true;
            else if (desc is TypeDescriptor.Union u)
                return u.Options.All(IsPrimitiveTypeDescriptor);
            else if (desc is TypeDescriptor.Intersection i)
                return i.Parts.All(IsPrimitiveTypeDescriptor);
            else if (desc is TypeDescriptor.Array a)
                return IsPrimitiveTypeDescriptor(a.ElementType);
            else
                return false;
        }
        private TypeDescriptor GetReferableDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            var desc = GetDescriptor(type, cutoff, enclosing);
            if (IsPrimitiveTypeDescriptor(desc))
                return desc;
            else
                return TypeDescriptor.Referable.Create(GetTypeName(type), desc);
        }
        private TypeDescriptor GetDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            enclosing = enclosing ?? ImmutableStack<Type>.Empty;
            if (enclosing.Contains(type))
                return new TypeDescriptor.Reference(GetTypeName(type));
            return descriptors.GetOrAdd(type, ty =>
            {
                ty = representations.Where(rep => rep.IsRepresentable(ty)).Select(rep => rep.GetRepresentationType(ty)).FirstOrDefault() ?? ty;
                var desc = GetNullableDescriptor(ty, cutoff, enclosing.Push(type)) // Check for nullability
                ?? GetArrayDescriptor(ty, cutoff, enclosing.Push(type)) // Check for collections
                ?? GetFSharpUnionDescriptor(ty, cutoff, enclosing.Push(type)) // Check for F# union types
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
            => type.ToString().Replace('+', '.');

        private TypeDescriptor GetNullableDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            var ti = type.GetTypeInfo();
            var ga = ti.GetGenericArguments();
            if (ga.Length == 1)
            {
                if (ti.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var t = GetReferableDescriptor(ga[0], cutoff, enclosing);
                    return t == null ? null : new TypeDescriptor.Union(t.ToString() + "?", new TypeDescriptor[] { t, TypeDescriptor.Null.Instance });
                }
            }
            return null;
        }
        private TypeDescriptor GetArrayDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            if (type.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GetTypeInfo().GetGenericArguments().Length == 2
                    && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    || i == typeof(IDictionary))
                .Any())
                return null;
            else if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                var q = from itf in type.GetTypeInfo().ImplementedInterfaces.Concat(new[] { type })
                        let iti = itf.GetTypeInfo()
                        let ga = iti.GetGenericArguments()
                        where ga.Length == 1 && iti.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        select ga[0];
                var inner = GetReferableDescriptor((q.FirstOrDefault() ?? typeof(object)), null, enclosing);
                return new TypeDescriptor.Array(inner);
            }
            else
                return null;
        }
        private TypeDescriptor GetRecordDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            var ti = type.GetTypeInfo();
            if (ti.ImplementedInterfaces
                .Where(i => i.GetTypeInfo().GetGenericArguments().Length == 2
                    && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    || i == typeof(IDictionary))
                .Any())
                return new TypeDescriptor.Record(type.ToString(), Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>());
            else if (ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.GetParameters().Length == 0).Any())
            {
                var props = ti.Iterate(x => x.BaseType?.GetTypeInfo())
                       .TakeWhile(x => x != null)
                       .SelectMany(x => x.DeclaredProperties)
                       .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                       .GroupBy(x => x.Name)
                       .Select(x => x.First())
                       .Select(x => new PropertyDescriptor<TypeDescriptor>(x.Name, GetReferableDescriptor(x.PropertyType, null, enclosing), x.GetCustomAttributes<RequiredAttribute>().Any()));

                return props.Any()
                    ? new TypeDescriptor.Record(type.ToString(), props)
                    : null;
            }
            else
            {
                var props = ti.Iterate(x => x.BaseType?.GetTypeInfo())
                    .TakeWhile(x => x != cutoff && x != null)
                    .SelectMany(x => x.DeclaredProperties)
                    .Where(p => p.CanRead && p.GetMethod.IsPublic)
                    .GroupBy(x => x.Name)
                    .Select(x => x.First())
                    .ToArray();
                if (!props.All(pi => pi.CanRead && !pi.CanWrite))
                    return null;
                var properties = from ci in ti.DeclaredConstructors
                                 let ps = ci.GetParameters()
                                 where !ci.IsStatic && ps.Length > 0 && ps.Length >= props.Length
                                     && ps.Join(props, p => p.Name, p => p.Name, (_, __) => 1, CaseInsensitiveEqualityComparer.Instance).Count() == props.Length
                                 from p in ps.Join(props, p => p.Name, p => p.Name,
                                     (par, prop) => new PropertyDescriptor<TypeDescriptor>(prop.Name, GetReferableDescriptor(prop.PropertyType, null, enclosing), !par.GetCustomAttributes<OptionalAttribute>().Any()),
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
                && !ti.Iterate(x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != cutoff && x != null).SelectMany(x => x.DeclaredProperties.Where(p => !p.GetAccessors()[0].IsStatic)).Any()
                ? new TypeDescriptor.Record(type.ToString(), Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>())
                : null;
        }
        private TypeDescriptor GetUnionDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            var ti = type.GetTypeInfo();
            if (ti.IsAbstract && ti.DeclaredNestedTypes.Any(nt => nt.BaseType == type))
            {
                var rec = GetRecordDescriptor(type, cutoff, enclosing);
                TypeDescriptor res = new TypeDescriptor.Union(rec == null ? type.ToString() : "", ti.DeclaredNestedTypes.Where(nt => nt.BaseType == type && nt.IsPublic).Select(ty => GetReferableDescriptor(ty, type, enclosing)));

                if (rec != null)
                    res = new TypeDescriptor.Intersection(type.ToString(), new[] { rec, res });

                return res;
            }
            else
                return null;
        }
        private TypeDescriptor GetFSharpUnionDescriptor(Type type, Type cutoff, ImmutableStack<Type> enclosing)
        {
            var cma = type.GetCustomAttributes().Where(a => a.GetType().Name == CompilationMappingAttribute).FirstOrDefault();
            if (cma != null)
            {
                var prop = cma.GetType().GetProperty(SourceConstructFlags);
                if (prop != null)
                {
                    if (prop.GetValue(cma, null)?.ToString() == SumType)
                    {
                        if (type.Namespace == Microsoft_FSharp_Core && type.Name == FSharpOption_1)
                        {
                            var opt = GetDescriptor(type.GetGenericArguments()[0]);
                            return TypeDescriptor.MakeUnion($"Optional<{opt.Name}>", new[]
                            {
                                opt,
                                TypeDescriptor.Null.Instance
                            });
                        }
                        else
                        {
                            var tags = type.GetNestedType("Tags")?.GetFields().ToDictionary(f => (int)f.GetValue(null), f => f.Name);
                            var constructorMethods = type.GetMethods()
                                .Select(mi => new { Method = mi, Attribute = mi.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == CompilationMappingAttribute) })
                                .Where(m => m.Attribute != null)
                                .Select(m => new
                                {
                                    m.Method,
                                    m.Attribute,
                                    Name = getOrNull(tags, (int)m.Attribute.GetType().GetProperty(SequenceNumber).GetValue(m.Attribute)) ?? m.Method.Name.Substring(m.Method.Name.StartsWith("New") ? 3 : 0)
                                });
                            var typeDescs = constructorMethods.Select(cm =>
                                TypeDescriptor.MakeRecord(tags[(int)cm.Attribute.GetType().GetProperty(SequenceNumber).GetValue(cm.Attribute)],
                                    new[] {
                                        new PropertyDescriptor<TypeDescriptor>("Tag", TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, cm.Name), true)
                                    }.Concat(
                                        cm.Method.GetParameters()
                                        .Select(pi => new PropertyDescriptor<TypeDescriptor>(adjustName(pi.Name), GetDescriptor(pi.ParameterType), true))
                                    )));
                            return TypeDescriptor.MakeUnion(type.ToString(), typeDescs);
                        }
                    }
                }
            }
            return null;

            string adjustName(string str)
            {
                if (str.Length > 0 && str[0] == '_')
                    str = str.Substring(1);
                if (str.Length > 0 && !char.IsUpper(str[0]))
                    str = char.ToUpper(str[0]) + str.Substring(1);
                return str;
            }
            string getOrNull(Dictionary<int, string> dict, int key)
                => dict.TryGetValue(key, out var res) ? res : null;
        }
    }
}
