using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RestPathAttribute : Attribute
    {
        public RestPathAttribute(string path) : this(path, null, null) { }
        public RestPathAttribute(string path, Type[] componentTypes) : this(path, componentTypes, null) { }
        public RestPathAttribute(string path, Type[] componentTypes, Type wildcardType)
        {
            Path = path;
            ComponentTypes = componentTypes;
            WildcardType = wildcardType;
        }
        public string Path { get; set; }
        public Type[] ComponentTypes { get; set; }
        public Type WildcardType { get; set; }
    }
}
