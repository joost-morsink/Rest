using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IRestRedirect
    {
        RestRedirectType Type { get; }
        IIdentity Target { get; }
    }
}
