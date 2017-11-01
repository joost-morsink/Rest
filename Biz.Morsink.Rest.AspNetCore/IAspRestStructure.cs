using Biz.Morsink.Rest.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    public interface IAspRestStructure
    {
        Type RootType { get; }
        IEnumerable<(Type,Func<object, IRestRepository>)> Repositories { get; }
        IEnumerable<IRestPathMapping> PathMappings { get; }
    }
}
