using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Base and marker interface for rest capability interfaces.
    /// </summary>
    /// <typeparam name="T">The type of resource the Rest capability applies to.</typeparam>
    public interface IRestCapability<T>
    {
    }
}
