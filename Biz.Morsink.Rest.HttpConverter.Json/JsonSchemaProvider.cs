using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    public class JsonSchemaProvider : IJsonSchemaProvider
    {
        private readonly Lazy<IEnumerable<IJsonSchemaTranslator>> translators;
        private readonly TypeDescriptorCreator typeDescriptorCreator;

        public JsonSchemaProvider(IServiceProvider serviceProvider)
        {
            translators = new Lazy<IEnumerable<IJsonSchemaTranslator>>(() => serviceProvider.GetServices<IJsonSchemaTranslator>());
            typeDescriptorCreator = serviceProvider.GetService<TypeDescriptorCreator>();
        }

        public JsonSchema GetSchema(TypeDescriptor typeDescriptor)
        {
            var specific = translators.Value.FirstOrDefault(t => typeDescriptorCreator.GetDescriptor(t.ForType)?.Equals(typeDescriptor) == true);
            if (specific == null)
            {
                var visitor = new JsonSchemaTypeDescriptorVisitor(typeDescriptorCreator);
                var schema = visitor.Transform(typeDescriptor);
                return new JsonSchema(schema);
            }
            else
                return specific.GetSchema();
        }
    }
}
