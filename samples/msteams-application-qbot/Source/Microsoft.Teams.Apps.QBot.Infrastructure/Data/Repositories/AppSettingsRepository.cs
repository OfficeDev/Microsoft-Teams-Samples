// <copyright file="AppSettingsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Repositories
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;

    /// <summary>
    /// App settings repository implementaiton.
    /// </summary>
    internal class AppSettingsRepository : IAppSettingsRepository
    {
        private const string ServiceUrlKey = "service_url";

        private readonly QBotDbContext dbContext;
        private readonly ILogger<AppSettingsRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsRepository"/> class.
        /// </summary>
        /// <param name="dbContext">Db Conext.</param>
        /// <param name="logger">Logger.</param>
        public AppSettingsRepository(
            QBotDbContext dbContext,
            ILogger<AppSettingsRepository> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public Task AddOrUpdateServiceUrlAsync(string serviceUrl)
        {
            try
            {
                var entity = this.dbContext.AppSettings.Find(ServiceUrlKey);

                if (entity == null)
                {
                    // If setting isn't found, add new
                    entity = new AppSettingEntity()
                    {
                        Key = ServiceUrlKey,
                        Value = serviceUrl,
                    };

                    this.dbContext.Add(entity);
                }
                else
                {
                    // else update existing.
                    entity.Value = serviceUrl;
                    this.dbContext.Update(entity);
                }

                return this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add or update serviceUrl";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public Task<string> GetServiceUrlAsync()
        {
            var setting = this.dbContext.AppSettings.Find(ServiceUrlKey);
            if (setting == null)
            {
                // Setting not found.
                this.logger.LogWarning("Service Url not found!");
                return Task.FromResult(string.Empty);
            }

            return Task.FromResult(setting.Value);
        }
    }
}
