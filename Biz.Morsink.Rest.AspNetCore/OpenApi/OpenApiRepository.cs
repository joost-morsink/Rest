using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.OpenApi;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// Repository for an OpenAPI Specification version 3.0 document.
    /// </summary>
    class OpenApiRepository
    {
        private readonly IServiceProvider serviceProvider;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        public OpenApiRepository(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Gets the OAS document.
        /// </summary>
        /// <param name="id">Dummy identity.</param>
        /// <returns>An OpenAPI Specification version 3.0 document.</returns>
        [RestGet]
        [RestDocumentation("This Get operation should return an OpenAPI Specification version 3.0. It should be the one you're looking at right now.")]
        public Document Get(IIdentity<Document> id)
        {
            var typeDescriptorCreator = serviceProvider.GetRequiredService<TypeDescriptorCreator>();
            var apidesc = new RestApiDescription(serviceProvider.GetServices<IRestRepository>(),typeDescriptorCreator );
            return Document.Create(apidesc, serviceProvider.GetServices<IRestPathMapping>(), typeDescriptorCreator, serviceProvider.GetRequiredService<IRestIdentityProvider>());
        }
    }
}
