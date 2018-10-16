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
        [SFormat(Property = SFormat.Literal)]
        public class Representation : IDictionary<string, RequestDescription[]> {
            private readonly RestCapabilities inner;
            public Representation() { inner = new RestCapabilities(); }
            public Representation(RestCapabilities inner)
            {
                this.inner = inner;
            }

            public RequestDescription[] this[string key] { get => inner[key]; set => inner[key] = value; }

            public ICollection<string> Keys => ((IDictionary<string, RequestDescription[]>)inner).Keys;

            public ICollection<RequestDescription[]> Values => ((IDictionary<string, RequestDescription[]>)inner).Values;

            public int Count => inner.Count;

            public bool IsReadOnly => ((IDictionary<string, RequestDescription[]>)inner).IsReadOnly;

            public void Add(string key, RequestDescription[] value)
            {
                inner.Add(key, value);
            }

            public void Add(KeyValuePair<string, RequestDescription[]> item)
            {
                ((IDictionary<string, RequestDescription[]>)inner).Add(item);
            }

            public void Clear()
            {
                inner.Clear();
            }

            public bool Contains(KeyValuePair<string, RequestDescription[]> item)
            {
                return ((IDictionary<string, RequestDescription[]>)inner).Contains(item);
            }

            public bool ContainsKey(string key)
            {
                return inner.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<string, RequestDescription[]>[] array, int arrayIndex)
            {
                ((IDictionary<string, RequestDescription[]>)inner).CopyTo(array, arrayIndex);
            }

            public IEnumerator<KeyValuePair<string, RequestDescription[]>> GetEnumerator()
            {
                return ((IDictionary<string, RequestDescription[]>)inner).GetEnumerator();
            }

            public bool Remove(string key)
            {
                return inner.Remove(key);
            }

            public bool Remove(KeyValuePair<string, RequestDescription[]> item)
            {
                return ((IDictionary<string, RequestDescription[]>)inner).Remove(item);
            }

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
