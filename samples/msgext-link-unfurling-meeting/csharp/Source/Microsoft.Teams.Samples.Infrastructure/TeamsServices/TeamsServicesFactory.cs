// <copyright file="TeamsServicesFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    using AutoMapper;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Services;

    /// <summary>
    /// Teams services factory implementation.
    /// </summary>
    internal class TeamsServicesFactory : ITeamsServicesFactory
    {
        private readonly IMapper mapper;
        private readonly IAppSettings appsettings;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsServicesFactory"/> class.
        /// </summary>
        /// <param name="mapper">Auto mapper.</param>
        /// <param name="appsettings">App settings.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public TeamsServicesFactory(
            IMapper mapper,
            IAppSettings appsettings,
            ILoggerFactory loggerFactory)
        {
            this.mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            this.appsettings = appsettings ?? throw new System.ArgumentNullException(nameof(appsettings));
            this.loggerFactory = loggerFactory ?? throw new System.ArgumentNullException(nameof(loggerFactory));
        }

        /// <inheritdoc/>
        public IConversationService GetConversationService(GraphServiceClient graphServiceClient)
        {
            return new ConversationService(graphServiceClient, this.appsettings, this.loggerFactory.CreateLogger<ConversationService>());
        }

        /// <inheritdoc/>
        public IMeetingsService GetMeetingsService(GraphServiceClient graphServiceClient)
        {
            return new MeetingsService(graphServiceClient, this.mapper);
        }
    }
}
