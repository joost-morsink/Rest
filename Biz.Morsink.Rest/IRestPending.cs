namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This interface indicates the Rest result is still pending.
    /// </summary>
    public interface IRestPending
    {
        /// <summary>
        /// Gets a Rest job that has a reference to the long running Rest response.
        /// </summary>
        RestJob Job { get; }
    }
}