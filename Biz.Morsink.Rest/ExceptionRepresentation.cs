using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Representation class for exceptions.
    /// </summary>
    public class ExceptionRepresentation : SimpleTypeRepresentation<Exception, ExceptionInfo>
    {
        /// <summary>
        /// Representation of Exceptions works one way only.
        /// This method throws a NotSupportedException.
        /// </summary>
        public override Exception GetRepresentable(ExceptionInfo representation)
        {
            throw new NotSupportedException();
        }
        public override ExceptionInfo GetRepresentation(Exception item)
            => ExceptionInfo.Create(item);
    }
}
