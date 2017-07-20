using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface ILinkProvider<T>
    {
        IReadOnlyList<Link> GetLinks(IIdentity<T> id);
    }
    public interface IDynamicLinkProvider<T>
    {
        IReadOnlyList<Link> GetLinks(T id);
    }
}
