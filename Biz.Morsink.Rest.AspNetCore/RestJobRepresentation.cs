using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// A ITypeRepresentation implementation for RestJobs.
    /// RestJob contains a 'Task&lt;RestResponse&gt;'-typed property, and Tasks are not meant to serialize.
    /// Because a RestJob is only serialized and never deserialized, the representation only contains general information about the RestJob.
    /// </summary>
    public class RestJobRepresentation : ITypeRepresentation
    {
        /// <summary>
        /// Representation type
        /// </summary>
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
