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
        private Dictionary<Type, IRestRepository> repositories;
        public RestRequestHandler(IEnumerable<IRestRepository> repositories)
        {
            this.repositories = repositories.ToDictionary(r => r.EntityType);
        }
        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            try
            {
                var type = request.Address.ForType;
                if(!repositories.TryGetValue(type, out var repo))
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
        {
            var capability = repo.GetCapability(new RestCapabilityDescriptorKey(request.Capability, typeof(T)));
            if (!capability.HasValue)
                return RestResult.BadRequest<object>(null).ToResponse();
            var descriptor = capability.Value.Descriptor;
            try
            {
                if (descriptor.BodyType != null)
                {
                    return await (ValueTask<RestResponse>)
                        typeof(RestRequestHandler).GetTypeInfo()
                        .GetDeclaredMethod(nameof(HandleWithBody))
                        .MakeGenericMethod(descriptor.EntityType, descriptor.BodyType, descriptor.ResultType)
                        .Invoke(this, new object[] { request, capability.Value });
                }
                else
                {
                    return await (ValueTask<RestResponse>)
                        typeof(RestRequestHandler).GetTypeInfo()
                        .GetDeclaredMethod(nameof(Handle))
                        .MakeGenericMethod(descriptor.EntityType, descriptor.ResultType)
                        .Invoke(this, new object[] { request, capability.Value });
                }
            }
            catch (Exception ex)
            {
                return RestResult.Error(descriptor.ResultType, ex).ToResponse();
            }
        }
        private async ValueTask<RestResponse> HandleWithBody<T, E, R>(RestRequest request, RestCapability<T> capability)
            where R : class
        {
            var action = (Func<IIdentity<T>, E, ValueTask<RestResult<R>>>)capability.CreateDelegate();
            var body = (E)request.BodyParser(typeof(E));
            var res = await action(request.Address as IIdentity<T>, body);
            return res.ToResponse();
        }
        private async ValueTask<RestResponse> Handle<T, R>(RestRequest request, RestCapability<T> capability)
            where R : class
        {
            var action = (Func<IIdentity<T>, ValueTask<RestResult<R>>>)capability.CreateDelegate();
            var res = await action(request.Address as IIdentity<T>);
            return res.ToResponse();
        }
    }
}
