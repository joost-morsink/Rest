using Biz.Morsink.Rest.AspNetCore.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Implementation of the IRestRequestScopeAccessor interface based on Asp.Net Core's HttpContext.
    /// </summary>
    public class AspNetCoreRestRequestScopeAccessor : IRestRequestScopeAccessor
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContextAccessor">An IHttpContextAccessor instance.</param>
        public AspNetCoreRestRequestScopeAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Gets the current Rest request scope.
        /// </summary>
        public IRestRequestScope Scope
        {
            get
            {
                var context = httpContextAccessor.HttpContext;
                RequestScope scope;
                if (context.TryGetContextItem<RequestScope>(out scope))
                    return scope;
                scope = new RequestScope(context);
                context.SetContextItem(scope);
                return scope;
            }
        }

        private class RequestScope : IRestRequestScope
        {
            private readonly HttpContext context;

            public RequestScope(HttpContext context)
            {
                this.context = context;
            }
            public void SetScopeItem<T>(T item)
            {
                context.Items[typeof(T)] = item;
            }

            public bool TryGetScopeItem<T>(out T result)
            {
                if (context.Items.TryGetValue(typeof(T), out var x))
                {
                    result = (T)x;
                    return true;
                }
                else
                {
                    result = default;
                    return false;
                }
            }
            public bool TryRemoveScopeItem<T>(out T result)
            {
                if (context.Items.TryGetValue(typeof(T), out var res))
                {
                    result = (T)res;
                    context.Items.Remove(typeof(T));
                    return true;
                }
                else
                {
                    result = default;
                    return false;
                }
            }
        }
    }
}
