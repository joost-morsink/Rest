using Biz.Morsink.DataConvert;
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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="locator">A service locator to resolve repositories.</param>
        /// <param name="converter">An optional DataConverter for use within the handler.</param>
        public CoreRestRequestHandler(IDataConverter converter = null)
        {
            this.converter = converter ?? DefaultDataConverter;
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
                    request.Metadata.TryGet<IServiceProvider>(out var locator)
                        ? locator
                        : throw new ArgumentException("RestRequest does not carry IServiceProvider"),
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
        where T : class
    {
        private X GetService<X>()
            => (X)serviceLocator.GetService(typeof(X));

        private readonly IServiceProvider serviceLocator;
        private readonly IDataConverter converter;
        private readonly TypeDescriptorCreator typeDescriptorCreator;
        private readonly ILinkProvider<T>[] linkProviders;
        private readonly IDynamicLinkProvider<T>[] dynamicLinkProviders;
        private readonly IAuthorizationProvider authorizationProvider;
        private readonly IUser user;

        public RestRequestHandler(IServiceProvider serviceLocator, IDataConverter converter = null)
        {
            this.serviceLocator = serviceLocator;

            linkProviders = GetService<IEnumerable<ILinkProvider<T>>>().ToArray();
            dynamicLinkProviders = GetService<IEnumerable<IDynamicLinkProvider<T>>>().ToArray();
            authorizationProvider = GetService<IAuthorizationProvider>();
            user = GetService<IUser>();
            this.converter = converter ?? CoreRestRequestHandler.DefaultDataConverter;
            typeDescriptorCreator = GetService<TypeDescriptorCreator>();
        }
        public async ValueTask<RestResponse> HandleTypedRequest(RestRequest request)
        {
            var repo = GetService<IRestRepository<T>>();
            if (repo == null)
                return RestResult.NotFound<T>().ToResponse();
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
                    var res = await (ValueTask<RestResponse>)method.Invoke(this, new object[] { request, cap });
                    if (!res.UntypedResult.IsFailure)
                    {
                        if (cap.Descriptor.Name == "GET")
                        {
                            return res.Select(r => r.Select(v =>
                                    v.Manipulate(rv => rv.Links
                                        .Concat(linkProviders.SelectMany(lp => lp.GetLinks((IIdentity<T>)request.Address)))
                                        .Concat(dynamicLinkProviders.SelectMany(lp => lp.GetLinks((T)rv.Value)))
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
                    return RestResult.Error(descriptor.ResultType, ex).ToResponse();
                }
            }

            return failures.OrderBy(f => sortOrder(f.UntypedResult.AsFailure().Reason))
                .FirstOrDefault() ?? RestResult.NotFound<T>().ToResponse();

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
        private async ValueTask<RestResponse> HandleWithBody<P, E, R>(RestRequest request, RestCapability<T> capability)
            where R : class
        {
            if (!converter.Convert(request.Parameters.AsDictionary()).TryTo(out P param))
                return RestResult.BadRequest<R>("Parameter").ToResponse();
            var action = (Func<IIdentity<T>, P, E, ValueTask<RestResponse<R>>>)capability.CreateDelegate();
            var req = request.ParseBody<E>();
            var res = await action(request.Address as IIdentity<T>, param, req.Body);
            return res;
        }
        private async ValueTask<RestResponse> Handle<P, R>(RestRequest request, RestCapability<T> capability)
            where R : class
        {
            if (!converter.Convert(request.Parameters.AsDictionary()).TryTo(out P param))
                return RestResult.BadRequest<R>("Parameter").ToResponse();
            var action = (Func<IIdentity<T>, P, ValueTask<RestResponse<R>>>)capability.CreateDelegate();
            var res = await action(request.Address as IIdentity<T>, param);
            return res;
        }
    }
}
