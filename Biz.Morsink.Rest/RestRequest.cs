using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Biz.Morsink.Rest
{
    public class RestRequest
    {
        public class Parameters
        {
            private KeyValuePair<string, string>[] parameters;
            private ILookup<string, string> lookup;
            private IReadOnlyDictionary<string, string> firstDict;
            public Parameters(IEnumerable<KeyValuePair<string, string>> parameters)
            {
                this.parameters = parameters.ToArray();
            }
            public Parameters(IEnumerable<(string, string)> parameters)
                : this(parameters.Select(x => new KeyValuePair<string, string>(x.Item1, x.Item2)))
            { }
            public ILookup<string, string> AsLookup()
                => lookup = lookup ?? parameters.ToLookup(p => p.Key, p => p.Value);
            public IReadOnlyDictionary<string, string> AsDictionary()
                => firstDict = firstDict ?? parameters.GroupBy(p => p.Key).ToImmutableDictionary(p => p.Key, p => p.First().Value);
        }

        public RestRequest(string capability, IIdentity address, IEnumerable<KeyValuePair<string, string>> parameters, Func<Type, object> bodyParser)
        {
            Capability = capability;
            Address = address;
            RequestParameters = new Parameters(parameters);
            BodyParser = bodyParser;
        }
        public RestRequest(string capability, IIdentity address, IEnumerable<(string, string)> parameters, Func<Type, object> bodyParser)
        {
            Capability = capability;
            Address = address;
            RequestParameters = new Parameters(parameters);
            BodyParser = bodyParser;
        }

        public string Capability { get; }
        public IIdentity Address { get; }
        public Func<Type, object> BodyParser { get; }
        public Parameters RequestParameters { get; }
    }
}