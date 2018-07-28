using System;
using System.Xml.Linq;
using Ex = System.Linq.Expressions.Expression;
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public partial class XmlSerializer
    {
        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed XmlSerializer for Nullable&lt;T&gt; types.
            /// </summary>
            public class Nullable : Typed<T>
            {
                private readonly Type valueType;
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent"></param>
                public Nullable(XmlSerializer parent) : base(parent)
                {
                    valueType = typeof(T).GetGeneric(typeof(Nullable<>));
                    if (valueType == null)
                        throw new ArgumentException("Generic type should be Nullable<X>", nameof(T));
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);

                private Func<XElement, T> MakeDeserializer()
                {
                    var input = Ex.Parameter(typeof(XElement), "input");

                    var block = Ex.Condition(
                            Ex.MakeBinary(System.Linq.Expressions.ExpressionType.Equal, Ex.Property(input, nameof(XElement.Value)), Ex.Constant("")),
                            Ex.Default(typeof(T)),
                            Ex.New(typeof(T).GetConstructor(new[] { valueType }),
                                Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), new[] { valueType }, input)));
                    return Ex.Lambda<Func<XElement, T>>(block, input).Compile();
                }

                private Func<T, XElement> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var block = Ex.Condition(
                        Ex.Property(input, nameof(Nullable<int>.HasValue)),
                        Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), new[] { valueType }, Ex.Property(input, nameof(Nullable<int>.Value))),
                        Ex.New(typeof(XElement).GetConstructor(new Type[] { typeof(XName) }), Ex.Constant((XName)"nullable")));
                    return Ex.Lambda<Func<T, XElement>>(block, input).Compile();
                }
            }
        }

    }
}
