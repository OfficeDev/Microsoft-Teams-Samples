namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.AppConfig
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;

    /// <summary>
    /// Application configuration repository class.
    /// </summary>
    public class AppConfigRepository : BaseRepository<AppConfigEntity>, IAppConfigRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store app settings.
        /// </summary>
        private const string TableName = "AppSettings";

        /// <summary>
        /// Represents the partitionKey used to store app settings.
        /// </summary>
        private const string TablePartitionKey = "Settings";

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigRepository"/> class.
        /// </summary>
        /// <param name="options">Entity represents bot options.</param>
        /// <param name="logger">Entity represents logger object.</param>
        public AppConfigRepository(
            IOptions<BlobStorageSetting> options,
            ILogger<AppConfigRepository> logger)
            : base(options?.Value.ConnectionString, TableName, TablePartitionKey, logger)
        {
        }

        /// <summary>
        /// Set the service url.
        /// </summary>
        /// <param name="serviceUrl">Service url.</param>
        /// <returns>Returns true if details inserted successfully,Else returns false.</returns>
        public async Task<bool> SetServiceUrlAsync(string serviceUrl)
        {
            if (serviceUrl == null)
            {
                throw new ArgumentNullException(nameof(serviceUrl), "The entity details should be provided");
            }

            await this.EnsureInitializedAsync();
            var item = await this.GetAllAsync<AppConfigEntity>();
            if (item == null || item.Count() == 0)
            {
                await this.CreateAsync(new AppConfigEntity()
                {
                    ServiceUrl = serviceUrl,
                    Id = Guid.NewGuid().ToString(),
                });
            }

            return true;
        }

        /// <summary>
        /// Get service url.
        /// </summary>
        /// <returns>Return the service url.</returns>
        public async Task<string> GetServiceUrlAsync()
        {
            await this.EnsureInitializedAsync();
            var item = await this.GetAllAsync<AppConfigEntity>();

            return item.FirstOrDefault()?.ServiceUrl;
        }
    }
}