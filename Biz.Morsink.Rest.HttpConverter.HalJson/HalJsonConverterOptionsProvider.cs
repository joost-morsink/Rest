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
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        /// <param name="configure">An optional configuration function.</param>
        public HalJsonConverterOptionsProvider(IServiceProvider serviceProvider, Func<HalJsonConverterOptions, HalJsonConverterOptions> configure)
        {
            var opts = new HalJsonConverterOptions();
            opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            Value = configure == null ? opts : configure(opts);
        }
        /// <summary>
        /// Contains the options value.
        /// </summary>
        public HalJsonConverterOptions Value { get; }
        /// <summary>
        /// Returns 'this'. 
        /// </summary>
        public IOptions<HalJsonConverterOptions> GetOptions() => this;
    }
}
