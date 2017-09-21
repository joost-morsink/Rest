using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A builder for a RestRequestHandler
    /// </summary>
    public class RestRequestHandlerBuilder : IRestRequestHandlerBuilder
    {
        /// <summary>
        /// Create a new Builder
        /// </summary>
        /// <returns>A new builder.</returns>
        public static IRestRequestHandlerBuilder Create()
            => new RestRequestHandlerBuilder { handlers = ImmutableList<Func<RestRequestHandlerDelegate, RestRequestHandlerDelegate>>.Empty };
        private ImmutableList<Func<RestRequestHandlerDelegate, RestRequestHandlerDelegate>> handlers;
        private RestRequestHandlerBuilder() { }
        /// <summary>
        /// Inserts a handler into the pipeline.
        /// </summary>
        /// <param name="handler">A RequestHandlerDelegate decoration function. This function serves as a middleware component.</param>
        /// <returns>A builder using the specified decoration function.</returns>
        public IRestRequestHandlerBuilder Use(Func<RestRequestHandlerDelegate, RestRequestHandlerDelegate> handler)
        {
            return new RestRequestHandlerBuilder { handlers = handlers.Add(handler) };
        }
        /// <summary>
        /// Inserts the terminal handler into the pipeline.
        /// </summary>
        /// <param name="handler">A function producing the core RestRequestHandlerDelegate.</param>
        /// <returns>An instance of the IRestRequestHandler interface that uses all specified middleware and the specified terminal handler.</returns>
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
