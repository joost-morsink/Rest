using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A listener interface for unexpected exceptions in the Rest request handler pipeline.
    /// </summary>
    public interface IRestExceptionListener
    {
        /// <summary>
        /// This method gets called when an exception occurs.
        /// </summary>
        /// <param name="ex">The unexpected exception.</param>
        void UnexpectedExceptionOccured(Exception ex);
    }
}
