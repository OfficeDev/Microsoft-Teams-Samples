// <copyright file="GraphClientFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    /// <summary>
    /// Graph service client factory implementation.
    /// </summary>
    internal class GraphClientFactory : IGraphClientFactory
    {
        /// <inheritdoc/>
        public GraphServiceClient GetAuthenticatedGraphClient(string accessToken)
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider(
              async (request) =>
              {
                  // Add auth header to outgoing request.
                  request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
                  await Task.CompletedTask;
              }));
        }
    }
}
