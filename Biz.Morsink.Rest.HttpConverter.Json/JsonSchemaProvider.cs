using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Implementation for the IJsonSchemaProvider interface.
    /// An IServiceProvider is needed to construct this class to break circular dependencies.
    /// </summary>
    public class JsonSchemaProvider : IJsonSchemaProvider
    {
        private readonly Lazy<IEnumerable<IJsonSchemaTranslator>> translators;
        private readonly TypeDescriptorCreator typeDescriptorCreator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider for dependencies.</param>
        public JsonSchemaProvider(IServiceProvider serviceProvider)
        {
            translators = new Lazy<IEnumerable<IJsonSchemaTranslator>>(() => serviceProvider.GetServices<IJsonSchemaTranslator>());
            typeDescriptorCreator = serviceProvider.GetService<TypeDescriptorCreator>();
        }
        /// <summary>
        /// This method should return the corresponding JsonSchema object for some TypeDescriptor.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor to get a schema for.</param>
        /// <returns>A JsonSchema object that corresponds to the given TypeDescriptor.</returns>
        public JsonSchema GetSchema(Type type)
        {
            var specific = translators.Value.Select(tr => tr.GetSchema(type)).Where(sch => sch != null).FirstOrDefault();
            if (specific == null)
            {
                var visitor = new JsonSchemaTypeDescriptorVisitor(typeDescriptorCreator);
                var schema = visitor.Transform(typeDescriptorCreator.GetDescriptor(type));
                return new JsonSchema(schema);
            }
            else
                return specific;
        }
    }
}
