using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IServiceLocator
    {
        object ResolveRequired(Type t);
        object ResolveOptional(Type t);
        IEnumerable<object> ResolveMulti(Type t);
    }
}
