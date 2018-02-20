using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// Static class containing readonly/const members to be used for Xml Schema and instances (for nil)
    /// </summary>
    public static class XsdConstants
    {
        /// <summary>
        /// The Xml schema namespace.
        /// </summary>
        public static readonly XNamespace XSD = "http://www.w3.org/2001/XMLSchema";
        /// <summary>
        /// The Xml schema instance namespace.
        /// </summary>
        public static readonly XNamespace XSI = "http://www.w3.org/2001/XMLSchema-instance";
        public const string schema = nameof(schema);
        public const string schemaLocation = nameof(schemaLocation);
        public const string include = nameof(include);
        public const string simpleType = nameof(simpleType);
        public const string complexType = nameof(complexType);
        public const string choice = nameof(choice);
        public const string sequence = nameof(sequence);
        public const string all = nameof(all);
        public const string element = nameof(element);
        public const string restriction = nameof(restriction);
        public const string enumeration = nameof(enumeration);
        public const string type = nameof(type);
        public const string name = nameof(name);
        public const string @base = nameof(@base);
        public const string minOccurs = nameof(minOccurs);
        public const string maxOccurs = nameof(maxOccurs);
        public const string nil = nameof(nil);
        public const string nillable = nameof(nillable);
        public const string value = nameof(value);
        public const string boolean = xs + ":" + nameof(boolean);
        public const string @string = xs + ":" + nameof(@string);
        public const string integer = xs + ":" + nameof(integer);
        public const string dateTime = xs + ":" + nameof(dateTime);
        public const string @decimal = xs + ":" + nameof(@decimal);
        public const string any = xs + ":" + nameof(any);
        public const string xs = nameof(xs);
        public const string xsi = nameof(xsi);
    }
}
