using Biz.Morsink.DataConvert;
using Newtonsoft.Json.Linq;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {
        /// <summary>
        /// Typed HalSerializer for DateTime.
        /// </summary>
        public class DateTime : Typed<System.DateTime>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parent">A reference to the parent HalSerializer instance.</param>
            public DateTime(HalSerializer parent) : base(parent)
            {
            }
            public override JToken Serialize(HalContext context, System.DateTime item)
            {
                return new JValue(Parent.converter.Convert(item).To<string>());
            }
            public override System.DateTime Deserialize(HalContext context, JToken token)
            {
                return Parent.converter.Convert((token as JValue)?.Value).To<System.DateTime>();
            }
        }
    }
}
