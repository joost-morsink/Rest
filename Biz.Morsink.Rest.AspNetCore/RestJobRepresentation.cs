using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    public class RestJobRepresentation : ITypeRepresentation
    {
        private class representation
        {
            [Required]
            public IIdentity Id { get; set; }
            [Required]
            public bool IsFinished { get; set; }
        }

        public object GetRepresentable(object rep)
        {
            throw new NotImplementedException();
        }

        public Type GetRepresentableType(Type type)
            => type == typeof(representation) ? typeof(RestJob) : null;

        public object GetRepresentation(object obj)
        {
            var job = (RestJob)obj;
            return new representation
            {
                Id = job.Id ,
                IsFinished = job.Task.Status >= TaskStatus.RanToCompletion
            };
        }

        public Type GetRepresentationType(Type type)
            => type == typeof(RestJob) ? typeof(representation) : null;

        public bool IsRepresentable(Type type)
            => type == typeof(RestJob);

        public bool IsRepresentation(Type type)
            => type == typeof(representation);
    }
}
