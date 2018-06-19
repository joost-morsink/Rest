using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public class HalJsonConverterOptionsProvider : IOptions<HalJsonConverterOptions>
    {
        private readonly Lazy<HalJsonConverterOptions> options;
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
        public HalJsonConverterOptions Value => options.Value;
        public IOptions<HalJsonConverterOptions> GetOptions() => this;
    }
}
