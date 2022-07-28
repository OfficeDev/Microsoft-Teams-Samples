// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GraphTeamsTag.Helper
{
    using GraphTeamsTag.Provider;
    using Microsoft.Graph;

    public class GraphHelper
    {
        /// <summary>
        /// Creates graph client to call Graph Beta API.
        /// </summary>
        public readonly GraphServiceClient graphBetaClient;

        public GraphHelper (SimpleBetaGraphClient simpleBetaGraphClient)
        {
            this.graphBetaClient = simpleBetaGraphClient.GetGraphClientforApp();
        }

        public async Task<ITeamTagsCollectionPage> ListTeamworkTagsAsync (string teamId)
        {
            var tags = await this.graphBetaClient.Teams[teamId].Tags.Request().GetAsync();

            return tags;
        }
    }
}
