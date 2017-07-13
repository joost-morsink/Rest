using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IRestSuccess 
    {
    }
    public interface IRestSuccess<T> : IRestSuccess, IHasRestValue<T>
    {
    }
}
