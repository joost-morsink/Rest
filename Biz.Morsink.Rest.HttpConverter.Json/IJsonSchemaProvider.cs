using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    public interface IJsonSchemaProvider
    {
        JsonSchema GetSchema(TypeDescriptor typeDescriptor);
    }
}
