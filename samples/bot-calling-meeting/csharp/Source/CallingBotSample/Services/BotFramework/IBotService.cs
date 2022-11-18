// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace CallingBotSample.Services.BotFramework
{
    public interface IBotService
    {
        Task<ResourceResponse> SendToConversation(string message, string conversationId);

        Task<ResourceResponse> SendToConversation(Attachment attachment, string conversationId);
    }
}
