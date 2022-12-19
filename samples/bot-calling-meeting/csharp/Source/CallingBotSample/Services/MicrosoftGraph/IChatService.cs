// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Graph;

namespace CallingBotSample.Services.MicrosoftGraph
{
    public interface IChatService
    {
        /// <summary>
        /// Install a Teams app to a chat
        /// </summary>
        /// <param name="chatId">The chat Id to install the app to</param>
        /// <param name="teamsCatalogAppId">
        /// The app Id to install to the chat.
        /// This should be the external app id defined in Teams Admin Center or Platform Center.
        /// You need to upload your app to either your org or Teams app store to test this API
        /// </param>
        /// <returns></returns>
        Task<TeamsAppInstallation> InstallApp(string chatId, string teamsCatalogAppId);
    }
}
