using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Jobs;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    public class RestJobResultRepresentation : SimpleTypeRepresentation<RestJobResult, RestJobResultRepresentation.Representation>
    {
        public class Representation
        {
            [Required]
            public IIdentity Id { get; set; }
            [Required]
            public string Type { get; set; }
            [Required]
            public bool IsSuccess { get; set; }
            public object Value { get; set; }
            public IReadOnlyList<object> Embeddings { get; set; }
            public IReadOnlyList<Link> Links { get; set; }
            [Required]
            public Dictionary<string, object> Metadata { get; set; }
        }
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
