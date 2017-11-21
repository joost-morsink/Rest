﻿using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Utils;
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
        private static (Type, Type) ExtractGeneric(this Type type)
        {
            var ti = type.GetTypeInfo();
            if (ti.GenericTypeArguments.Length == 1)
            {
                return (ti.GetGenericTypeDefinition(), ti.GenericTypeArguments[0]);
            }
            else
            {
                return (null, type);
            }
        }
        private static IEnumerable<T> Iterate<T>(this T seed, Func<T, T> next)
        {
            while (true)
            {
                yield return seed;
                seed = next(seed);
            }
        }
        public static IEnumerable<(Type, Func<IServiceProvider, IRestRepository>)> GetRepositoryFactories<C>(Func<IServiceProvider, C> containerFactory)
        {
            var targets = from mi in typeof(C).GetTypeInfo().DeclaredMethods
                          let target = mi.GetParameters()[0].ParameterType.GetGeneric(typeof(IIdentity<>))
                          where target != null
                          from attr in mi.GetCustomAttributes<RestAttribute>()
                          group (mi, attr) by target;

            var res = targets.Select(t =>
            {
                var factory = CreateRepositoryFactory<C>(t.Key, t);
                return (t.Key, new Func<IServiceProvider, IRestRepository>(sp => factory(containerFactory(sp))));
            }).ToArray();

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
            private readonly Func<C, IIdentity<T>, P, RestRequest, CancellationToken, ValueTask<RestResponse<T>>> func;

            public RestGetMaker(Func<C, IIdentity<T>, P, RestRequest, CancellationToken, ValueTask<RestResponse<T>>> func)
            {
                this.func = func;
            }
            public IRestCapability<T> Make(C container)
                => new Capability(container, func);
            private class Capability : IRestGet<T, P>, IRestRequestContainer
            {
                private readonly C container;
                private readonly Func<C, IIdentity<T>, P, RestRequest, CancellationToken, ValueTask<RestResponse<T>>> func;

                public Capability(C c, Func<C, IIdentity<T>, P, RestRequest, CancellationToken, ValueTask<RestResponse<T>>> func)
                {
                    this.container = c;
                    this.func = func;
                    Request = (c as IRestRequestContainer)?.Request;
                }

                public RestRequest Request { get; set; }

                public ValueTask<RestResponse<T>> Get(IIdentity<T> id, P parameters, CancellationToken cancellationToken)
                    => func(container, id, parameters, Request, cancellationToken);
            }
        }
        private static Func<C, IRestCapability<T>> MakeGet<C, T>(MethodInfo methodInfo)
        {
            var r = MakeFunc<C, T>(methodInfo, false);
            if (r.InnerReturn != typeof(T))
                throw new ArgumentException("Get method should return an entity of the addressed type.");
            var maker = (ICapabilityMaker<C, T>)Activator.CreateInstance(typeof(RestGetMaker<,,>).MakeGenericType(typeof(C), typeof(T), r.Parameter), r.Function);
            return maker.Make;
        }

        #region Helpers for making functions
        private class MakeFuncResult
        {
            public MakeFuncResult(Type parameterType, Type bodyType, Type innerReturnType, Delegate function)
            {
                Parameter = parameterType;
                Body = bodyType;
                InnerReturn = innerReturnType;
                Function = function;
            }
            public Type Parameter { get; }
            public Type Body { get; }
            public Type InnerReturn { get; }
            public Delegate Function { get; }
        }
        private static MakeFuncResult MakeFunc<C, T>(MethodInfo methodInfo, bool withBody)
        {
            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 0 || parameters[0].ParameterType != typeof(IIdentity<T>))
                throw new ArgumentException("Method should have first parameter");

            var container = Ex.Parameter(typeof(C), "container");
            var id = Ex.Parameter(typeof(IIdentity<T>), "id");
            var p = parameters.Length == 1 || parameters[1].GetCustomAttributes<RestBodyAttribute>().Any()
                ? Ex.Parameter(typeof(Empty), "p")
                : Ex.Parameter(parameters[1].ParameterType, "p");
            var body = parameters.Any(par => par.GetCustomAttributes<RestBodyAttribute>().Any())
                ? Ex.Parameter(parameters.First(par => par.GetCustomAttributes<RestBodyAttribute>().Any()).ParameterType, "body")
                : parameters.Length > 2 && parameters[2].ParameterType != typeof(CancellationToken) && parameters[2].ParameterType != typeof(RestRequest)
                    ? Ex.Parameter(parameters[2].ParameterType, "body")
                    : Ex.Parameter(typeof(Empty), "body");
            var request = Ex.Parameter(typeof(RestRequest), "request");
            var cancel = Ex.Parameter(typeof(CancellationToken), "cancel");
            var retType = methodInfo.ReturnType;
            var (_, _, innerType) = SplitTypes(methodInfo.ReturnType);

            var methodParams = methodInfo.GetParameters().Join(withBody ? new[] { id, p, body, request, cancel } : new[] { id, p, request, cancel }, par => par.ParameterType, exp => exp.Type, (par, exp) => exp);
            if (methodParams.Count() != methodInfo.GetParameters().Length)
                throw new ArgumentException("Method parameters cannot be satisfied by availiable expressions.");

            var lambda = withBody
                ? Ex.Lambda(WrapExpression(
                    Ex.Call(container, methodInfo, methodParams),
                    methodInfo.ReturnType),
                    container, id, p, body, request, cancel)
                : Ex.Lambda(WrapExpression(
                    Ex.Call(container, methodInfo, methodParams),
                    methodInfo.ReturnType),
                    container, id, p, request, cancel);
            var func = lambda.Compile();

            return new MakeFuncResult(p.Type, body.Type, innerType, func);
        }
        private static Ex WrapExpression(Ex expression, Type retType)
        {
            var (asyncConstructor, restConstructor, inner) = SplitTypes(retType);

            if (asyncConstructor == null && restConstructor == null)
            {
                return Ex.Call(Ex.New(typeof(RestValue<>).MakeGenericType(inner).GetConstructor(new[] { inner }), expression),
                    nameof(RestValue<object>.ToResponseAsync), Type.EmptyTypes,
                    Ex.Default(typeof(TypeKeyedDictionary)));
            }
            if (asyncConstructor == null)
            {
                if (restConstructor == typeof(RestValue<>))
                    return Ex.Call(expression,
                        nameof(RestValue<object>.ToResponseAsync), Type.EmptyTypes,
                        Ex.Default(typeof(TypeKeyedDictionary)));
                else if (restConstructor == typeof(RestResult<>))
                    return Ex.Call(expression,
                        nameof(RestResult<object>.ToResponseAsync), Type.EmptyTypes,
                        Ex.Default(typeof(TypeKeyedDictionary)));
                else if (restConstructor == typeof(RestResponse<>))
                    return Ex.Call(expression, nameof(RestResponse<object>.ToAsync), Type.EmptyTypes,
                        Ex.Default(typeof(TypeKeyedDictionary)));
                else
                    throw new ArgumentException($"Unknown rest typeconstructor {restConstructor}");
            }
            else if (asyncConstructor == typeof(Task<>))
            {
                if (restConstructor == null)
                    return Ex.Call(typeof(AttributedRestRepositories).GetTypeInfo().DeclaredMethods
                        .First(m => m.Name == nameof(ConvertTaskToResponseAsync)).MakeGenericMethod(inner),
                        expression);
                else if (restConstructor == typeof(RestValue<>))
                    return Ex.Call(typeof(AttributedRestRepositories).GetTypeInfo().DeclaredMethods
                        .First(m => m.Name == nameof(ConvertValueToResponseAsync)).MakeGenericMethod(inner),
                        expression);
                else if (restConstructor == typeof(RestResult<>))
                    return Ex.Call(typeof(AttributedRestRepositories).GetTypeInfo().DeclaredMethods
                        .First(m => m.Name == nameof(ConvertResultToRestResponseAsync)).MakeGenericMethod(inner),
                        expression);
                else if (restConstructor == typeof(RestResponse<>))
                    return Ex.New(typeof(ValueTask<>).MakeGenericType(typeof(RestResponse<>).MakeGenericType(inner))
                        .GetConstructor(new[] { typeof(Task<>).MakeGenericType(typeof(RestResponse<>).MakeGenericType(inner)) }),
                        expression);
                else
                    throw new ArgumentException($"Unknown rest typeconstructor {restConstructor}");
            }
            else if (asyncConstructor == typeof(ValueTask<>))
            {
                if (restConstructor == null)
                    return Ex.Call(typeof(AttributedRestRepositories).GetTypeInfo().DeclaredMethods
                        .First(m => m.Name == nameof(ConvertVtToResponseAsync)).MakeGenericMethod(inner),
                        expression);
                else if (restConstructor == typeof(RestValue<>))
                    return Ex.Call(typeof(AttributedRestRepositories).GetTypeInfo().DeclaredMethods
                        .First(m => m.Name == nameof(ConvertVtValueToResponseAsync)).MakeGenericMethod(inner),
                        expression);
                else if (restConstructor == typeof(RestResult<>))
                    return Ex.Call(typeof(AttributedRestRepositories).GetTypeInfo().DeclaredMethods
                        .First(m => m.Name == nameof(ConvertVtResultToRestResponseAsync)).MakeGenericMethod(inner),
                        expression);
                else if (restConstructor == typeof(RestResponse<>))
                    return expression;
                else
                    throw new ArgumentException($"Unknown rest typeconstructor {restConstructor}");
            }
            else
                throw new ArgumentException($"Unknown async typeconstructor {asyncConstructor}");
        }
        private static (Type[], Type) ExtractTypeConstructorList(this Type type)
        {
            var lst = new List<Type>();
            (Type, Type) split;
            do
            {
                split = type.ExtractGeneric();
                if (split.Item1 != null)
                {
                    lst.Add(split.Item1);
                    type = split.Item2;
                }
            } while (split.Item1 != null);
            return (lst.ToArray(), split.Item2);
        }
        private static (Type, Type, Type) SplitTypes(Type type)
        {
            var (constrs, inner) = ExtractTypeConstructorList(type);
            Type async = null;
            Type rest = null;
            foreach (var t in constrs.Reverse())
            {
                if (t == typeof(RestValue<>) || t == typeof(RestResult<>) || t == typeof(RestResponse<>))
                {
                    if (async != null || rest != null)
                        throw new ArgumentException("Unknown type construction.");
                    rest = t;
                }
                else if (t == typeof(Task<>) || t == typeof(ValueTask<>))
                {
                    if (async != null)
                        throw new ArgumentException("Unknown type construction.");
                    async = t;
                }
                else if (async == null && rest == null)
                    inner = t.MakeGenericType(inner);
            }
            return (async, rest, inner);
        }
        private static async ValueTask<RestResponse<T>> ConvertTaskToResponseAsync<T>(Task<T> val)
            where T : class
            => Rest.Value(await val).ToResponse();
        private static async ValueTask<RestResponse<T>> ConvertValueToResponseAsync<T>(Task<RestValue<T>> val)
            where T : class
            => (await val).ToResponse();
        private static async ValueTask<RestResponse<T>> ConvertResultToRestResponseAsync<T>(Task<RestResult<T>> val)
            where T : class
            => (await val).ToResponse();
        private static async ValueTask<RestResponse<T>> ConvertVtToResponseAsync<T>(ValueTask<T> val)
            where T : class
            => Rest.Value(await val).ToResponse();
        private static async ValueTask<RestResponse<T>> ConvertVtValueToResponseAsync<T>(ValueTask<RestValue<T>> val)
            where T : class
            => (await val).ToResponse();
        private static async ValueTask<RestResponse<T>> ConvertVtResultToRestResponseAsync<T>(ValueTask<RestResult<T>> val)
            where T : class
            => (await val).ToResponse();
        #endregion

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
