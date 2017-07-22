using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public class RestCapabilityDescriptorKey : IEquatable<RestCapabilityDescriptorKey>
    {
        public static RestCapabilityDescriptorKey Create(Type interfaceType)
        {
            var iti = interfaceType.GetTypeInfo();
            var name = iti.GetCustomAttribute<CapabilityAttribute>()?.Name;
            if (name == null)
                return null;
            var method = iti.DeclaredMethods.Single();
            var entity = (from i in iti.ImplementedInterfaces
                          where i.GenericTypeArguments.Length == 1
                          let gen = i.GetGenericTypeDefinition()
                          where gen == typeof(IRestCapability<>)
                          select i.GenericTypeArguments[0]).FirstOrDefault();
            if (entity == null)
                return null;
            if (method.GetParameters().Length == 0
                || method.GetParameters()[0].ParameterType != typeof(IIdentity<>).MakeGenericType(entity))
                return null;
            return new RestCapabilityDescriptorKey(name, entity);
        }
        public RestCapabilityDescriptorKey(string name, Type entityType)
        {
            Name = name;
            EntityType = entityType;
        }
        public string Name { get; }
        public Type EntityType { get; }
        public override int GetHashCode()
            => Name.GetHashCode() ^ EntityType.GetHashCode();
        public override bool Equals(object obj)
            => Equals(obj as RestCapabilityDescriptorKey);
        public bool Equals(RestCapabilityDescriptorKey other)
            => other != null
                && Name == other.Name
                && EntityType == other.EntityType;
        public static bool operator ==(RestCapabilityDescriptorKey x, RestCapabilityDescriptorKey y)
            => ReferenceEquals(x, y)
                || x != null && x.Equals(y);
        public static bool operator !=(RestCapabilityDescriptorKey x, RestCapabilityDescriptorKey y)
            => !(x == y);

    }
    public class RestCapabilityDescriptor : RestCapabilityDescriptorKey
    {
        public static new RestCapabilityDescriptor Create(Type interfaceType)
        {
            var iti = interfaceType.GetTypeInfo();
            var key = RestCapabilityDescriptorKey.Create(interfaceType);
            if (key == null)
                return null;

            var method = iti.DeclaredMethods.Single();

            var body = method.GetParameters().Skip(1).Select(p => p.ParameterType).FirstOrDefault();
            var result = method.ReturnType.GenericTypeArguments.Length == 1
                && (method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) || method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                && method.ReturnType.GenericTypeArguments[0].GenericTypeArguments.Length == 1
                && method.ReturnType.GenericTypeArguments[0].GetGenericTypeDefinition() == typeof(RestResult<>)
                ? method.ReturnType.GenericTypeArguments[0].GenericTypeArguments[0]
                : null;
            return new RestCapabilityDescriptor(key.Name, key.EntityType, body, result, interfaceType);
        }
        public RestCapabilityDescriptor(string name, Type entityType, Type bodyType, Type resultType, Type interfaceType)
            : base(name, entityType)
        {
            BodyType = bodyType;
            ResultType = resultType;
            InterfaceType = interfaceType;
        }
        public Type BodyType { get; }
        public Type ResultType { get; }
        public Type InterfaceType { get; }
    }
}
