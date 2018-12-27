using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Representation class for RestCapabilities.
    /// </summary>
    public class RestCapabilitiesRepresentation : SimpleTypeRepresentation<RestCapabilities, RestCapabilitiesRepresentation.Representation>
    {
        public override RestCapabilities GetRepresentable(Representation representation)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Representation class for RestCapabilities.
        /// </summary>
        [SFormat(Property = SFormat.Literal)]
        public class Representation : IDictionary<string, RequestDescription[]>
        {
            private readonly RestCapabilities inner;
            /// <summary>
            /// Constructor.
            /// </summary>
            public Representation() { inner = new RestCapabilities(); }
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="inner">The actual RestCapabilities instance.</param>
            public Representation(RestCapabilities inner)
            {
                this.inner = inner;
            }

            /// <summary>
            /// Gets or sets a collection of RequestDescription items for some Rest capability.
            /// </summary>
            /// <param name="key">The name of the Rest capability.</param>
            /// <returns>An array of RequestDescriptions for the specified Rest capability.</returns>
            public RequestDescription[] this[string key] { get => inner[key]; set => inner[key] = value; }

            /// <summary>
            /// Gets all the capability names.
            /// </summary>
            public ICollection<string> Keys => ((IDictionary<string, RequestDescription[]>)inner).Keys;

            /// <summary>
            /// Gets all the RequestDescriptions.
            /// </summary>
            public ICollection<RequestDescription[]> Values => ((IDictionary<string, RequestDescription[]>)inner).Values;

            /// <summary>
            /// Gets the number of distinct capability names.
            /// </summary>
            public int Count => inner.Count;

            /// <summary>
            /// Indicates whether this dictionary is read-only or read-write.
            /// </summary>
            public bool IsReadOnly => ((IDictionary<string, RequestDescription[]>)inner).IsReadOnly;

            /// <summary>
            /// Adds an entry to the dictionary.
            /// </summary>
            /// <param name="key">The Rest capability.</param>
            /// <param name="value">A collection of RequestDescriptions.</param>
            public void Add(string key, RequestDescription[] value)
            {
                inner.Add(key, value);
            }
            /// <summary>
            /// Adds an entry to the dictionary.
            /// </summary>
            /// <param name="item">The key-value pair to add.</param>
            public void Add(KeyValuePair<string, RequestDescription[]> item)
            {
                ((IDictionary<string, RequestDescription[]>)inner).Add(item);
            }
            /// <summary>
            /// Clears the dictionary.
            /// </summary>
            public void Clear()
            {
                inner.Clear();
            }
            /// <summary>
            /// Checks if the dictionary contains the specified key-value pair.
            /// </summary>
            /// <param name="item">The key-value pair to check.</param>
            /// <returns>True if the dictionary contains the key-value pair, false otherwise.</returns>
            public bool Contains(KeyValuePair<string, RequestDescription[]> item)
            {
                return ((IDictionary<string, RequestDescription[]>)inner).Contains(item);
            }
            /// <summary>
            /// Check if the dictionary contains the specified key.
            /// </summary>
            /// <param name="key">The key to check.</param>
            /// <returns>True if the dictionary contains the specified key, false otherwise.</returns>
            public bool ContainsKey(string key)
            {
                return inner.ContainsKey(key);
            }
            /// <summary>
            /// Copies the entries to an array.
            /// </summary>
            /// <param name="array">An array to copy the elements into.</param>
            /// <param name="arrayIndex">A start index for the copy operation.</param>
            public void CopyTo(KeyValuePair<string, RequestDescription[]>[] array, int arrayIndex)
            {
                ((IDictionary<string, RequestDescription[]>)inner).CopyTo(array, arrayIndex);
            }
            /// <summary>
            /// Gets an enumerator for this collection.
            /// </summary>
            /// <returns>An enumerator.</returns>
            public IEnumerator<KeyValuePair<string, RequestDescription[]>> GetEnumerator()
            {
                return ((IDictionary<string, RequestDescription[]>)inner).GetEnumerator();
            }
            /// <summary>
            /// Removes the entry with the specified key.
            /// </summary>
            /// <param name="key">The key to remove.</param>
            /// <returns>True if the item was successfully removed from the dictionary.</returns>
            public bool Remove(string key)
            {
                return inner.Remove(key);
            }
            /// <summary>
            /// Removes the specified entry.
            /// </summary>
            /// <param name="item">The entry to remove.</param>
            /// <returns>True if the item was successfully removed from the dictionary.</returns>
            public bool Remove(KeyValuePair<string, RequestDescription[]> item)
            {
                return ((IDictionary<string, RequestDescription[]>)inner).Remove(item);
            }
            /// <summary>
            /// Tries to get a value belonging to a specified key.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">A value if one could be found for the specified key, null otherwise.</param>
            /// <returns>True if the key could be found, false otherwise.</returns>
            public bool TryGetValue(string key, out RequestDescription[] value)
            {
                return inner.TryGetValue(key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IDictionary<string, RequestDescription[]>)inner).GetEnumerator();
            }
        }
        public override Representation GetRepresentation(RestCapabilities item)
            => new Representation(item);
    }
}
