using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    public class RestPrefix
    {
        public RestPrefix(string prefix, string abbreviation)
        {
            Prefix = prefix;
            Abbreviation = abbreviation;
        }
        public string Prefix { get; }
        public string Abbreviation { get; }
    }

}
