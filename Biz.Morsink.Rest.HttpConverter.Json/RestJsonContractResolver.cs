using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// A Json Contract Resolver for Rest. 
    /// It knows how to serialize some Rest specific types like IIdentity values.
    /// </summary>
    public class RestJsonContractResolver : CamelCasePropertyNamesContractResolver
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider to instantiate dependencies.</param>
        public RestJsonContractResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);
            if (typeof(IIdentity).IsAssignableFrom(objectType))
                contract.Converter = new IdentityConverter(serviceProvider.GetService<IRestIdentityProvider>());
            return contract;
        }
    }
}
