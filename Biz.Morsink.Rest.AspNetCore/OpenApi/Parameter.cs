namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// This class represents a parameter to an operation.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The in-indicator.
        /// Should be one of: query, path, header, cookie.
        /// </summary>
        public string In { get; set; }
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