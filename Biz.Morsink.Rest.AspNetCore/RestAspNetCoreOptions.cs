using Biz.Morsink.Rest.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    public class RestAspNetCoreOptions : IOptions<RestAspNetCoreOptions>
    {
        public RestAspNetCoreOptions() { }
        public bool UseCuries { get; set; }
        public string VersionHeader { get; set; }
        public VersionMatcher DefaultVersionMatcher { get; private set; }
        public RestAspNetCoreOptions UseOldestVersion()
        {
            DefaultVersionMatcher = VersionMatcher.Oldest;
            return this;
        }
        public RestAspNetCoreOptions UseNewestVersion()
        {
            DefaultVersionMatcher = VersionMatcher.Newest;
            return this;
        }

        RestAspNetCoreOptions IOptions<RestAspNetCoreOptions>.Value => this;
    }
}
