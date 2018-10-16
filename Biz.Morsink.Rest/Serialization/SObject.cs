using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// An intermediate serialization format for objects.
    /// Objects are containers for properties.
    /// </summary>
    public class SObject : SItem
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="properties">A collection of properties.</param>
        public SObject(params SProperty[] properties) : this((IEnumerable<SProperty>)properties)
        { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="properties">A collection of properties.</param>
        public SObject(IEnumerable<SProperty> properties)
        {
            Properties = properties.ToArray();
        }
        /// <summary>
        /// The properties contained in the object.
        /// </summary>
        public IReadOnlyList<SProperty> Properties { get; }

        /// <summary>
        /// Converts the properties contained in this object to a dictionary.
        /// </summary>
        /// <param name="equalityComparer">Optional equality comparer for the dictionary.</param>
        /// <returns></returns>
        public Dictionary<string, SItem> ToDictionary(IEqualityComparer<string> equalityComparer = null)
            => Properties.ToDictionary(p => p.Name, p => p.Token, equalityComparer ?? EqualityComparer<string>.Default);

        public override int GetHashCode()
            => Properties.Aggregate(0, (acc, p) => acc ^ p.GetHashCode());
        public override bool Equals(SItem other)
            => other is SObject obj && Equals(obj);
        public bool Equals(SObject other)
            => Properties.OrderBy(p => p.Name).SequenceEqual(other.Properties.OrderBy(p => p.Name));
    }
}
