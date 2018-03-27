using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A test repository that will throw an exception on every GET request.
    /// </summary>
    public class ExceptionRepository
    {
        /// <summary>
        /// The GET request handler.
        /// </summary>
        /// <param name="ex">A dummy exception identity value.</param>
        /// <returns>Does not return: throws an exception.</returns>
        [RestGet]
        public Task<Exception> Get(IIdentity<Exception> ex)
        {
            throw new Exception("Test-exception");
        }
        /// <summary>
        /// The ExceptionRepository's Structure object.
        /// </summary>
        public class Structure : IRestStructure
        {
            void IRestStructure.RegisterComponents(IServiceCollection serviceCollection, ServiceLifetime lifetime)
            {
                serviceCollection.AddAttributedRestRepository<ExceptionRepository>(lifetime)
                    .AddRestPathMapping<Exception>("/exception?*");
            }
        }
    }
}
