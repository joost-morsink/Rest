﻿using Biz.Morsink.DataConvert;
using Biz.Morsink.DataConvert.Converters;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
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
    /// As the core RestRequest handler, this component breaks down a RestRequest instance to a specific method call on a repository.
    /// </summary>
    public class CoreRestRequestHandler : IRestRequestHandler
    {
        /// <summary>
        /// A default DataConverter for conversion within the handler.
        /// </summary>
        public static IDataConverter DefaultDataConverter { get; } = new DataConverter(
                IdentityConverter.Instance,
                IsoDateTimeConverter.Instance,
                Base64Converter.Instance,
                new ToStringConverter(true),
                new TryParseConverter().Restrict((from, to) => to != typeof(bool)), // bool parsing has a custom converter in pipeline
                EnumToNumericConverter.Instance,
                SimpleNumericConverter.Instance,
                BooleanConverter.Instance,
                new NumericToEnumConverter(),
                EnumParseConverter.CaseInsensitive,
                new ToNullableConverter(),
                TupleConverter.Instance,
                RecordConverter.ForReadOnlyDictionaries(),
                RecordConverter.ForDictionaries(),
                ToObjectConverter.Instance,
                new FromStringRepresentationConverter().Restrict((from, to) => from != typeof(Version)), // Version could conflict with numeric types' syntaxes.
                new DynamicConverter());
        private readonly IDataConverter converter;
        private readonly IServiceProviderAccessor serviceProviderAccessor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="locator">A service locator to resolve repositories.</param>
        /// <param name="converter">An optional DataConverter for use within the handler.</param>
        public CoreRestRequestHandler(IServiceProviderAccessor serviceProviderAccessor, IDataConverter converter = null)
        {
            this.converter = converter ?? DefaultDataConverter;
            this.serviceProviderAccessor = serviceProviderAccessor;
        }
        /// <summary>
        /// Handles a RestRequest
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <returns>A possibly asynchronous RestResponse.</returns>
        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            try
            {
                var type = request.Address.ForType;

                var t = Activator.CreateInstance(typeof(RestRequestHandler<>).MakeGenericType(type), new object[]
                {
                    serviceProviderAccessor.ServiceProvider ?? throw new ArgumentException("No scoped IServiceProvider"),
                    converter
                });
                return await (ValueTask<RestResponse>)
                    typeof(RestRequestHandler<>).MakeGenericType(type).GetTypeInfo()
                    .GetDeclaredMethod(nameof(RestRequestHandler<object>.HandleTypedRequest))
                    .Invoke(t, new object[] { request });
            }
            catch (Exception ex)
            {
                return RestResult.Error<object>(ex).ToResponse();
            }
        }
    }
    internal class RestRequestHandler<T>
    {
        private X GetService<X>()
            => (X)serviceLocator.GetService(typeof(X));

        private readonly IServiceProvider serviceLocator;
        private readonly IEnumerable<IRestExceptionListener> exceptionListeners;
        private readonly IDataConverter converter;
        private readonly ITypeDescriptorCreator typeDescriptorCreator;
        private readonly ILinkProvider<T>[] linkProviders;
        private readonly IDynamicLinkProvider<T>[] dynamicLinkProviders;
        private readonly IAuthorizationProvider authorizationProvider;
        private readonly IUser user;

        public RestRequestHandler(IServiceProvider serviceLocator,  IDataConverter converter = null)
        {
            this.serviceLocator = serviceLocator;

            exceptionListeners = GetService<IEnumerable<IRestExceptionListener>>();
            linkProviders = GetService<IEnumerable<ILinkProvider<T>>>().ToArray();
            dynamicLinkProviders = GetService<IEnumerable<IDynamicLinkProvider<T>>>().ToArray();
            authorizationProvider = GetService<IAuthorizationProvider>();
            user = GetService<IUser>();
            this.converter = converter ?? CoreRestRequestHandler.DefaultDataConverter;
            typeDescriptorCreator = GetService<ITypeDescriptorCreator>();
        }
        public async ValueTask<RestResponse> HandleTypedRequest(RestRequest request)
        {
            var repo = GetService<IRestRepository<T>>();
            if (repo == null)
            {
                var res = RestResult<T>.Failure.NotFound.Instance(RestEntityKind.Repository).ToResponse();
                if (request.Metadata.TryGet(out Versioning ver))
                    res = res.WithMetadata(ver.WithoutCurrent());
                return res;
            }
            if (repo is IRestRequestContainer container)
                container.Request = request;

            var capabilities = repo.GetCapabilities(new RestCapabilityDescriptorKey(request.Capability, typeof(T)));
            var failures = new List<RestResponse>();
            foreach (var cap in capabilities)
            {
                var descriptor = cap.Descriptor;
                try
                {
                    var method = descriptor.BodyType != null
                        ? typeof(RestRequestHandler<T>).GetTypeInfo()
                            .GetDeclaredMethod(nameof(HandleWithBody))
                            .MakeGenericMethod(descriptor.ParameterType, descriptor.BodyType, descriptor.ResultType)
                        : typeof(RestRequestHandler<T>).GetTypeInfo()
                            .GetDeclaredMethod(nameof(Handle))
                            .MakeGenericMethod(descriptor.ParameterType, descriptor.ResultType);
                    var res = await (ValueTask<RestResponse>)method.Invoke(this, new object[] { repo, request, cap });
                    if (request.Metadata.TryGet(out Versioning ver) && !res.Metadata.TryGet(out Versioning _))
                        res = res.AddMetadata(ver);
                    if (!res.UntypedResult.IsFailure)
                    {
                        if (res is RestResponse<T> tres)
                        {
                            return tres.Select(r => r.Select(v =>
                                    v.ToLazy().Manipulate(rv => rv.Links
                                        .Concat(linkProviders.SelectMany(lp => lp.GetLinks((IIdentity<T>)request.Address)))
                                        .Concat(dynamicLinkProviders.SelectMany(lp => lp.GetLinks(rv.Value)))
                                        .Where(l => l.IsAllowedBy(authorizationProvider, user)))));
                        }
                        else
                            return res;
                    }
                    else
                        failures.Add(res);
                }
                catch (Exception ex)
                {
                    foreach (var listener in exceptionListeners)
                        listener.UnexpectedExceptionOccured(ex);
                    return RestResult.Error(descriptor.ResultType, ex).ToResponse();
                }
            }

            return failures.OrderBy(f => sortOrder(f.UntypedResult.AsFailure().Reason))
                .FirstOrDefault() ?? RestResult<T>.Failure.NotFound.Instance(RestEntityKind.Capability).ToResponse();

            int sortOrder(RestFailureReason reason)
            {
                switch (reason)
                {
                    case RestFailureReason.BadRequest:
                        return 0;
                    case RestFailureReason.Error:
                        return 1;
                    case RestFailureReason.NotFound:
                        return 2;
                    default:
                        return 3;
                }
            }
        }
        private async ValueTask<RestResponse> HandleWithBody<P, E, R>(IRestRepository repo, RestRequest request, RestCapability<T> capability)
        {
            if (!converter.Convert(request.Parameters.AsDictionary()).TryTo(out P param))
                return RestResult.BadRequest<R>("Parameter").ToResponse();
            try
            {
                var action = (Func<IIdentity<T>, P, E, CancellationToken, ValueTask<RestResponse<R>>>)capability.CreateDelegate();
                var req = request.ParseBody<E>();
                var res = await action(request.Address as IIdentity<T>, param, req.Body, request.CancellationToken);
                var result = await repo.ProcessResponse(res);
                return result;
            }
            catch (RestFailureException rfe)
            {
                return rfe.Failure.Select<R>().ToResponse();
            }
        }
        private async ValueTask<RestResponse> Handle<P, R>(IRestRepository repo, RestRequest request, RestCapability<T> capability)
        {
            if (!converter.Convert(request.Parameters.AsDictionary()).TryTo(out P param))
                return RestResult.BadRequest<R>("Parameter").ToResponse();
            try
            {
                var action = (Func<IIdentity<T>, P, CancellationToken, ValueTask<RestResponse<R>>>)capability.CreateDelegate();
                var res = await action(request.Address as IIdentity<T>, param, request.CancellationToken);
                var result = await repo.ProcessResponse(res);
                return result;
            }
            catch (RestFailureException rfe)
            {
                return rfe.Failure.Select<R>().ToResponse();
            }
        }
    }
}
