﻿using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// This class represents a reference to another object.
    /// </summary>
    public class Reference
    {
        /// <summary>
        /// A reference to the other object.
        /// </summary>
        [Required]
        public string Ref { get; set; }
    }
}
