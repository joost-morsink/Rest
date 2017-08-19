using System;
using System.Collections.Immutable;

namespace Biz.Morsink.Rest
{
    public class RestRequestHandlerBuilder : IRestRequestHandlerBuilder
    {
        public static IRestRequestHandlerBuilder Create()
            => new RestRequestHandlerBuilder { handlers = ImmutableList<Func<IRestRequestHandler, IRestRequestHandler>>.Empty };
        private ImmutableList<Func<IRestRequestHandler, IRestRequestHandler>> handlers;
        private RestRequestHandlerBuilder() { } 
        
        public IRestRequestHandlerBuilder Use(Func<IRestRequestHandler, IRestRequestHandler> handler)
        {
            return new RestRequestHandlerBuilder { handlers = handlers.Add(handler) };
        }
        public IRestRequestHandler Run(Func<IRestRequestHandler> handler)
        {
            var result = handler();
            foreach (var h in handlers.Reverse())
                result = h(result);
            return result;
        }
    }
}
