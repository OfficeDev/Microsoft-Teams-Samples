namespace Microsoft.Teams.Apps.QBot.Infrastructure.Secrets
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.Authentication;

    /// <summary>
    /// Secrets provider interface.
    /// </summary>
    public interface ISecretsProvider
    {
        /// <summary>
        /// Gets bot app credentials.
        /// </summary>
        /// <returns>Bot app credentials.</returns>
        Task<AppCredentials> GetBotAppCredentialsAsync();
    }
}
