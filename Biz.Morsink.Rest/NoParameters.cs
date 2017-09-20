using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Parameter class that indicates no parameters are needed.
    /// </summary>
    public class NoParameters
    {
        /// <summary>
        /// Constructor/
        /// </summary>
        /// <param name="_">Dummy parameter needed for DataConvert</param>
        public NoParameters(object _ = null) { }
        
        /// <summary>
        /// Dummy property needed for DataConvert.
        /// </summary>
        public object _ => null;
    }
}
