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
        private readonly Dictionary<Type, IRestRepository> repositories;
        private readonly IDataConverter converter;

        public RestRequestHandler(IEnumerable<IRestRepository> repositories, IDataConverter converter = null)
        {
            this.repositories = repositories.ToDictionary(r => r.EntityType);
            this.converter = converter ?? new DataConverter(
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
        }
        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            try
            {
                var type = request.Address.ForType;
                if (!repositories.TryGetValue(type, out var repo))
                    return RestResult.NotFound<object>().ToResponse();

                return await (ValueTask<RestResponse>)
                    typeof(RestRequestHandler).GetTypeInfo()
                    .GetDeclaredMethod(nameof(HandleTypedRequest))
                    .MakeGenericMethod(type)
                    .Invoke(this, new object[] { request, repo });
            }
            catch (Exception ex)
            {
                return RestResult.Error<object>(ex).ToResponse();
            }
        }
        private async ValueTask<RestResponse> HandleTypedRequest<T>(RestRequest request, IRestRepository<T> repo)
            where T : class
        {
            var capabilities = repo.GetCapabilities(new RestCapabilityDescriptorKey(request.Capability, typeof(T)));

            foreach (var cap in capabilities)
            {
                System.Diagnostics.Debug.WriteLine($"{cap.Descriptor.Name} on {cap.Descriptor.EntityType} with parameter {cap.Descriptor.ParameterType}");
                var descriptor = cap.Descriptor;
                try
                {
                    var method = descriptor.BodyType != null
                        ? typeof(RestRequestHandler).GetTypeInfo()
                            .GetDeclaredMethod(nameof(HandleWithBody))
                            .MakeGenericMethod(descriptor.EntityType, descriptor.ParameterType, descriptor.BodyType, descriptor.ResultType)
                        : typeof(RestRequestHandler).GetTypeInfo()
                            .GetDeclaredMethod(nameof(Handle))
                            .MakeGenericMethod(descriptor.EntityType, descriptor.ParameterType, descriptor.ResultType);
                    var res = await (ValueTask<RestResponse>)method.Invoke(this, new object[] { request, cap });
                    if (res.IsSuccess)
                        return res;
                }
                catch (Exception ex)
                {
                    return RestResult.Error(descriptor.ResultType, ex).ToResponse();
                }
            }
            return RestResult.NotFound<T>().ToResponse();

        }
        private async ValueTask<RestResponse> HandleWithBody<T, P, E, R>(RestRequest request, RestCapability<T> capability)
            where T : class
            where R : class
        {
            if (!converter.Convert(request.RequestParameters.AsDictionary()).TryTo(out P param))
                return RestResult.BadRequest<R>("Parameter").ToResponse();
            var action = (Func<IIdentity<T>, P, E, ValueTask<RestResult<R>>>)capability.CreateDelegate();
            var body = (E)request.BodyParser(typeof(E));
            var res = await action(request.Address as IIdentity<T>, param, body);
            return res.ToResponse();
        }
        private async ValueTask<RestResponse> Handle<T, P, R>(RestRequest request, RestCapability<T> capability)
            where R : class
        {
            if (!converter.Convert(request.RequestParameters.AsDictionary()).TryTo(out P param))
                return RestResult.BadRequest<R>("Parameter").ToResponse();
            var action = (Func<IIdentity<T>, P, ValueTask<RestResult<R>>>)capability.CreateDelegate();
            var res = await action(request.Address as IIdentity<T>, param);
            return res.ToResponse();
        }
    }
}
