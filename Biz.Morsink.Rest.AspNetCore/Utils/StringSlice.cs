using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    /// <summary>
    /// This struct represents a slice of a bigger string.
    /// The slice is 'aware' of its containing string.
    /// </summary>
    public struct StringSlice
    {
        /// <summary>
        /// Constructor. 
        /// Constructs a 'full slice'.
        /// </summary>
        /// <param name="fullString">The unsliced string.</param>
        public StringSlice(string fullString) : this(fullString, 0, fullString.Length) { }
        /// <summary>
        /// Constructor.
        /// Constructs a slice from a certain offset to the end of the string.
        /// </summary>
        /// <param name="fullString">The unsliced string.</param>
        /// <param name="offset">The offset of the slice within the string.</param>
        public StringSlice(string fullString, int offset) : this(fullString, offset, fullString.Length - offset) { }
        /// <summary>
        /// Constructor.
        /// Constructs a slice with a certain offset and length.
        /// </summary>
        /// <param name="fullString">The unsliced string.</param>
        /// <param name="offset">The offset of the slice within the string.</param>
        /// <param name="length">The length of the slice.</param>
        public StringSlice(string fullString, int offset, int length)
        {
            if (offset + length > fullString.Length)
                throw new ArgumentOutOfRangeException("Substring out of range.");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("Offset should be positive.");
            if (length < 0)
                throw new ArgumentOutOfRangeException("Length should be positive.");
            FullString = fullString;
            Offset = offset;
            Length = length;
        }
        /// <summary>
        /// The underlying full string.
        /// </summary>
        public string FullString { get; }
        /// <summary>
        /// The offset of the slice in the string.
        /// </summary>
        public int Offset { get; }
        /// <summary>
        /// The length of the slice.
        /// </summary>
        public int Length { get; }
        /// <summary>
        /// The string value the slice represents.
        /// </summary>
        public string Value => FullString.Substring(Offset, Length);
        /// <summary>
        /// The length of the unsliced string.
        /// </summary>
        public int FullLength => FullString.Length;
        /// <summary>
        /// The prefix of the slice.
        /// </summary>
        public StringSlice Prefix => new StringSlice(FullString, 0, Offset);
        /// <summary>
        /// The prefix of the slice concatenated with the slice itself.
        /// </summary>
        public StringSlice PrefixWithSlice => new StringSlice(FullString, 0, Offset + Length);
        /// <summary>
        /// The suffix of the slice.
        /// </summary>
        public StringSlice Suffix => new StringSlice(FullString, Offset + Length);
        /// <summary>
        /// The suffix of the slice concatenated to the slice itself.
        /// </summary>
        public StringSlice SliceWithSuffix => new StringSlice(FullString, Offset);
        /// <summary>
        /// Indexer property for the slice.
        /// </summary>
        /// <param name="index">The character index.</param>
        /// <returns>The character at the specified index.</returns>
        public char this[int index] => index >= 0 && index < Length ? FullString[Offset + index] : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>
        /// Tries to match a substring to the slice.
        /// </summary>
        /// <param name="sub">The substring to match.</param>
        /// <returns>The length of the matching parts.</returns>
        public int SubstringMatchLength(string sub)
            => SubstringMatchLength(new StringSlice(sub, 0, sub.Length));
        /// <summary>
        /// Tries to match a substring to the slice.
        /// </summary>
        /// <param name="sub">The substring to match.</param>
        /// <returns>The length of the matching parts.</returns>
        public int SubstringMatchLength(StringSlice sub)
        {
            int i;
            for (i = 0; i < sub.Length && i < Length; i++)
                if (this[i] != sub[i])
                    break;
            return i;
        }
        /// <summary>
        /// Slices the slice at a certain offset.
        /// </summary>
        /// <param name="offset">The offset within this slice.</param>
        /// <returns>A new slice.</returns>
        public StringSlice Slice(int offset)
            => offset >= 0 && offset <= Length ? new StringSlice(FullString, Offset + offset, Length - offset) : throw new ArgumentOutOfRangeException(nameof(offset));
        /// <summary>
        /// Slices the slice at a certain offset with a cetain length.
        /// </summary>
        /// <param name="offset">The offset within this slice.</param>
        /// <param name="length">The length of the resulting slice.</param>
        /// <returns>A new slice.</returns>
        public StringSlice Slice(int offset, int length)
            => offset >= 0 && length >= 0 && offset + length <= Length ? new StringSlice(FullString, Offset + offset, length) : throw new ArgumentOutOfRangeException();
        public override string ToString()
            => Value;

        /// <summary>
        /// Moves the left boundary of the slice.
        /// </summary>
        /// <param name="offset">The offset to move the boundary with.</param>
        /// <returns>A new slice with a moved left boundary.</returns>
        public StringSlice MoveLeftBoundary(int offset)
            => -offset <= Offset && offset - Length <= 0 ? new StringSlice(FullString, Offset + offset, Length - offset) : throw new ArgumentOutOfRangeException(nameof(offset));
        /// <summary>
        /// Moves the right boundary of the slice.
        /// </summary>
        /// <param name="offset">The offset to move the boundary with.</param>
        /// <returns>A new slice with a moved right boundary.</returns>
        public StringSlice MoveRightBoundary(int offset)
            => Offset + Length + offset <= FullLength && -offset < Length ? new StringSlice(FullString, Offset, Length + offset) : throw new ArgumentOutOfRangeException(nameof(offset));
        /// <summary>
        /// Translates the slice to a new position with the same length.
        /// </summary>
        /// <param name="offset">The offset to translate.</param>
        /// <returns>A new translated slice.</returns>
        public StringSlice Translate(int offset)
            => -offset <= Offset && Offset + Length + offset <= FullLength ? new StringSlice(FullString, Offset + offset, Length) : throw new ArgumentOutOfRangeException(nameof(offset));
    }
}
