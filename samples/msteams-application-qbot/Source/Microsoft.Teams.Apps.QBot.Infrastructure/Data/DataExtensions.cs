namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.QBot.Domain;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Infrastructure.Data.Repositories;

    /// <summary>
    /// Data extensions.
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects concrete implementations of repositories defined in the Domain project.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service Collection.</returns>
        public static IServiceCollection AddSqlServerStorage(this IServiceCollection services, IConfiguration configuration)
        {
            // Db Context
            services.AddDbContext<QBotDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer"), options => options.EnableRetryOnFailure(5)));

            // Auto-Mapper
            services.AddAutoMapper(typeof(Profiles));

            // Repositories
            services.AddTransient<ICourseRepository, CourseRepository>();
            services.AddTransient<IChannelRepository, ChannelRepository>();
            services.AddTransient<IMemberRepository, MemberRepository>();
            services.AddTransient<ITutorialGroupRepository, TutorialGroupRepository>();
            services.AddTransient<IQuestionRespository, QuestionRepository>();
            services.AddTransient<IAppSettingsRepository, AppSettingsRepository>();
            services.AddTransient<IKnowledgeBaseRepository, KnowledgeBaseRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            return services;
        }
    }
}
