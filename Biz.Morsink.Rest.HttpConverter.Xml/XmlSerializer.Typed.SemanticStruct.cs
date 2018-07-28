using System;
using System.Linq;
using System.Xml.Linq;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public partial class XmlSerializer
    {
        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed XmlSerializer for semantic structs.
            /// </summary>
            /// <typeparam name="P">The type of the underlying value.</typeparam>
            public class SemanticStruct<P> : Typed<T>
            {
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">The parent serializer.</param>
                public SemanticStruct(XmlSerializer parent) : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                private Func<XElement, T> MakeDeserializer()
                {
                    var ctor = typeof(T).GetConstructor(new[] { typeof(P) });
                    var e = Ex.Parameter(typeof(XElement), "e");
                    var block = Ex.New(ctor, Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), new[] { typeof(P) }, e));
                    var lambda = Ex.Lambda<Func<XElement, T>>(block, e);
                    return lambda.Compile();
                }

                private Func<T, XElement> MakeSerializer()
                {
                    var prop = typeof(T).GetProperties().Where(p => p.PropertyType == typeof(P)).First();
                    var t = Ex.Parameter(typeof(T), "t");
                    var block = Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), new[] { typeof(P) }, Ex.Property(t, prop));
                    var lambda = Ex.Lambda<Func<T, XElement>>(block, t);
                    return lambda.Compile();
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);
                public override XElement Serialize(T item)
                    => serializer(item);
            }
        }

    }
}
