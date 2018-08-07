using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    partial class XmlSerializer
    {
        partial class Typed<T>
        {
            public class UnionRep : Typed<T>
            {
                private readonly Type[] types;
                private readonly TypeDescriptor[] typeDescs;

                public UnionRep(XmlSerializer parent) : base(parent)
                {
                    if (!UnionRepresentationDescriptorKind.IsOfKind(typeof(T)))
                        throw new ArgumentException("Type is not of kind UnionRepresentation.");
                    types = UnionRepresentation.GetTypeParameters(typeof(T));
                    typeDescs = types.Select(t => Parent.typeDescriptorCreator.GetDescriptor(t)).ToArray();
                }

                public override T Deserialize(XElement e)
                {
                    Type best = null;
                    int score = int.MinValue;
                    foreach (var (type, desc) in types.Zip(typeDescs, (x, y) => (x, y)))
                    {
                        var sc = Score(desc, e);
                        if (sc > score)
                        {
                            best = type;
                            score = sc;
                        }
                    }
                    return (T)(object)UnionRepresentation.FromOptions(types).Create(Parent.Deserialize(e, best));
                }

                private int Score(TypeDescriptor desc, XElement token)
                {
                    var score = 0;
                    var props = (desc as TypeDescriptor.Record)?.Properties;

                    if (props != null)
                    {
                        var req = new HashSet<string>(props.Where(p => p.Value.Required).Select(p => p.Key));
                        foreach(var e in token.Elements())
                        {
                            if(props.TryGetValue(e.Name.LocalName, out var pdesc))
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
                    return 1;
                }

                public override XElement Serialize(T item)
                {
                    var u = item as UnionRepresentation;
                    return Parent.Serialize(u.GetItem());
                }
            }
        }
    }
}
