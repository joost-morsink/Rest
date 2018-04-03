using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Key component of a Rest capability descriptor
    /// </summary>
    public class RestCapabilityDescriptorKey : IEquatable<RestCapabilityDescriptorKey>
    {
        /// <summary>
        /// Creates a Rest capability descriptor key based on an interface type.
        /// The interface must be attributed with a CapabilityAttribute and must contain exactly 1 method of the following form:
        /// 1. The return type must be ValueTask&lt;RestResponse&;ltR&gt;&gt; for some R.
        /// 2. The first parameter must be IIdentity&lt;T&gt; for some resource type T.
        /// 3. The second parameter must be P for some parameter type P.
        /// 4. The third parameter is optional and describes an entity of type E used for the execution of the capability.
        /// </summary>
        /// <param name="interfaceType">The Rest capability interface's type.</param>
        /// <returns>A Rest capability descriptor key.</returns>
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
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the capability.</param>
        /// <param name="entityType">The resource type of the capability.</param>
        public RestCapabilityDescriptorKey(string name, Type entityType)
        {
            Name = name;
            EntityType = entityType;
        }
        /// <summary>
        /// Gets the name of the capability.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the resource type of the capability.
        /// </summary>
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
                || !ReferenceEquals(x, null) && x.Equals(y);
        public static bool operator !=(RestCapabilityDescriptorKey x, RestCapabilityDescriptorKey y)
            => !(x == y);

    }
    /// <summary>
    /// A class containing descriptive information about a Rest capability.
    /// </summary>
    public class RestCapabilityDescriptor : RestCapabilityDescriptorKey
    {
        /// <summary>
        /// Creates a Rest capability descriptor based on an interface type.
        /// The interface must be attributed with a CapabilityAttribute and must contain exactly 1 method of the following form:
        /// 1. The return type must be ValueTask&lt;RestResponse&;ltR&gt;&gt; for some R.
        /// 2. The first parameter must be IIdentity&lt;T&gt; for some resource type T.
        /// 3. The second parameter must be P for some parameter type P.
        /// 4. The third parameter is optional and describes an entity of type E used for the execution of the capability.
        /// </summary>
        /// <param name="interfaceType">The Rest capability interface's type.</param>
        /// <returns>A Rest capability descriptor key.</returns>
        public static new RestCapabilityDescriptor Create(Type interfaceType)
        {
            var iti = interfaceType.GetTypeInfo();
            var key = RestCapabilityDescriptorKey.Create(interfaceType);
            if (key == null)
                return null;

            var method = iti.DeclaredMethods.Single();
            

            var par = method.GetParameters().Skip(1).Select(p => p.ParameterType).Where(pt => pt != typeof(CancellationToken)).FirstOrDefault();
            var body = method.GetParameters().Skip(2).Select(p => p.ParameterType).Where(pt => pt != typeof(CancellationToken)).FirstOrDefault();
            var result = method.ReturnType.GenericTypeArguments.Length == 1
                && (method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) || method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                && method.ReturnType.GenericTypeArguments[0].GenericTypeArguments.Length == 1
                && method.ReturnType.GenericTypeArguments[0].GetGenericTypeDefinition() == typeof(RestResponse<>)
                ? method.ReturnType.GenericTypeArguments[0].GenericTypeArguments[0]
                : null;
            return new RestCapabilityDescriptor(key.Name, key.EntityType, par, body, result, interfaceType, null);
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the Rest capability.</param>
        /// <param name="entityType">The resource type for the capability.</param>
        /// <param name="parameterType">The parameter type for the capability.</param>
        /// <param name="bodyType">The body type for the capability.</param>
        /// <param name="resultType">The result type for the capability.</param>
        /// <param name="interfaceType">The Rest capability interface's type.</param>
        /// <param name="method">The implementation method for the capability.</param>
        public RestCapabilityDescriptor(string name, Type entityType, Type parameterType, Type bodyType, Type resultType, Type interfaceType, MethodInfo method)
            : base(name, entityType)
        {
            ParameterType = parameterType;
            BodyType = bodyType;
            ResultType = resultType;
            InterfaceType = interfaceType;
            Method = method;
        }
        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        public Type ParameterType { get; }
        /// <summary>
        /// Gets the body type.
        /// </summary>
        public Type BodyType { get; }
        /// <summary>
        /// Gets the result type.
        /// </summary>
        public Type ResultType { get; }
        /// <summary>
        /// Gets the interface type.
        /// </summary>
        public Type InterfaceType { get; }
        /// <summary>
        /// Gets the implementation method.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Creates a Func delegate for the capability method on the specified target instance.
        /// </summary>
        /// <param name="target"></param>
        /// <returns>A Func delegate for the capability method on the specified target instance.</returns>
        public Delegate CreateDelegate(object target)
        {
            var mi = InterfaceType.GetTypeInfo().DeclaredMethods.Single();
            if (BodyType == null)
                return mi.CreateDelegate(typeof(Func<,,,>).MakeGenericType(
                    typeof(IIdentity<>).MakeGenericType(EntityType),
                    ParameterType,
                    typeof(CancellationToken),
                    typeof(ValueTask<>).MakeGenericType(typeof(RestResponse<>).MakeGenericType(ResultType))), target);
            else
                return mi.CreateDelegate(typeof(Func<,,,,>).MakeGenericType(
                    typeof(IIdentity<>).MakeGenericType(EntityType),
                    ParameterType,
                    BodyType,
                    typeof(CancellationToken),
                    typeof(ValueTask<>).MakeGenericType(typeof(RestResponse<>).MakeGenericType(ResultType))), target);
        }
        /// <summary>
        /// Sets the Method property on a new descriptor object.
        /// </summary>
        /// <param name="methodInfo">The implementation method.</param>
        /// <returns>A new RestCapabilityDescriptor with given implementation method.</returns>
        public RestCapabilityDescriptor WithMethod(MethodInfo methodInfo)
            => new RestCapabilityDescriptor(Name, EntityType, ParameterType, BodyType, ResultType, InterfaceType, methodInfo);
    }
}
