using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// A property (key value mapping) in intermediate serialization format.
    /// </summary>
    public class SProperty : IEquatable<SProperty>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="token">The value of the property.</param>
        public SProperty(string name, SItem token)
           : this(name, token, SFormat.Default)
        { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="token">The value of the property.</param>
        /// <param name="format">The formatting for the property name.</param>
        public SProperty(string name, SItem token, SFormat format)
        {
            Name = name;
            Token = token;
            Format = format;
        }

        /// <summary>
        /// The property name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The property value.
        /// </summary>
        public SItem Token { get; }
        /// <summary>
        /// The formatting for the property.
        /// </summary>
        public SFormat Format { get; }

        public override int GetHashCode()
            => Name.GetHashCode() ^ Token.GetHashCode();
        public override bool Equals(object obj)
            => obj is SProperty prop && Equals(prop);
        public bool Equals(SProperty other)
            => Name == other.Name && Token.Equals(other.Token);
    }
}
