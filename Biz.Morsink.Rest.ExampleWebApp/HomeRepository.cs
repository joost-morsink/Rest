﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A Repository for the Home resource.
    /// </summary>
    public class HomeRepository : RestRepository<Home>, IRestGet<Home, NoParameters>
    {
        /// <summary>
        /// Get implementation for the Home Resource.
        /// </summary>
        /// <param name="id">A dummy identity value for the Home Resource.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest response containing the Home Resource.</returns>
        public ValueTask<RestResponse<Home>> Get(IIdentity<Home> id, NoParameters parameters)
        {
            return Rest.ValueBuilder(Home.Instance).WithLink(Link.Create("admin", FreeIdentity<Person>.Create(1))).BuildResponseAsync();
        }
    }
}