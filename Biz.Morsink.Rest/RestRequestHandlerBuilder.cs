using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public class RestRequestHandlerBuilder : IRestRequestHandlerBuilder
    {
        public static IRestRequestHandlerBuilder Create()
            => new RestRequestHandlerBuilder { handlers = ImmutableList<Func<RestRequestHandlerDelegate, RestRequestHandlerDelegate>>.Empty };
        private ImmutableList<Func<RestRequestHandlerDelegate, RestRequestHandlerDelegate>> handlers;
        private RestRequestHandlerBuilder() { } 
        
        public IRestRequestHandlerBuilder Use(Func<RestRequestHandlerDelegate, RestRequestHandlerDelegate> handler)
        {
            return new RestRequestHandlerBuilder { handlers = handlers.Add(handler) };
        }
        public IRestRequestHandler Run(Func<RestRequestHandlerDelegate> handler)
        {
            var result = handler();
            foreach (var h in handlers.Reverse())
                result = h(result);
            return new Handler(result);
        }
        private class Handler : IRestRequestHandler
        {
            private readonly RestRequestHandlerDelegate handler;

            public Handler(RestRequestHandlerDelegate handler)
            {
                this.handler = handler;
            }

            public ValueTask<RestResponse> HandleRequest(RestRequest request)
                => handler(request);
        }
    }
}
