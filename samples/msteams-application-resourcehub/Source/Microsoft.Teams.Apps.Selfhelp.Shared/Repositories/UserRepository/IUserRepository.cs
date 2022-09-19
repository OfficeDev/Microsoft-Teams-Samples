namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This interface lists all the methods which are used to manage storing and deleting user configurations.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Inserts or updates a new user configuration details when user installs a Bot.
        /// </summary>
        /// <param name="conversationDetails">The user configuration details.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        Task<bool> CreateUserConfigurationsAsync(UserEntity conversationDetails);

        /// <summary>
        /// Inserts or updates a new user configuration details when user installs a Bot.
        /// </summary>
        /// <param name="conversationDetails">The user configuration details.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        Task<bool> UpdateUserConfigurationsAsync(UserEntity conversationDetails);

        /// <summary>
        /// Gets conversation details.
        /// </summary>
        /// <param name="userAadId">User Aad id.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        Task<UserEntity> GetConversationAsync(string userAadId);

        /// <summary>
        /// Insert or delete a new user configuration details when user installs a Bot.
        /// </summary>
        /// <param name="userConfigurationDetails">The user configuration details.</param>
        /// <returns>Returns true if user configuration details inserted or updated successfully. Else returns false.</returns>
        Task<bool> DeleteUserConfigurationsAsync(UserEntity userConfigurationDetails);

        /// <summary>
        /// Gets all user conversation details.
        /// </summary>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        Task<IEnumerable<UserEntity>> GetAllUserConversationAsync();

        /// <summary>
        /// Gets User details.
        /// </summary>
        /// <param name="userAadId">User Aad id.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        Task<IEnumerable<UserEntity>> GetUserByUserIdAsync(string userAadId);

        /// <summary>
        /// Get all user tenant id details.
        /// </summary>
        /// <param name="tenantId">Unique tenant id.</param>
        /// <returns>Returns all user tenant id details.</returns>
        Task<IEnumerable<UserEntity>> GetUserByTenantIdAsync(string tenantId);
    }
}