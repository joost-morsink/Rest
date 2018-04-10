using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public abstract class RestMetaDataAttribute : Attribute 
    {
        public RestMetaDataAttribute(Type type)
        {
            Type = type;
        }
        public Type Type { get; }
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class RestMetaDataInAttribute : RestMetaDataAttribute
    {
        public RestMetaDataInAttribute(Type type) : base(type) { }
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class RestMetaDataOutAttribute : RestMetaDataAttribute
    {
        public RestMetaDataOutAttribute(Type type) : base(type) { }
    }
}
