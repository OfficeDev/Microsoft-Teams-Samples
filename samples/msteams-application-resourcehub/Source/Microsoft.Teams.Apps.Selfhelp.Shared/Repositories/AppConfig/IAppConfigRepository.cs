namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.AppConfig
{
    /// <summary>
    /// Interface for task information repository.
    /// </summary>
    public interface IAppConfigRepository
    {
        /// <summary>
        /// Set the service url.
        /// </summary>
        /// <param name="serviceUrl">Sevice url.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<bool> SetServiceUrlAsync(string serviceUrl);

        /// <summary>
        /// Get the service url.
        /// </summary>
        /// <returns>Returns the service url.</returns>
        Task<string> GetServiceUrlAsync();
    }
}