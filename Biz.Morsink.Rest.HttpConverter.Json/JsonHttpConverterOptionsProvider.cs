using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// An implementation of the IOptions&lt;T&gt; pattern for JsonHttpConverterOptions.
    /// The main reason for this class is the regular options pattern not supporting dependency injection.
    /// </summary>
    public class JsonHttpConverterOptionsProvider : IOptions<JsonHttpConverterOptions>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contractResolver">An IContractResolver instance.</param>
        /// <param name="configure">A optional function to configure the JsonHttpConverterOptions.</param>
        public JsonHttpConverterOptionsProvider(IContractResolver contractResolver, Func<JsonHttpConverterOptions, JsonHttpConverterOptions> configure)
        {
            var opts = new JsonHttpConverterOptions();
            opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            opts.SerializerSettings.ContractResolver = contractResolver;
            Value = configure == null ? opts : configure(opts);
        }
        /// <summary>
        /// Gets an instance of JsonHttpConverterOptions.
        /// </summary>
        public JsonHttpConverterOptions Value { get; }
        /// <summary>
        /// Gets an instance of the IOptions instance.
        /// </summary>
        /// <returns>'this'</returns>
        public IOptions<JsonHttpConverterOptions> GetOptions() => this;
    }
}
