using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Jobs;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    public class RestJobResultRepresentation : ITypeRepresentation
    {
        private class representation
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
            public TypeKeyedDictionary Metadata { get; set; }
        }
        public object GetRepresentable(object rep)
        {
            throw new NotImplementedException();
        }

        public Type GetRepresentableType(Type type)
            => type == typeof(representation) ? typeof(RestJobResult) : null;

        public object GetRepresentation(object obj)
        {
            var res = (RestJobResult)obj;
            if (res.Job.Task.IsCompleted)
            {
                var rv = res.Job.Task.Result.UntypedResult as IHasRestValue;
                return new representation
                {
                    Id = res.Id,
                    Type = res.Job.Task.Result.IsSuccess ? "Success": res.Job.Task.Result.UntypedResult.AsFailure().Reason.ToString(),
                    IsSuccess = res.Job.Task.Result.UntypedResult.IsSuccess,
                    Metadata = res.Job.Task.Result.Metadata,
                    Value = rv?.RestValue.Value,
                    Embeddings = rv?.RestValue.Embeddings,
                    Links = rv?.RestValue.Links
                };
            }
            else
                return null;
        }

        public Type GetRepresentationType(Type type)
            => type == typeof(RestJobResult) ? typeof(representation) : null;

        public bool IsRepresentable(Type type)
            => type == typeof(RestJobResult);

        public bool IsRepresentation(Type type)
            => type == typeof(representation);
    }
}
