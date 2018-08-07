using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    public class SProperty
    {
        public SProperty(string name, SItem token)
        {
            Name = name;
            Token = token;
        }

        public string Name { get; }
        public SItem Token { get; }
    }
}
