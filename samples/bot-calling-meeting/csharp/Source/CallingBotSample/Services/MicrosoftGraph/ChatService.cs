// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.ExternalConnectors;

namespace CallingBotSample.Services.MicrosoftGraph
{
    public class ChatService : IChatService
    {
        private readonly GraphServiceClient graphServiceClient;

        public ChatService(GraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient;
        }

        /// <inheritdoc/>
        public Task<TeamsAppInstallation> InstallApp(string chatId, string teamsCatalogAppId)
        {
            var teamsAppInstallation = new TeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    {"teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{teamsCatalogAppId}"}
                }
            };

            return graphServiceClient.Chats[chatId].InstalledApps
                .Request()
                .AddAsync(teamsAppInstallation);
        }
    }
}
