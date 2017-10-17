using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A Rest Identity Provider for the Example WebApp
    /// </summary>
    public class ExampleRestIdentityProvider : RestIdentityProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ExampleRestIdentityProvider()
        {
            BuildEntry(typeof(TypeDescriptor)).WithPath("/schema/*").Add();
            BuildEntry(typeof(Person)).WithPath("/person/*").Add();
            BuildEntry(typeof(PersonCollection)).WithPath("/person?*").Add();
            BuildEntry(typeof(Home)).WithPath("/?*").Add();
        }
    }
}
