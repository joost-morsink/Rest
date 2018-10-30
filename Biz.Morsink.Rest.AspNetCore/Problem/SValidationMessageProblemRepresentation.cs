using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Problem
{
    /// <summary>
    /// Representation of serialization validation messages with Problem (RFC7807).
    /// </summary>
    public class SValidationMessageProblemRepresentation : SimpleTypeRepresentation<IEnumerable<SValidation.Message>, SValidationMessageProblemRepresentation.Representation>
    {
        /// <summary>
        /// Problem derepresentation is not supported.
        /// </summary>
        public override IEnumerable<SValidation.Message> GetRepresentable(Representation representation)
            => null;

        /// <summary>
        /// Gets the representation for a collection of messages.
        /// </summary>
        /// <param name="item">A collection of messages.</param>
        /// <returns>A Problem representing the messages.</returns>
        public override Representation GetRepresentation(IEnumerable<SValidation.Message> item)
            => new Representation
            {
                Title = "Validation error in serialization",
                Status = 400,
                ValidationErrors = item.GroupBy(i => i.Path).ToDictionary(g => g.Key, g => g.Select(i => i.Error).ToArray())
            };
        /// <summary>
        /// Problem derivation for the representation of serialization validation errors.
        /// </summary>
        public class Representation : Problem
        {
            /// <summary>
            /// A Dictionary containing all errors.
            /// </summary>
            public Dictionary<string, SValidation.Error[]> ValidationErrors { get; set; }
        }

    }
}
