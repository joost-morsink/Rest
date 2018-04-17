namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// A Content is the value schema for some media type.
    /// </summary>
    public class Content
    {
        /// <summary>
        /// A schema or schema reference for this kind of content.
        /// </summary>
        public OrReference<Schema> Schema { get; set; }
    }
}