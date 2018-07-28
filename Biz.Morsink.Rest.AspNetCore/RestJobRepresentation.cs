using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Jobs;
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
    public class RestJobRepresentation : SimpleTypeRepresentation<RestJob, RestJobRepresentation.Representation>
    {
        /// <summary>
        /// Representation type
        /// </summary>
        public class Representation
        {
            [Required]
            public IIdentity Id { get; set; }
            [Required]
            public bool IsFinished { get; set; }
        }

        public override RestJob GetRepresentable(Representation representation)
        {
            throw new NotSupportedException();
        }

        public override Representation GetRepresentation(RestJob job)
        {
            return new Representation
            {
                Id = job.Id,
                IsFinished = job.Task.Status >= TaskStatus.RanToCompletion
            };
        }
    }
}
