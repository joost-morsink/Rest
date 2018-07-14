using Microsoft.Extensions.DependencyInjection;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    /// <summary>
    /// A decorating interface over IServiceCollection to apply HalJsonHttpConverter related configuration.
    /// </summary>
    public interface IHalJsonHttpConverterBuilder
    {
        /// <summary>
        /// The underlying IServiceCollection.
        /// </summary>
        IServiceCollection ServiceCollection { get; }
    }
}