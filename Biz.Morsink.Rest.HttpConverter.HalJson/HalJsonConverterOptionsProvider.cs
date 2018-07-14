using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    /// <summary>
    /// Options provider pattern implementation for the Hal Json Http converter component.
    /// </summary>
    public class HalJsonConverterOptionsProvider : IOptions<HalJsonConverterOptions>
    {
        private readonly Lazy<HalJsonConverterOptions> options;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        /// <param name="configure">An optional configuration function.</param>
        public HalJsonConverterOptionsProvider(IServiceProvider serviceProvider, Func<HalJsonConverterOptions, HalJsonConverterOptions> configure)
        {
            options = new Lazy<HalJsonConverterOptions>(() =>
            {
                var opts = new HalJsonConverterOptions();
                opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opts.SerializerSettings.ContractResolver = serviceProvider.GetRequiredService<IContractResolver>();
                return configure == null ? opts : configure(opts);
            });
        }
        /// <summary>
        /// Contains the options value.
        /// </summary>
        public HalJsonConverterOptions Value => options.Value;
        /// <summary>
        /// Returns 'this'. 
        /// </summary>
        public IOptions<HalJsonConverterOptions> GetOptions() => this;
    }
}
