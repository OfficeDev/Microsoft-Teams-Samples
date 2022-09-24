namespace Microsoft.Teams.Apps.QBot.Infrastructure.QnAService
{
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;

    /// <summary>
    /// QnA Service extensions.
    /// </summary>
    public static class QnAServiceExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects concrete implementation of QnAService.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service Collection.</returns>
        public static IServiceCollection AddQnAService(this IServiceCollection services, IConfiguration configuration)
        {
            // Add QnAMakerClient
            var subscriptionKey = configuration.GetValue<string>("QnAMaker:SubscriptionKey");
            var endpoint = configuration.GetValue<string>("QnAMaker:Endpoint");
            var qnaMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = endpoint,
            };
            services.AddSingleton<IQnAMakerClient>(qnaMakerClient);

            // Add QnAMakerRuntimeClient
            var runtimeEndpoint = configuration.GetValue<string>("QnAMaker:RuntimeEndpoint");
            var endpointKey = configuration.GetValue<string>("QnAMaker:EndpointKey");
            var qnaMakerRuntimeClient = new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey))
            {
                RuntimeEndpoint = runtimeEndpoint,
            };
            services.AddSingleton<IQnAMakerRuntimeClient>(qnaMakerRuntimeClient);

            // Add QnAServiceSettings
            var qnaServiceSettings = new QnAServiceSettings()
            {
                ScoreThreshold = configuration.GetValue<int>("QnAMaker:ScoreThreshold"),
                KnowledgeBaseId = configuration.GetValue<string>("QnAMaker:KnowledgeBaseId"),
            };
            services.AddSingleton<IQnAServiceSettings>(qnaServiceSettings);

            // Add QnAService
            services.AddTransient<IQnAService, QnAService>();
            return services;
        }
    }
}
