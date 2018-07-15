using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Configuration for the JsonHttpConverter
    /// </summary>
    public class JsonHttpConverterOptions
    {
        public JsonHttpConverterOptions()
        {
            SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize; // Needed for identity based embeddings.
        }
        /// <summary>
        /// Gets or sets the JsonSerializerSettings for the JsonHttpConverter.
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings();

        /// <summary>
        /// Gets or sets the NamingStrategy for the JsonHttpConverter.
        /// </summary>
        public NamingStrategy NamingStrategy { get; set; }
        /// <summary>
        /// Gets or sets support for F# types for the JsonHttpConverter.
        /// </summary>
        public bool FSharpSupport { get; set; }
        /// <summary>
        /// Gets or sets the property name for the link location. 
        /// Should be set to null for HTTP header location.
        /// </summary>
        public string LinkLocation { get; set; }
        /// <summary>
        /// Gets or sets a boolean indicating whether the embeddings from a Rest value should literally be embedded in the Json response.
        /// </summary>
        public bool EmbedEmbeddings { get; set; }
        /// <summary>
        /// Applies the default naming strategy to the JsonHttpConverter.
        /// </summary>
        /// <returns>The current instance.</returns>
        public JsonHttpConverterOptions ApplyDefaultNamingStrategy()
        {
            NamingStrategy = new DefaultNamingStrategy();
            return this;
        }
        /// <summary>
        /// Applies a camelCase naming strategy to the JsonHttpConverter.
        /// </summary>
        /// <returns>The current instance.</returns>
        public JsonHttpConverterOptions ApplyCamelCaseNamingStrategy()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
            return this;
        }
        /// <summary>
        /// Applies F# support to JsonHttpConverter.
        /// </summary>
        /// <param name="support">True if support should be enabled, and false for disabled.</param>
        /// <returns>The current instance.</returns>
        public JsonHttpConverterOptions UseFSharpSupport(bool support = true)
        {
            FSharpSupport = support;
            return this;
        }
        /// <summary>
        /// Sets the propertyName for the collection of links.
        /// </summary>
        /// <param name="propertyName">The property name for the link location.</param>
        /// <returns>The current instance.</returns>
        public JsonHttpConverterOptions UseLinkLocation(string propertyName)
        {
            LinkLocation = propertyName ?? throw new ArgumentNullException(nameof(propertyName), "To indicate header area use UseLinksInHeaders method instead.");
            return this;
        }
        /// <summary>
        /// Sets the location for the links to the HTTP header area.
        /// </summary>
        /// <returns>The current instance.</returns>
        public JsonHttpConverterOptions UseLinksInHeaders()
        {
            LinkLocation = null;
            return this;
        }
        /// <summary>
        /// Sets the EmbedEmbeddings flag.
        /// </summary>
        /// <returns>The current instance.</returns>
        public JsonHttpConverterOptions UseEmbeddings(bool embed = true)
        {
            EmbedEmbeddings = embed;
            return this;
        }
    }
}