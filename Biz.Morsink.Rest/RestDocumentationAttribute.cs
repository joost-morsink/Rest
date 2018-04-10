using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Property,
        AllowMultiple = true, Inherited = true)]
    public class RestDocumentationAttribute : Attribute
    {
        public RestDocumentationAttribute(string doc, string format = "text/plain") : base()
        {
            Documentation = doc;
            Format = format;
        }
        public string Documentation { get; set; }
        public string Format { get; set; }
    }
}
