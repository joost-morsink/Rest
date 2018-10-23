using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity.PathProvider;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// Validation of an SItem using a TypeDescriptor.
    /// </summary>
    public static class SValidation
    {
        /// <summary>
        /// An enum for all error types.
        /// </summary>
        public enum Error
        {
            ArrayExpected,
            ValueExpected,
            ObjectExpected,
            StringValueExpected,
            DateExpected,
            BooleanExpected,
            NumericValueExpected,
            RequiredPropertyMissing,
            NoOptionMatch,
            NullExpected,
            IncorrectValue,
            UnknownRef
        }
        /// <summary>
        /// Validation error message class.
        /// </summary>
        public class Message
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="path">The path to the erroneous value.</param>
            /// <param name="error">The error.</param>
            public Message(string path, Error error)
            {
                Path = path;
                Error = error;
            }
            /// <summary>
            /// Contains the path to the erroneous value.
            /// </summary>
            public string Path { get; set; }
            /// <summary>
            /// Contains the error.
            /// </summary>
            public Error Error { get; set; }

        }
        /// <summary>
        /// Validates an SItem according to a TypeDescriptor.
        /// </summary>
        /// <param name="item">The item to validate.</param>
        /// <param name="typeDescriptor">The type descriptor to use for validation.</param>
        /// <param name="typeDescriptorCreator">A type descriptor creator for resolving references to other type descriptor.</param>
        /// <param name="converter">A converter for checking the validity of SValues.</param>
        /// <param name="path">The path to the current value.</param>
        /// <returns>A collection of error messages. The item is valid if no error messages are returned.</returns>
        public static IEnumerable<Message> Validate(this SItem item, TypeDescriptor typeDescriptor, ITypeDescriptorCreator typeDescriptorCreator, IDataConverter converter, string path = null)
        {
            switch (typeDescriptor)
            {
                case TypeDescriptor.Any _:
                    yield break;
                case TypeDescriptor.Array arr:
                    if (item is SArray sarr)
                    {
                        foreach (var msg in sarr.Content.SelectMany((i, idx) => i.Validate(arr.ElementType, typeDescriptorCreator, converter, $"{path}[{idx}]")))
                            yield return msg;
                    }
                    else
                        yield return new Message(path, Error.ArrayExpected);
                    break;
                case TypeDescriptor.Primitive.Numeric _:
                    if (item is SValue val)
                    {
                        if (val.Value == null || !val.Value.GetType().IsNumeric() && !converter.Convert(val.Value).TryTo(out decimal _))
                            yield return new Message(path, Error.NumericValueExpected);
                    }
                    else
                        yield return new Message(path, Error.ValueExpected);
                    break;
                case TypeDescriptor.Primitive.String _:
                    if (item is SValue val2)
                    {
                        if (val2.Value == null || !converter.Convert(val2.Value).TryTo(out string _))
                            yield return new Message(path, Error.StringValueExpected);
                    }
                    else
                        yield return new Message(path, Error.ValueExpected);
                    break;
                case TypeDescriptor.Primitive.DateTime _:
                    if (item is SValue val3)
                    {
                        if (val3.Value == null || !converter.Convert(val3.Value).TryTo(out DateTime _))
                            yield return new Message(path, Error.DateExpected);
                    }
                    else
                        yield return new Message(path, Error.ValueExpected);
                    break;
                case TypeDescriptor.Primitive.Boolean _:
                    if (item is SValue val4)
                    {
                        if (val4.Value == null || !converter.Convert(val4.Value).TryTo(out bool _))
                            yield return new Message(path, Error.BooleanExpected);
                    }
                    else
                        yield return new Message(path, Error.ValueExpected);
                    break;
                case TypeDescriptor.Dictionary dict:
                    if (item is SObject obj)
                    {
                        var props = obj.ToDictionary();
                        foreach (var msg in props.SelectMany(p => p.Value.Validate(dict.ValueType, typeDescriptorCreator, converter, AddToPrefix(path, p.Key))))
                            yield return msg;
                    }
                    else
                        yield return new Message(path, Error.ObjectExpected);
                    break;
                case TypeDescriptor.Record rec:
                    if (item is SObject obj2)
                    {
                        var props = obj2.Properties.ToDictionary(p => p.Name, p => p.Token, CaseInsensitiveEqualityComparer.Instance);
                        foreach (var msg in rec.Properties
                            .SelectMany(p => validateProperty(p.Key, p.Value, props)))

                            yield return msg;
                    }
                    else
                        yield return new Message(path, Error.ObjectExpected);
                    break;
                case TypeDescriptor.Intersection inter:
                    foreach (var msg in inter.Parts.SelectMany(part => item.Validate(part, typeDescriptorCreator, converter, path)))
                        yield return msg;
                    break;
                case TypeDescriptor.Union union:
                    var opts = union.Options.Select(opt => item.Validate(opt, typeDescriptorCreator, converter, path).ToArray()).ToArray();
                    if (opts.Any(opt => opt.Length == 0))
                        yield break;
                    yield return new Message(path, Error.NoOptionMatch);
                    foreach (var msg in opts.SelectMany(opt => opt))
                        yield return msg;
                    break;
                case TypeDescriptor.Null _:
                    if (item is SValue val5)
                    {
                        if (val5.Value != null)
                            yield return new Message(path, Error.NullExpected);
                    }
                    else
                        yield return new Message(path, Error.ValueExpected);
                    break;
                case TypeDescriptor.Value value:
                    if (item is SValue val6)
                    {
                        var conversionResult = converter.DoGeneralConversion(val6.Value, value.InnerValue.GetType());
                        if (!conversionResult.IsSuccessful || !Equals(conversionResult.Result, val6.Value))
                            yield return new Message(path, Error.IncorrectValue);
                    }
                    else
                        yield return new Message(path, Error.ValueExpected);
                    break;
                case TypeDescriptor.Reference refer:
                    var td = typeDescriptorCreator.GetDescriptorByName(refer.RefName);
                    if (td == null)
                        yield return new Message(path, Error.UnknownRef);
                    else
                        foreach (var msg in item.Validate(td, typeDescriptorCreator, converter, path))
                            yield return msg;
                    break;
                case TypeDescriptor.Referable refer:

                    td = refer.ExpandedDescriptor ?? typeDescriptorCreator.GetDescriptorByName(refer.RefName);
                    if (td == null)
                        yield return new Message(path, Error.UnknownRef);
                    else
                        foreach (var msg in item.Validate(td, typeDescriptorCreator, converter, path))
                            yield return msg;
                    break;
            }
            IEnumerable<Message> validateProperty(string propName, PropertyDescriptor<TypeDescriptor> propDesc, Dictionary<string, SItem> dict)
            {
                if (!dict.TryGetValue(propName, out var propValue) || propValue is SValue itemValue && itemValue.Value == null)
                {
                    if (propDesc.Required)
                        yield return new Message(AddToPrefix(path, propName), Error.RequiredPropertyMissing);
                }
                else
                {
                    foreach (var msg in propValue.Validate(propDesc.Type, typeDescriptorCreator, converter, AddToPrefix(path, propName)))
                        yield return msg;
                }
            }
        }

        private static string AddToPrefix(string prefix, string p)
        {
            return prefix == null ? p : $"{prefix}.{p}";
        }

        private static bool IsNumeric(this Type type)
            => new[] { typeof(int),typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(sbyte), typeof(byte),
            typeof(float), typeof(double), typeof(decimal)}
                .Contains(type);

    }
}
