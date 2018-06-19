using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.Rest.HttpConverter.Xml.XsdConstants;
namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// Utility methods
    /// </summary>
    static class Utils
    {
        public static object GetContent(this XElement element)
            => element == null
                ? null
                : element.HasElements
                    ? (object)element.Elements()
                    : element.Value;
        public static object GetContentOrNil(this XElement element)
                    => element == null
                        ? (object)new XAttribute(XSI + nil, true)
                        : element.HasElements
                            ? (object)element.Elements()
                            : element.Value;
    }
}
