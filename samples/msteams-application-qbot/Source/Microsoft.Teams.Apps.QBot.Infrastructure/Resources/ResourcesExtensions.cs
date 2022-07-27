namespace Microsoft.Teams.Apps.QBot.Infrastructure.Resources
{
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Resources extensions.
    /// </summary>
    public static class ResourcesExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Add Localization.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddResources(this IServiceCollection services, IConfiguration configuration)
        {
            // Add localization.
            services.AddLocalization();

            // Configure localization options.
            var defaultCulture = configuration.GetValue<string>("Resources:defaultCulture", "en-US");
            services.Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo(defaultCulture),
                    };

                    options.DefaultRequestCulture = new AspNetCore.Localization.RequestCulture(culture: defaultCulture, uiCulture: defaultCulture);
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });

            return services;
        }
    }
}
