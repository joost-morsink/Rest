using Biz.Morsink.DataConvert;
using Biz.Morsink.DataConvert.Converters;
using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public class RestRequestHandler : IRestRequestHandler
    {
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
        private readonly IServiceLocator locator;

        public RestRequestHandler(IServiceLocator locator, IDataConverter converter = null)
        {
            this.locator = locator;
            this.converter = converter ?? DefaultDataConverter;
        }
        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            try
            {
                var type = request.Address.ForType;
                var repo = locator.ResolveOptional(typeof(IRestRepository<>).MakeGenericType(type)) as IRestRepository;
                if (repo == null)
                    return RestResult.NotFound<object>().ToResponse();

                var lps = locator.ResolveMulti(typeof(ILinkProvider<>).MakeGenericType(type));
                var dlps = locator.ResolveMulti(typeof(IDynamicLinkProvider<>).MakeGenericType(type));

                var t = Activator.CreateInstance(typeof(RestRequestHandler<>).MakeGenericType(type), new object[] { lps, dlps, converter });
                return await (ValueTask<RestResponse>)
                    typeof(RestRequestHandler<>).MakeGenericType(type).GetTypeInfo()
                    .GetDeclaredMethod(nameof(RestRequestHandler<object>.HandleTypedRequest))
                    .Invoke(t, new object[] { request, repo });
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
        private readonly IDataConverter converter;
        private readonly ILinkProvider<T>[] linkProviders;
        private readonly IDynamicLinkProvider<T>[] dynamicLinkProviders;

        public RestRequestHandler(IEnumerable<ILinkProvider<T>> linkProviders, IEnumerable<IDynamicLinkProvider<T>> dynamicLinkProviders, IDataConverter converter = null)
        {
            this.linkProviders = linkProviders.ToArray();
            this.dynamicLinkProviders = dynamicLinkProviders.ToArray();
            this.converter = converter ?? RestRequestHandler.DefaultDataConverter;

        }
        public async ValueTask<RestResponse> HandleTypedRequest(RestRequest request, IRestRepository<T> repo)
        {
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
