using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This attribute allows documentation to be set to Rest implementation constructs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Property,
        AllowMultiple = true, Inherited = true)]
    public class RestDocumentationAttribute : Attribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="doc">The documentation string.</param>
        /// <param name="format">A media type for the documentation string. Default is text/plain.</param>
        public RestDocumentationAttribute(string doc, string format = "text/plain") : base()
        {
            Documentation = doc;
            Format = format;
        }
        /// <summary>
        /// The documentation string.
        /// </summary>
        public string Documentation { get; set; }
        /// <summary>
        /// The media type for the documentation string.
        /// </summary>
        public string Format { get; set; }
    }
}
