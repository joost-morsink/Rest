using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Jobs;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class respresents RestJobResults.
    /// </summary>
    public class RestJobResultRepresentation : SimpleTypeRepresentation<RestJobResult, RestJobResultRepresentation.Representation>
    {
        /// <summary>
        /// The actual representation class for RestJobResults.
        /// </summary>
        public class Representation
        {
            /// <summary>
            /// The identity value of the result.
            /// </summary>
            [Required]
            public IIdentity Id { get; set; }
            /// <summary>
            /// The type.
            /// </summary>
            [Required]
            public string Type { get; set; }
            /// <summary>
            /// Indicator of success for the result.
            /// </summary>
            [Required]
            public bool IsSuccess { get; set; }
            /// <summary>
            /// The result's value, if successful.
            /// </summary>
            public object Value { get; set; }
            /// <summary>
            /// The result's embeddings, if successful.
            /// </summary>
            public IReadOnlyList<object> Embeddings { get; set; }
            /// <summary>
            /// The result's links, if successful.
            /// </summary>
            public IReadOnlyList<Link> Links { get; set; }
            /// <summary>
            /// Metadata in the response.
            /// </summary>
            [Required]
            public Dictionary<string, object> Metadata { get; set; }
        }
        /// <summary>
        /// Only one-way representation is supported.
        /// This method throws a NotSupportedException.
        /// </summary>
        public override RestJobResult GetRepresentable(Representation representation)
        {
            throw new NotSupportedException();
        }

        public override Representation GetRepresentation(RestJobResult res)
        {
            if (res.Job.Task.IsCompleted)
            {
                var rv = res.Job.Task.Result.UntypedResult as IHasRestValue;
                return new Representation
                {
                    Id = res.Id,
                    Type = res.Job.Task.Result.IsSuccess ? "Success" : res.Job.Task.Result.UntypedResult.AsFailure().Reason.ToString(),
                    IsSuccess = res.Job.Task.Result.UntypedResult.IsSuccess,
                    Metadata = res.Job.Task.Result.Metadata.AsEnumerable().ToDictionary(kvp => kvp.Key.Name, kvp => kvp.Value),
                    Value = rv?.RestValue.Value,
                    Embeddings = rv?.RestValue.Embeddings,
                    Links = rv?.RestValue.Links
                };
            }
            else
                return null;
        }
    }
}
