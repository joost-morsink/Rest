
using Biz.Morsink.Identity.PathProvider;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class RecordDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static RecordDescriptorKind Instance { get; } = new RecordDescriptorKind();
        private RecordDescriptorKind() { }
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var ti = context.Type.GetTypeInfo();

            if (ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.GetParameters().Length == 0).Any())
            {
                var props = ti.Iterate(x => x.BaseType?.GetTypeInfo())
                       .TakeWhile(x => x != null)
                       .SelectMany(x => x.DeclaredProperties)
                       .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                       .GroupBy(x => x.Name)
                       .Select(x => x.First())
                       .Select(x => new PropertyDescriptor<TypeDescriptor>(x.Name, creator.GetReferableDescriptor(context.WithType(x.PropertyType).WithCutoff( null)), x.GetCustomAttributes<RequiredAttribute>().Any()));

                return props.Any()
                    ? new TypeDescriptor.Record(context.Type.ToString(), props)
                    : null;
            }
            else
            {
                var props = ti.Iterate(x => x.BaseType?.GetTypeInfo())
                    .TakeWhile(x => x != context.Cutoff && x != null)
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
                                     (par, prop) => new PropertyDescriptor<TypeDescriptor>(prop.Name, creator.GetReferableDescriptor(context.WithType(prop.PropertyType).WithCutoff(null)), !par.GetCustomAttributes<OptionalAttribute>().Any()),
                                     CaseInsensitiveEqualityComparer.Instance)
                                 select p;

                return properties.Any() ? new TypeDescriptor.Record(context.Type.ToString(), properties) : null;
            }

        }
    }
}
