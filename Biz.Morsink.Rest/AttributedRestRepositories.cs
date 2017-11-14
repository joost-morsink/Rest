using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest
{
    public abstract class RestAttribute : Attribute
    {
        public abstract string Capability { get; }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestGetAttribute : RestAttribute
    {
        public override string Capability => "GET";
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPutAttribute : RestAttribute
    {
        public override string Capability => "PUT";
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPostAttribute : RestAttribute
    {
        public override string Capability => "POST";
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPatchAttribute : RestAttribute
    {
        public override string Capability => "PATCH";
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestDeleteAttribute : RestAttribute
    {
        public override string Capability => "DELETE";
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RestBodyAttribute : Attribute { }

    public static class AttributedRestRepositories
    {
        private interface ICapabilityMaker<C, T>
        {
            IRestCapability<T> Make(C container);
        }
        private static Type GetGeneric(this Type type, Type interf)
        => type.GetTypeInfo().ImplementedInterfaces.Concat(new[] { type })
            .Where(i => i.GetGenericArguments().Length == 1 && i.GetGenericTypeDefinition() == interf)
            .Select(i => i.GetGenericArguments()[0])
            .FirstOrDefault();

        public static IEnumerable<(Type, Func<IServiceProvider, IRestRepository>)> GetRepositoryFactories<C>(Func<IServiceProvider, C> containerFactory)
        {
            var targets = from mi in typeof(C).GetTypeInfo().DeclaredMethods
                          let target = mi.GetParameters()[0].ParameterType.GetGeneric(typeof(IIdentity<>))
                          where target != null
                          from attr in mi.GetCustomAttributes<RestAttribute>()
                          group (mi, attr) by target;

            var res = targets.Select(t => (t.Key, new Func<IServiceProvider, IRestRepository>(sp => CreateRepositoryFactory<C>(t.Key, t)(containerFactory(sp))))).ToArray();

            return res;
        }
        private static Func<C, IRestRepository> CreateRepositoryFactory<C>(Type key, IEnumerable<(MethodInfo, RestAttribute)> methods)
        {
            var method = typeof(AttributedRestRepositories).GetTypeInfo().DeclaredMethods
               .Where(m => m.Name == nameof(CreateRepositoryFactory) && m.GetGenericArguments().Length == 2)
               .First();
            return (Func<C, IRestRepository>)method.MakeGenericMethod(typeof(C), key).Invoke(null, new[] { methods });
        }
        private static Func<C, IRestRepository<T>> CreateRepositoryFactory<C, T>(IEnumerable<(MethodInfo, RestAttribute)> methods)
        {
            var capabilities = new List<Func<C, IRestCapability<T>>>();
            foreach (var (method, attr) in methods)
            {
                switch (attr.Capability)
                {
                    case "GET":
                        capabilities.Add(MakeGet<C, T>(method));
                        break;
                }
            }
            return c => new Repository<T>(capabilities.Select(cap => cap(c)));
        }

        private class RestGetMaker<C, T, P> : ICapabilityMaker<C, T>
            where T : class
        {
            private readonly Func<C, IIdentity<T>, P, CancellationToken, ValueTask<RestResponse<T>>> func;

            public RestGetMaker(Func<C, IIdentity<T>, P, CancellationToken, ValueTask<RestResponse<T>>> func)
            {
                this.func = func;
            }
            public IRestCapability<T> Make(C container)
                => new Capability(container, func);
            private class Capability : IRestGet<T, P>
            {
                private readonly C container;
                private readonly Func<C, IIdentity<T>, P, CancellationToken, ValueTask<RestResponse<T>>> func;

                public Capability(C c, Func<C, IIdentity<T>, P, CancellationToken, ValueTask<RestResponse<T>>> func)
                {
                    this.container = c;
                    this.func = func;
                }

                public ValueTask<RestResponse<T>> Get(IIdentity<T> id, P parameters, CancellationToken cancellationToken)
                    => func(container, id, parameters, cancellationToken);
            }
        }
        private static Func<C, IRestCapability<T>> MakeGet<C, T>(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 0 || parameters[0].ParameterType != typeof(IIdentity<T>))
                throw new ArgumentException("Method should have first parameter");

            var container = Ex.Parameter(typeof(C), "container");
            var id = Ex.Parameter(typeof(IIdentity<T>), "id");
            var p = parameters.Length == 1 || parameters[1].GetCustomAttributes<RestBodyAttribute>().Any()
                ? Ex.Parameter(typeof(Empty), "p")
                : Ex.Parameter(parameters[1].ParameterType, "p");
            var cancel = Ex.Parameter(typeof(CancellationToken), "cancel");

            var lambda = Ex.Lambda(
                Ex.Call(container, methodInfo,
                    methodInfo.GetParameters().Join(new[] { id, p, cancel }, par => par.ParameterType, exp => exp.Type, (par, exp) => exp)),
                container, id, p, cancel);
            var func = lambda.Compile();

            var maker = (ICapabilityMaker<C, T>)Activator.CreateInstance(typeof(RestGetMaker<,,>).MakeGenericType(typeof(C), typeof(T), p.Type), func);

            return maker.Make;
        }

        private class Repository<T> : RestRepository<T>
        {
            private readonly IRestCapability<T>[] capabilities;
            public Repository(IEnumerable<IRestCapability<T>> capabilities)
            {
                this.capabilities = capabilities.ToArray();
                foreach (var cap in capabilities)
                    RegisterDynamic(cap);
            }

        }
    }
}
