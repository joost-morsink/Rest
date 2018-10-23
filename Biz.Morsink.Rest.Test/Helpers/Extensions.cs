using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Test.Helpers
{
    public static class Extensions
    {
        public static IEnumerable<SValidation.Message> Validate(this SItem item, TypeDescriptor typeDescriptor)
        {
            return item.Validate(typeDescriptor, null, DataConvert.DataConverter.Default);
        }
    }
}
