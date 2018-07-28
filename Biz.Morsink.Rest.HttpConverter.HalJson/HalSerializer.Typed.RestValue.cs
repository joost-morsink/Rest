using System;
using System.Collections.Generic;
using System.Linq;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Utils;
using Newtonsoft.Json.Linq;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {

        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed HalSerializer for Rest Values.
            /// </summary>
            public class RestValue : Typed<T>
            {
                private readonly Type valueType;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public RestValue(HalSerializer parent) : base(parent)
                {
                    if (!typeof(IRestValue).IsAssignableFrom(typeof(T)))
                        throw new ArgumentException("Type is not a RestValue");
                    valueType = typeof(T).GetGeneric(typeof(RestValue<>));
                }
                public override JToken Serialize(HalContext context, T item)
                {
                    var rv = (IRestValue)item;
                    context = context.With(rv);
                    var obj = Parent.Serialize(context, rv.Value);
                    var links = new JObject(from lnk in rv.Links
                                            group lnk by lnk.RelType into g
                                            select new JProperty(g.Key, g.Skip(1).Any()
                                                ? new JArray(g.Select(x => Parent.Serialize(context, x.Target)))
                                                : Parent.Serialize(context, g.First().Target)));
                    obj["_links"] = links;
                    obj["_embedded"] = new JArray(rv.Embeddings.Select(o => Parent.Serialize(o is IHasIdentity hid ? context.Without(hid.Id) : context, o)));
                    return obj;
                }
                public override T Deserialize(HalContext context, JToken token)
                {
                    throw new NotSupportedException();
                    // TODO: Missing deserialization type
                    //if (token is JObject o)
                    //{
                    //    var res = new EmptyRestValue();
                    //    var embedded = o["_embedded"];
                    //    if(embedded!=null && embedded is JArray es)
                    //    {

                    //        res.WithEmbeddings(es.Select(e => Parent.Deserialize()
                    //    }
                    //}
                    //else
                    //    throw new ArgumentException("Token should be object.");
                }
                private class EmptyRestValue : IRestValue
                {
                    public EmptyRestValue()
                        : this(Enumerable.Empty<Link>(), Enumerable.Empty<object>())
                    { }
                    public EmptyRestValue(IEnumerable<Link> links, IEnumerable<object> embeddings)
                    {
                        Links = links.ToArray();
                        Embeddings = embeddings.ToArray();
                    }
                    public object Value => null;

                    public Type ValueType => typeof(object);

                    public IReadOnlyList<Link> Links { get; }

                    public IReadOnlyList<object> Embeddings { get; }

                    public EmptyRestValue Manipulate(Func<IRestValue, IEnumerable<Link>> links = null, Func<IRestValue, IEnumerable<object>> embeddings = null)
                        => new EmptyRestValue(links(this), embeddings(this));
                    IRestValue IRestValue.Manipulate(Func<IRestValue, IEnumerable<Link>> links, Func<IRestValue, IEnumerable<object>> embeddings)
                        => Manipulate(links, embeddings);

                    public IRestValue AssignValue(Type valueType, object value)
                        => (IRestValue)Activator.CreateInstance(typeof(RestValue<>).MakeGenericType(valueType), value, Links, Embeddings);
                }
            }
        }
    }
}
