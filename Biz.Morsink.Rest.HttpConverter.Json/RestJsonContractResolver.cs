using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// A Json Contract Resolver for Rest. 
    /// It knows how to serialize some Rest specific types like IIdentity values.
    /// </summary>
    public class RestJsonContractResolver : CamelCasePropertyNamesContractResolver
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<JsonHttpConverterOptions> options;
        private readonly IJsonSchemaTranslator[] translators;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider to instantiate dependencies.</param>
        public RestJsonContractResolver(IEnumerable<IJsonSchemaTranslator> translators)
        {
            this.translators = translators.ToArray();
        }
        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);
            var converter = translators.Select(t => t.GetConverter()).Where(c => c.CanConvert(objectType)).FirstOrDefault();
            if (converter != null)
                contract.Converter = converter;
            return contract;
        }
    }
}
