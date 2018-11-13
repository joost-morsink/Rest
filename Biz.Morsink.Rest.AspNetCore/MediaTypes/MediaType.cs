using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    public struct MediaType : IEquatable<MediaType>
    {
        public MediaType(StringSegment main, StringSegment sub, StringSegment suffix, params MediaTypeParameter[] parameters)
        {
            Main = main;
            Sub = sub;
            Suffix = suffix;
            Parameters = parameters ?? new MediaTypeParameter[0];
        }

        public StringSegment Main { get; }
        public StringSegment Sub { get; }
        public StringSegment Suffix { get; }
        public MediaTypeParameter[] Parameters { get; }
        public override string ToString()
        {
            var str = Suffix == null ? $"{Main}/{Sub}" : $"{Main}/{Sub}+{Suffix}";
            if (Parameters.Length == 0)
                return str;
            else
                return string.Join(";", Parameters.Select(p => p.ToString()).Prepend(str));
        }
        public static MediaType? TryParse(StringSegment str)
            => TryParse(str, out var res) ? res : default(MediaType?);
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

        public MediaType WithoutSuffix()
            => new MediaType(Main, Sub, null, Parameters);
        public MediaType WithSuffix(string suffix)
            => new MediaType(Main, Sub, suffix, Parameters);
    }

    public struct MediaTypeParameter : IEquatable<MediaTypeParameter>
    {
        public MediaTypeParameter(StringSegment name, StringSegment value)
        {
            Name = name;
            Value = value;
        }
        public StringSegment Name { get; }
        public StringSegment Value { get; }
        public override string ToString()
            => $"{Name}={Value}";
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
