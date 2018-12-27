using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// A struct representing a media type.
    /// </summary>
    public struct MediaType : IEquatable<MediaType>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="main">The main (pre-slash) part of the mediatype.</param>
        /// <param name="sub">The sub (post-slash, pre-plus) part of the mediatype.</param>
        /// <param name="suffix">An optional suffix (post-plus) part for the media type</param>
        /// <param name="parameters">Optional parameters for the media type.</param>
        public MediaType(StringSegment main, StringSegment sub, StringSegment suffix, params MediaTypeParameter[] parameters)
        {
            Main = main;
            Sub = sub;
            Suffix = suffix;
            Parameters = parameters ?? new MediaTypeParameter[0];
        }
        /// <summary>
        /// The main part of the media type.
        /// </summary>
        public StringSegment Main { get; }
        /// <summary>
        /// The sub part of the media type.
        /// </summary>
        public StringSegment Sub { get; }
        /// <summary>
        /// The optional media type suffix.
        /// </summary>
        public StringSegment Suffix { get; }
        /// <summary>
        /// Optional parameters for the media type.
        /// </summary>
        public MediaTypeParameter[] Parameters { get; }
        public override string ToString()
        {
            var str = Suffix == null ? $"{Main}/{Sub}" : $"{Main}/{Sub}+{Suffix}";
            if (Parameters.Length == 0)
                return str;
            else
                return string.Join(";", Parameters.Select(p => p.ToString()).Prepend(str));
        }
        /// <summary>
        /// Tries to parse a media type instance from a StringSegment.
        /// </summary>
        /// <param name="str">A StringSegment to parse.</param>
        /// <returns>A nullable MediaType.</returns>
        public static MediaType? TryParse(StringSegment str)
            => TryParse(str, out var res) ? res : default(MediaType?);
        /// <summary>
        /// Tries to parse a media type instance from a StringSegment.
        /// </summary>
        /// <param name="str">A StringSegment to parse.</param>
        /// <param name="result">The result of the parse action if succesful, null otherwise.</param>
        /// <returns>True if the StringSegment could be parsed as a MediaType, false otherwise.</returns>
        public static bool TryParse(StringSegment str, out MediaType result)
        {
            var parts = str.Split(new[] { ';' }).ToArray();
            var slashIdx = parts[0].IndexOf('/');
            if (slashIdx < 0)
            {
                result = default;
                return false;
            }
            var plusIdx = parts[0].IndexOf('+', slashIdx);

            if (plusIdx < slashIdx)
                result = new MediaType(parts[0].Subsegment(0, slashIdx), parts[0].Subsegment(slashIdx + 1), null, parseOtherParts().OrderBy(mtp => mtp.Name).ToArray());
            else
                result = new MediaType(parts[0].Subsegment(0, slashIdx), parts[0].Subsegment(slashIdx + 1, plusIdx - slashIdx - 1), parts[0].Subsegment(plusIdx + 1), parseOtherParts().OrderBy(mtp => mtp.Name).ToArray());
            return true;

            IEnumerable<MediaTypeParameter> parseOtherParts()
            {
                for (int i = 1; i < parts.Length; i++)
                    if (MediaTypeParameter.TryParse(parts[i], out var par))
                        yield return par;
            }
        }
        /// <summary>
        /// Parses the string as a MediaType.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns>A MediaType instance.</returns>
        public static MediaType Parse(string str)
        {
            if (!TryParse(str, out var result))
                throw new ArgumentException("Not parseable", nameof(str));
            return result;
        }
        public static implicit operator MediaType(string str)
            => Parse(str);
        public static implicit operator string(MediaType mediaType)
            => mediaType.ToString();

        public override int GetHashCode()
            => Main.GetHashCode() ^ Sub.GetHashCode() ^ (Suffix == null ? 0 : Suffix.GetHashCode());
        public override bool Equals(object obj)
            => obj is MediaType mt && Equals(mt);
        public bool Equals(MediaType other)
            => Main == other.Main && Sub == other.Sub && Suffix == other.Suffix && Parameters.SequenceEqual(other.Parameters);

        /// <summary>
        /// Gets the same MediaType, but without the suffix.
        /// </summary>
        /// <returns>The same MediaType, but without the suffix.</returns>
        public MediaType WithoutSuffix()
            => new MediaType(Main, Sub, null, Parameters);
        /// <summary>
        /// Gets the same MediaType, but with a different suffix.
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns>The same MediaType, but with a different suffix.</returns>
        public MediaType WithSuffix(string suffix)
            => new MediaType(Main, Sub, suffix, Parameters);
    }
    /// <summary>
    /// A parameter for a MediaType.
    /// </summary>
    public struct MediaTypeParameter : IEquatable<MediaTypeParameter>
    {
        /// <summary>
        /// Cosntructor.
        /// </summary>
        /// <param name="name">The name of the media type parameter.</param>
        /// <param name="value">The value of the media type parameter.</param>
        public MediaTypeParameter(StringSegment name, StringSegment value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// The name of the media type parameter.
        /// </summary>
        public StringSegment Name { get; }
        /// <summary>
        /// The Value of the media type parameter.
        /// </summary>
        public StringSegment Value { get; }
        public override string ToString()
            => $"{Name}={Value}";
        /// <summary>
        /// Tries to parse a StringSegment as a MediaTypeParmeter.
        /// </summary>
        /// <param name="str">The StringSegment to parse.</param>
        /// <param name="result">The resulting MediaTypeParameter if successful, default otherwise.</param>
        /// <returns>True if the parse was successful, false otherwise.</returns>
        public static bool TryParse(StringSegment str, out MediaTypeParameter result)
        {
            var idx = str.IndexOf('=');
            if (idx > 0)
            {
                result = new MediaTypeParameter(str.Subsegment(0, idx), str.Subsegment(idx + 1));
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
        public override int GetHashCode()
            => Name.GetHashCode() ^ Value.GetHashCode();
        public override bool Equals(object obj)
            => obj is MediaTypeParameter mtp && Equals(mtp);
        public bool Equals(MediaTypeParameter other)
            => Name == other.Name && Value == other.Value;
        public static bool operator ==(MediaTypeParameter left, MediaTypeParameter right)
            => left.Equals(right);
        public static bool operator !=(MediaTypeParameter left, MediaTypeParameter right)
            => !left.Equals(right);
    }
}
