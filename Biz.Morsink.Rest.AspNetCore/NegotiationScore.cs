using Biz.Morsink.Rest.AspNetCore.MediaTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Represents a result for a content negotiation step.
    /// </summary>
    public struct NegotiationScore
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mediaType">The media type.</param>
        /// <param name="q">The effective Q for the media type.</param>
        public NegotiationScore(MediaType mediaType, decimal q)
        {
            MediaType = mediaType;
            Q = q;
        }
        /// <summary>
        /// The media type.
        /// </summary>
        public MediaType MediaType { get; }
        /// <summary>
        /// The effective Q value.
        /// </summary>
        public decimal Q { get; }

        /// <summary>
        /// Creates a new NegotiationScore by manipulating the Q value.
        /// </summary>
        /// <param name="f">A function to manipulate the Q value with.</param>
        /// <returns>A new NegotiationScore.</returns>
        public NegotiationScore WithQ(Func<decimal, decimal> f)
            => new NegotiationScore(MediaType, f(Q));
        /// <summary>
        /// Creates a new NegotiationScore by assigning a new Q value.
        /// </summary>
        /// <param name="q">A new Q value.</param>
        /// <returns>A new NegotiationScore.</returns>
        public NegotiationScore WithQ(decimal q)
            => new NegotiationScore(MediaType, q);
    }
}
