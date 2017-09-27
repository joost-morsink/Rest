using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// The Home Resource is a representation of the Home Page for the API.
    /// </summary>
    public class Home
    {
        /// <summary>
        /// The Home resource does not contain any data, and is available as a singleton through this property.
        /// </summary>
        public static Home Instance { get; } = new Home();
    }
}
