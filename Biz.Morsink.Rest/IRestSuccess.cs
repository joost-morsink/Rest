using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IRestSuccess : IRestResult, IHasRestValue
    {
    }
    public interface IRestSuccess<T> : IRestSuccess, IHasRestValue<T>
        where T : class
    {
    }
}
