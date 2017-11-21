using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public abstract class RestAttribute : Attribute
    {
        public abstract string Capability { get; }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestGetAttribute : RestAttribute
    {
        public override string Capability => "GET";
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPutAttribute : RestAttribute
    {
        public override string Capability => "PUT";
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPostAttribute : RestAttribute
    {
        public override string Capability => "POST";
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPatchAttribute : RestAttribute
    {
        public override string Capability => "PATCH";
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class RestDeleteAttribute : RestAttribute
    {
        public override string Capability => "DELETE";
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RestBodyAttribute : Attribute { }

}
