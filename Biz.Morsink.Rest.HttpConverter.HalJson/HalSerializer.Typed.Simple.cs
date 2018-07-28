using Biz.Morsink.DataConvert;
using Newtonsoft.Json.Linq;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {

        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed HalSerializer for simple (primitive) types.
            /// </summary>
            public class Simple : Typed<T>
            {
                private readonly IDataConverter converter;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                /// <param name="converter">A DataConverter for simple conversions.</param>
                public Simple(HalSerializer parent, IDataConverter converter)
                    : base(parent)
                {
                    this.converter = converter;
                }

                public override T Deserialize(HalContext context, JToken token)
                    => Parent.converter.Convert((token as JValue)?.Value).To<T>();

                public override JToken Serialize(HalContext context, T item)
                    => Parent.converter.Convert(item).To<string>();
            }
        }
    }
}
