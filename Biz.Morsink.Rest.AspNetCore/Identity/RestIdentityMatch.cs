using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    public struct RestIdentityMatch
    { 
        public RestIdentityMatch(RestPath.Match match, Type forType, Type[] componentTypes, Type[] wildcardTypes, Version version)
        {
            Match = match;
            ForType = forType;
            ComponentTypes = componentTypes;
            WildcardTypes = wildcardTypes;
            Version = version;
        }
        public bool IsSuccessful => Match.IsSuccessful;
        public RestPath.Match Match { get; }
        public RestPath Path => Match.Path;
        public Type ForType { get; }
        public Type[] ComponentTypes { get; }
        public Type[] WildcardTypes { get; }
        public Version Version { get; }
    }

}
