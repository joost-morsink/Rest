using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest.HttpConverter.Json.FSharp
{
    /// <summary>
    /// A JsonConverter for the F# option type.
    /// </summary>
    public class FSharpOptionConverter : JsonConverter
    {
        private static readonly string Microsoft_FSharp_Core = nameof(Microsoft_FSharp_Core).Replace('_', '.');
        private static readonly string FSharpOption_1 = nameof(FSharpOption_1).Replace('_', '`');
        private const string Value = nameof(Value);
        private const string Some = nameof(Some);
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="optionType">An F# 'a option type.</param>
        public FSharpOptionConverter(Type optionType)
        {
            this.optionType = optionType;
            innerType = GetInnerType(optionType);
            if (innerType == null)
                throw new ArgumentException("Supplied argument is not an F# option type.");
            getValue = MakeGetValue(optionType);
            makeValue = MakeMakeValue(optionType);
        }
        /// <summary>
        /// Checks whether the type is an F# 'a option type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an F# 'a option type.</returns>
        public static bool IsFSharpOptionType(Type type)
            => GetInnerType(type) != null;
        /// <summary>
        /// Gets the generic parameter of the 'a option type.
        /// </summary>
        /// <param name="type">An F# 'a option type.</param>
        /// <returns>The generic parameter if the type is an option type, or null otherwise.</returns>
        public static Type GetInnerType(Type type)
        {
            var ga = type.GetGenericArguments();
            return ga.Length == 1 && type.Namespace == Microsoft_FSharp_Core && type.Name == FSharpOption_1 ? ga[0] : null;
        }
        /// <summary>
        /// Creates a function that reads the value from an option.
        /// </summary>
        /// <param name="type">The F# 'a option type.</param>
        /// <returns>A function that reads the value from an option.</returns>
        public static Func<object, object> MakeGetValue(Type type)
        {
            var o = Ex.Parameter(typeof(object), "o");
            var t = Ex.Parameter(type, "t");
            var block = Ex.Block(new[] { t },
                Ex.Assign(t, Ex.Convert(o, type)),
                Ex.Convert(Ex.Property(t, Value), typeof(object)));
            var lambda = Ex.Lambda<Func<object, object>>(block, o);
            return lambda.Compile();
        }
        /// <summary>
        /// Creates a function that makes an option instance from a value.
        /// </summary>
        /// <param name="type">The F# 'a option type.</param>
        /// <returns>A function that makes an option instance from a value.</returns>
        public static Func<object,object> MakeMakeValue(Type type)
        {
            var o = Ex.Parameter(typeof(object), "o");
            var m = type.GetMethod(Some, BindingFlags.Static | BindingFlags.Public);
            var t = Ex.Parameter(m.GetParameters()[0].ParameterType, "t");
            var block = Ex.Block(new[] { t },
                Ex.Assign(t, Ex.Convert(o, t.Type)),
                Ex.Convert(Ex.Call(m, t), typeof(object)));
            var lambda = Ex.Lambda<Func<object, object>>(block, o);
            return lambda.Compile();
        }

        private readonly Type optionType;
        private readonly Type innerType;
        private readonly Func<object, object> getValue;
        private readonly Func<object, object> makeValue;

        public override bool CanConvert(Type objectType)
            => objectType == optionType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            else
                return makeValue(serializer.Deserialize(reader, innerType));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                serializer.Serialize(writer, getValue(value));
        }
    }
}
