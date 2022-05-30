namespace Microsoft.Teams.Apps.QBot.Infrastructure
{
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.QBot.Infrastructure.Data;

    /// <summary>
    /// Data extensions.
    /// </summary>
    internal class QBotDbContextFactory : IDesignTimeDbContextFactory<QBotDbContext>
    {
        /// <inheritdoc />
        public QBotDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<QBotDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("SqlServer"));

            return new QBotDbContext(optionsBuilder.Options);
        }
    }
}
