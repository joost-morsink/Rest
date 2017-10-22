using Biz.Morsink.DataConvert;
using Biz.Morsink.DataConvert.Converters;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;
using Biz.Morsink.Rest.Schema;
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
        private readonly IServiceProvider locator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="locator">A service locator to resolve repositories.</param>
        /// <param name="converter">An optional DataConverter for use within the handler.</param>
        public CoreRestRequestHandler(IServiceProvider locator, IDataConverter converter = null)
        {
            this.locator = locator;
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

                var t = Activator.CreateInstance(typeof(RestRequestHandler<>).MakeGenericType(type), new object[] { locator, converter });
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

        public RestRequestHandler(IServiceProvider serviceLocator, IDataConverter converter = null)
        {
            this.serviceLocator = serviceLocator;

            linkProviders = GetService<IEnumerable<ILinkProvider<T>>>().ToArray();
            dynamicLinkProviders = GetService<IEnumerable<IDynamicLinkProvider<T>>>().ToArray();
            this.converter = converter ?? CoreRestRequestHandler.DefaultDataConverter;
            typeDescriptorCreator = GetService<TypeDescriptorCreator>();
        }
        public async ValueTask<RestResponse> HandleTypedRequest(RestRequest request)
        {
            var repo = GetService<IRestRepository<T>>();
            if (repo == null)
                return RestResult.NotFound<T>().ToResponse();
   
            var capabilities = repo.GetCapabilities(new RestCapabilityDescriptorKey(request.Capability, typeof(T)));

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
                    if (res.IsSuccess)
                    {
                        if (cap.Descriptor.Name == "GET")
                        {
                            return res.Select(r => r.Select(v =>
                                    v.Manipulate(rv => rv.Links
                                        .Concat(linkProviders.SelectMany(lp => lp.GetLinks((IIdentity<T>)request.Address)))
                                        .Concat(dynamicLinkProviders.SelectMany(lp => lp.GetLinks((T)rv.Value))))));
                        }
                        else
                            return res;
                    }
                }
                catch (Exception ex)
                {
                    return RestResult.Error(descriptor.ResultType, ex).ToResponse();
                }
            }
            return RestResult.NotFound<T>().ToResponse();

        }
        private async ValueTask<RestResponse> HandleWithBody<P, E, R>(RestRequest request, RestCapability<T> capability)
            where R : class
        {
            if (!converter.Convert(request.Parameters.AsDictionary()).TryTo(out P param))
                return RestResult.BadRequest<R>("Parameter").ToResponse();
            var action = (Func<IIdentity<T>, P, E, ValueTask<RestResponse<R>>>)capability.CreateDelegate();
            var body = (E)request.BodyParser(typeof(E));
            var res = await action(request.Address as IIdentity<T>, param, body);
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
