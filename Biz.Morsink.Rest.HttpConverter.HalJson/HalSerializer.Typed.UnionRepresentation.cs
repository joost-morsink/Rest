using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json.Linq;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {
        public abstract partial class Typed<T>
        {
            public class UnionRep : Typed<T>
            {
                private readonly TypeDescriptor[] typeDescs;
                private readonly Type[] types;

                public UnionRep(HalSerializer parent) : base(parent)
                {
                    if (!UnionRepresentationDescriptorKind.IsOfKind(typeof(T)))
                        throw new ArgumentException("Type is not of kind UnionRepresentation.");
                    types = UnionRepresentation.GetTypeParameters(typeof(T));
                    typeDescs = types.Select(t => Parent.typeDescriptorCreator.GetDescriptor(t)).ToArray();
                }

                public override T Deserialize(HalContext context, JToken token)
                {
                    Type best = null;
                    int score = int.MinValue;
                    foreach (var (type, desc) in types.Zip(typeDescs, (x, y) => (x, y)))
                    {
                        var sc = Score(desc, token);
                        if (sc > score)
                        {
                            best = type;
                            score = sc;
                        }
                    }
                    return (T)(object)UnionRepresentation.FromOptions(types).Create(Parent.Deserialize(best, context, token));
                }

                private int Score(TypeDescriptor desc, JToken token)
                {
                    var score = 0;
                    if (token is JObject jobj)
                    {
                        var props = (desc as TypeDescriptor.Record)?.Properties;
                        if (props != null)
                        {
                            var req = new HashSet<string>(props.Where(p => p.Value.Required).Select(p => p.Key));
                            foreach (var prop in jobj.Properties())
                            {
                                if (props.TryGetValue(prop.Name, out var pdesc))
                                {
                                    if (pdesc.Required)
                                        req.Remove(pdesc.Name);
                                    score += 10;
                                }
                            }
                            if (req.Count == 0)
                                score *= 10;
                            return score;
                        }
                    }
                    return 1;
                }

                public override JToken Serialize(HalContext context, T item)
                {
                    var u = item as UnionRepresentation;
                    return Parent.Serialize(context, u.GetItem());
                }
            }
        }
    }
}
