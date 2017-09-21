using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface for successful Rest results.
    /// A successful result has a value, indicated by deriving from the IHasRestValue interface.
    /// </summary>
    public interface IRestSuccess : IRestResult, IHasRestValue
    {
    }
    /// <summary>
    /// Generic interface for successful Rest results.
    /// A successful result has a value of type T, indicated by deriving from the IHasRestValue&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IRestSuccess<T> : IRestSuccess, IHasRestValue<T>
        where T : class
    {
    }
}
