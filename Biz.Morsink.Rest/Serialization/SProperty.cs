using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    public class SProperty : IEquatable<SProperty>
    {
        public SProperty(string name, SItem token)
           : this(name, token, SFormat.Default)
        { }
        public SProperty(string name, SItem token, SFormat format)
        {
            Name = name;
            Token = token;
            Format = format;
        }

        public string Name { get; }
        public SItem Token { get; }
        public SFormat Format { get; }

        public override int GetHashCode()
            => Name.GetHashCode() ^ Token.GetHashCode();
        public override bool Equals(object obj)
            => obj is SProperty prop && Equals(prop);
        public bool Equals(SProperty other)
            => Name == other.Name && Token.Equals(other.Token);
    }
}
