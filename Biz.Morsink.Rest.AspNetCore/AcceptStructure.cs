using System;
using System.Collections.Generic;
using System.Globalization;
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
            public Case(string str)
            {
                str = str.Trim();
                if (str.IndexOf(';') < 0)
                {
                    MainWildcard = str == "*/*";
                    SubWildcard = str.EndsWith("/*");
                    MimeType = str.Trim(' ', '*');
                    Q = 1m;
                }
                else
                {
                    var parts = str.Split(';');
                    MainWildcard = parts[0] == "*/*";
                    SubWildcard = parts[0].EndsWith("/*");
                    MimeType = parts[0].Trim(' ', '*');
                    var qpart = parts[1].Trim();
                    Q = qpart.StartsWith("q=") ? decimal.Parse(qpart.Substring(2), CultureInfo.InvariantCulture) : 1m;
                }
            }

            /// <summary>
            /// Contains the mime type (or part thereof, depending on the wildcard properties).
            /// </summary>
            public string MimeType { get; }
            /// <summary>
            /// Indicates whether the case is a */* pattern.
            /// </summary>
            public bool MainWildcard { get; }
            /// <summary>
            /// Indicates whether the case has a /* suffix.
            /// </summary>
            public bool SubWildcard { get; }
            /// <summary>
            /// Gets the Q value for this case.
            /// </summary>
            public decimal Q { get; }
            /// <summary>
            /// Scores a mime type against this case.
            /// </summary>
            /// <param name="mimeType">The mime type to score.</param>
            /// <returns>A score.</returns>
            public decimal Score(string mimeType, string suffix)
            {
                if (MainWildcard)
                    return Q;
                else if (SubWildcard)
                    return mimeType.StartsWith(MimeType) ? Q : 0m;
                else if (mimeType == MimeType)
                    return Q;
                else if (suffix != null && MimeType.EndsWith("+" + suffix))
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
        public (Case, decimal) Score(string mimeType, string suffix)
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
