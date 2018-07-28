using Biz.Morsink.Rest.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Options class for Rest for ASP.Net core.
    /// </summary>
    public class RestAspNetCoreOptions : IOptions<RestAspNetCoreOptions>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public RestAspNetCoreOptions() { }
        /// <summary>
        /// Indicates whether compact Uri's should be used, when possible.
        /// </summary>
        public bool UseCuries { get; set; }
        /// <summary>
        /// Sets the HTTP header name for version information.
        /// </summary>
        public string VersionHeader { get; set; }
        /// <summary>
        /// Sets the HTTP header name for supported version information.
        /// </summary>
        public string SupportedVersionsHeader { get; set; }
        /// <summary>
        /// Contains a VersionMatcher to find the correct Rest capability that is requested.
        /// </summary>
        public VersionMatcher DefaultVersionMatcher { get; private set; }
        /// <summary>
        /// Sets the Version matcher to match the oldest available version.
        /// </summary>
        /// <returns>The current instance.</returns>
        public RestAspNetCoreOptions UseOldestVersion()
        {
            DefaultVersionMatcher = VersionMatcher.Oldest;
            return this;
        }
        /// <summary>
        /// Sets the Version matcher to match the newest or latest available version.
        /// </summary>
        /// <returns>The current instance.</returns>
        public RestAspNetCoreOptions UseNewestVersion()
        {
            DefaultVersionMatcher = VersionMatcher.Newest;
            return this;
        }
        /// <summary>
        /// Gets the current instance.
        /// </summary>
        RestAspNetCoreOptions IOptions<RestAspNetCoreOptions>.Value => this;
    }
}
