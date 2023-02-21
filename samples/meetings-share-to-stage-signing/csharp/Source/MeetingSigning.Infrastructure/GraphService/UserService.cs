// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.GraphService
{
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IServices;
    using User = Domain.Models.User;

    /// <inheritdoc/>
    public class UserService : GraphServiceHelper, IUserService
    {
        private readonly GraphServiceClient _graphServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="graphServiceClient">Graph service client.</param>
        /// <param name="logger">Logger.</param>
        public UserService(
            GraphServiceClient graphServiceClient,
            ILogger<UserService> logger) : base(logger)
        {
            _graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
        }

        /// <inheritdoc/>
        public async Task<User> GetUser(string userId)
        {
            try
            {
                var result = await _graphServiceClient.Users[userId]
                    .Request()
                    .Select("DisplayName,Mail")
                    .GetAsync();

                return new User { Name = result.DisplayName, UserId = userId, Email = result.Mail };
            }
            catch (Exception ex)
            {
                throw HandleGraphExceptions(ex);
            }
        }
    }
}
