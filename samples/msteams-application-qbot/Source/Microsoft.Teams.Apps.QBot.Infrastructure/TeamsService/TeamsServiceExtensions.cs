namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;

    /// <summary>
    /// Teams service extensions.
    /// </summary>
    public static class TeamsServiceExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects concrete implementations of teams services defined in the Domain project.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddTeamsServices(this IServiceCollection services)
        {
            services.AddSingleton<ITeamPhotoCache, TeamPhotoCache>();
            services.AddSingleton<IUserProfilePictureCache, UserProfilePictureCache>();
            services.AddTransient<IUserProfileService, UserProfileService>();
            services.AddTransient<IMessageFactory, TeamsMessageFactory>();
            services.AddTransient<ITeamsMessageService, TeamsMessageService>();
            services.AddTransient<ITeamInfoService, TeamInfoService>();
            services.AddTransient<IDeepLinkCreator, DeepLinkCreator>();
            return services;
        }
    }
}