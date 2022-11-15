// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Services.BotFramework;

using Microsoft.Bot.Schema;

public interface IBotService
{
    Task<ResourceResponse> SendToConversation(string message, string conversationId);

    Task<ResourceResponse> SendToConversation(Attachment attachment, string conversationId);
}
