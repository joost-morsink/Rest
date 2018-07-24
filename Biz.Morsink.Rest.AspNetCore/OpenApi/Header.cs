namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// This class represents header information in an OpenAPI Specification version 3.0
    /// </summary>
    public class Header
    {
        /// <summary>
        /// The parameter's description.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Indicates whether the parameter is required or not.
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Indicates whether the parameter allows for empty values.
        /// </summary>
        public bool? AllowEmptyValue { get; set; }
        /// <summary>
        /// The schema for the parameter.
        /// </summary>
        public OrReference<Schema> Schema { get; set; }
    }
}