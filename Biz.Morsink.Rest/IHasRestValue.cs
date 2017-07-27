using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IHasRestValue
    {
        IRestValue RestValue { get; }
    }
    public interface IHasRestValue<T> : IHasRestValue
        where T:class
    {
        new RestValue<T> RestValue { get; }
    }
}
