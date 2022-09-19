namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserRepository
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This class manages storage operations related to user configurations.
    /// </summary>
    public class UserRepository : BaseRepository<UserEntity>, IUserRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store user configurations.
        /// </summary>
        private const string UserConfigurationTable = "UserEntity";

        /// <summary>
        /// Represents the partitionKey used to store user configurations.
        /// </summary>
        private const string TablePartitionKey = "User";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// User repository details.
        /// </summary>
        /// <param name="options">Entity represents bot options.</param>
        /// <param name="logger">Entity represents logger object.</param>
        public UserRepository(
            IOptions<BlobStorageSetting> options, 
            ILogger<UserRepository> logger) 
            : base(options?.Value.ConnectionString, UserConfigurationTable, TablePartitionKey, logger)
        {
        }

        /// <summary>
        /// Gets conversation details.
        /// </summary>
        /// <param name="userAadId">Id of user.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully,Else returns false.</returns>
        public async Task<UserEntity> GetConversationAsync(string userAadId)
        {
            await this.EnsureInitializedAsync();
            return await this.GetAsync<UserEntity>(userAadId);
        }

        /// <summary>
        /// Gets all user conversation details.
        /// </summary>
        /// <returns>Returns true if configuration details inserted or updated successfully,Else returns false.</returns>
        public async Task<IEnumerable<UserEntity>> GetAllUserConversationAsync()
        {
            await this.EnsureInitializedAsync();
            return await this.GetAllAsync<UserEntity>();
        }

        /// <summary>
        /// Insert or update a new user configuration details when user installs a Bot.
        /// </summary>
        /// <param name="userConfigurationDetails">The user configuration details.</param>
        /// <returns>Returns true if user configuration details inserted or updated successfully,Else returns false.</returns>
        public async Task<bool> CreateUserConfigurationsAsync(UserEntity userConfigurationDetails)
        {
            if (userConfigurationDetails == null)
            {
                throw new ArgumentNullException(nameof(userConfigurationDetails), "The user configuration details should be provided");
            }

            await this.EnsureInitializedAsync();
            var user = await this.GetAsync<UserEntity>(userConfigurationDetails.UserId);
            if (user == null)
            {
                return await this.CreateAsync(userConfigurationDetails);
            }

            return true;
        }

        /// <summary>
        /// Insert or update a new user configuration details when user installs a Bot.
        /// </summary>
        /// <param name="userConfigurationDetails">The user configuration details.</param>
        /// <returns>Returns true if user configuration details inserted or updated successfully,Else returns false.</returns>
        public async Task<bool> UpdateUserConfigurationsAsync(UserEntity userConfigurationDetails)
        {
            if (userConfigurationDetails == null)
            {
                throw new ArgumentNullException(nameof(userConfigurationDetails), "The user configuration details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.UpdateAsync(userConfigurationDetails);
        }

        /// <summary>
        /// Get user by user id.
        /// </summary>
        /// <param name="userAadId">Aad id of user.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully,Else returns false.</returns>
        public async Task<IEnumerable<UserEntity>> GetUserByUserIdAsync(string userAadId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(UserEntity.UserId)} eq '{userAadId}'";
            return await this.GetWithFilterAsync<UserEntity>(query);
        }

        /// <summary>
        /// Gets conversation details.
        /// </summary>
        /// <param name="teamId">Id of user.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully,Else returns false.</returns>
        public async Task<IEnumerable<UserEntity>> GetUserByTenantIdAsync(string tenantId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(UserEntity.TenantId)} eq '{tenantId}'";
            return await this.GetWithFilterAsync<UserEntity>(query);
        }

        /// <summary>
        /// Insert or delete a new user configuration details when user installs a Bot.
        /// </summary>
        /// <param name="userConfigurationDetails">The user configuration details.</param>
        /// <returns>Returns true if user configuration details inserted or updated successfully,Else returns false.</returns>
        public async Task<bool> DeleteUserConfigurationsAsync(UserEntity userConfigurationDetails)
        {
            if (userConfigurationDetails == null)
            {
                throw new ArgumentNullException(nameof(userConfigurationDetails), "The user configuration details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.DeleteAsync(userConfigurationDetails);
        }
    }
}