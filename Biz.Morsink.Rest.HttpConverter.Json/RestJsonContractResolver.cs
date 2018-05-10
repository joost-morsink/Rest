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
    public class RestJsonContractResolver : DefaultContractResolver
    {
        private readonly IJsonSchemaTranslator[] translators;
        private readonly ITypeRepresentation[] typeRepresentations;
        private readonly IOptions<JsonHttpConverterOptions> options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider to instantiate dependencies.</param>
        public RestJsonContractResolver(IEnumerable<IJsonSchemaTranslator> translators, IEnumerable<ITypeRepresentation> typeRepresentations, IOptions<JsonHttpConverterOptions> options)
        {
            this.translators = translators.ToArray();
            this.typeRepresentations = typeRepresentations.ToArray();
            this.options = options;
        }
        private void Initialize()
        {
            var opts = options.Value;
            var ns = opts.NamingStrategy;
            if (ns != null)
                NamingStrategy = ns;
        }
        protected override JsonContract CreateContract(Type objectType)
        {
            Initialize();
            var contract = base.CreateContract(objectType);
            foreach (var typeRep in typeRepresentations.Where(tr => tr.IsRepresentable(objectType)))
                contract.Converter = new TypeRepresentationConverter(objectType, typeRep);

            foreach (var converter in translators.Select(t => t.GetConverter()).Where(c => c != null && c.CanConvert(objectType)).Take(1))
                contract.Converter = converter;

            if (options.Value.FSharpSupport)
            {
                if (FSharp.FSharpUnionConverter.IsFSharpUnionType(objectType))
                    contract.Converter = new FSharp.FSharpUnionConverter(objectType);
                if (FSharp.FSharpOptionConverter.IsFSharpOptionType(objectType))
                    contract.Converter = new FSharp.FSharpOptionConverter(objectType);
            }
            return contract;
        }
    }
}
