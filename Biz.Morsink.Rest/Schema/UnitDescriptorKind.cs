using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class UnitDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static UnitDescriptorKind Instance { get; } = new UnitDescriptorKind();
        private UnitDescriptorKind() { }
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var ti = context.Type.GetTypeInfo();
            var parameterlessConstructors = ti.DeclaredConstructors.Where(ci => !ci.IsStatic && ci.GetParameters().Length == 0);
            return parameterlessConstructors.Any()
                && !ti.Iterate(x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != context.Cutoff && x != null).SelectMany(x => x.DeclaredProperties.Where(p => !p.GetAccessors()[0].IsStatic)).Any()
                ? new TypeDescriptor.Record(context.Type.ToString(), Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>())
                : null;
        }
    }
}
