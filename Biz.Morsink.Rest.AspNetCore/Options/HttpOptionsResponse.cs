using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Options
{
    public class HttpOptionsResponse
    {
        public HttpOptionsResponse() { }
        public HttpOptionsResponse(IRestRepository repo)
        {
            var caps = repo.GetCapabilities().ToDictionary(c => c.Name);
            if (caps.TryGetValue("GET", out var cap))
                Get = new RequestDescription { Parameters = cap.ParameterType?.GetDescriptor(), ResponseBody = cap.ResultType?.GetDescriptor() };
            if (caps.TryGetValue("POST", out cap))
                Post = new RequestDescription { Parameters = cap.ParameterType?.GetDescriptor(), RequestBody = cap.BodyType?.GetDescriptor(), ResponseBody = cap.ResultType?.GetDescriptor() };
            if (caps.TryGetValue("PUT", out cap))
                Put = new RequestDescription { Parameters = cap.ParameterType?.GetDescriptor(), RequestBody = cap.BodyType?.GetDescriptor(), ResponseBody = cap.ResultType?.GetDescriptor() };
            if (caps.TryGetValue("DELETE", out cap))
                Delete = new RequestDescription { Parameters = cap.ParameterType?.GetDescriptor(), RequestBody = cap.BodyType?.GetDescriptor(), ResponseBody = cap.ResultType?.GetDescriptor() };
        }
        public RequestDescription Get { get; set; }
        public RequestDescription Post { get; set; }
        public RequestDescription Put { get; set; }
        public RequestDescription Delete { get; set; }
    }
    public class RequestDescription
    {
        public TypeDescriptor RequestBody { get; set; }
        public TypeDescriptor Parameters { get; set; }
        public TypeDescriptor ResponseBody { get; set; }
    }
}
