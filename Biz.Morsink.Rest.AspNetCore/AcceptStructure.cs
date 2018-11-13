using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.AspNetCore.MediaTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Helper class for evaluating HTTP Accept headers.
    /// </summary>
    public class AcceptStructure
    {
        /// <summary>
        /// Contains all the cases in the Accept header.
        /// </summary>
        public Case[] Cases { get; }
        /// <summary>
        /// This nested class represents a single case in the Accept header.
        /// </summary>
        public class Case
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="str">The string representing the case.</param>
            public Case(MediaType str)
            {
                MediaType = str;
            }

            /// <summary>
            /// Contains the mime type (or part thereof, depending on the wildcard properties).
            /// </summary>
            public MediaType MediaType { get; }
            /// <summary>
            /// Indicates whether the case is a */* pattern.
            /// </summary>
            public bool MainWildcard => MediaType.Main == "*";
            /// <summary>
            /// Indicates whether the case has a /* suffix.
            /// </summary>
            public bool SubWildcard => MediaType.Sub == "*";
            /// <summary>
            /// Gets the Q value for this case.
            /// </summary>
            public decimal Q => 
                MediaType.Parameters
                .Where(p => p.Name == "q")
                .Select(p => DataConverter.Default.Convert(p.Value).To(0m))
                .Append(1m)
                .First();

            /// <summary>
            /// Scores a mime type against this case.
            /// </summary>
            /// <param name="mimeType">The mime type to score.</param>
            /// <returns>A score.</returns>
            public decimal Score(MediaType mimeType, string suffix)
            {
                if (MainWildcard)
                    return Q;
                else if (SubWildcard)
                    return mimeType.Main == MediaType.Main ? Q : 0m;
                else if (mimeType == MediaType)
                    return Q;
                else if (suffix != null && MediaType.Suffix == suffix)
                    return Q - 0.001m;
                else
                    return 0m;
            }
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="acceptValues">All the Accept header parts.</param>
        public AcceptStructure(IReadOnlyList<string> acceptValues)
        {
            Cases = new Case[acceptValues.Count];
            for (int i = 0; i < Cases.Length; i++)
                Cases[i] = new Case(acceptValues[i]);
            Array.Sort(Cases, (x, y) => -x.Q.CompareTo(y.Q));
        }
        /// <summary>
        /// Scores a mime type against the Accept header.
        /// </summary>
        /// <param name="mimeType">The mime type to score.</param>
        /// <returns>A score.</returns>
        public (Case, decimal) Score(MediaType mimeType, string suffix)
        {
            for (int i = 0; i < Cases.Length; i++)
            {
                var q = Cases[i].Score(mimeType, suffix);
                if (q > 0m)
                    return (Cases[i], q);
            }
            return (null, 0m);
        }
    }
}
