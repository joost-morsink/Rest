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

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class OpenApiRepository
    {
        private readonly IServiceProvider serviceProvider;

        public OpenApiRepository(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        [RestGet]
        public Document Get(IIdentity<Document> id)
        {
            var typeDescriptorCreator = serviceProvider.GetRequiredService<TypeDescriptorCreator>();
            var apidesc = new RestApiDescription(serviceProvider.GetServices<IRestRepository>(),typeDescriptorCreator );
            return Document.Create(apidesc, serviceProvider.GetServices<IRestPathMapping>(), typeDescriptorCreator, serviceProvider.GetRequiredService<IRestIdentityProvider>());
        }
        public class Structure : IRestStructure
        {
            void IRestStructure.RegisterComponents(IServiceCollection serviceCollection, ServiceLifetime lifetime)
            {
                serviceCollection.AddAttributedRestRepository<OpenApiRepository>(lifetime)
                    .AddRestPathMapping<Document>("/openapi+");
            }
        }
    }
}
