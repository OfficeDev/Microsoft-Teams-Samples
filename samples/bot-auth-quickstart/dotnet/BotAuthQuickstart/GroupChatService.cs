// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Models;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Extensions.Graph;

namespace BotAuthQuickstart;

public static class GroupChatService
{
    /// <summary>
    /// Handles the 'chats' command - lists group chats where the user is a member.
    /// Uses delegated permissions (Chat.Read) via the user's Graph client.
    /// </summary>
    public static async Task HandleChatsCommand(IContext<MessageActivity> context)
    {
        // Verify this is a personal scope
        if (context.Activity.Conversation?.Type != "personal")
        {
            Console.WriteLine("This command is only available in personal chat with the bot.");
            return;
        }

        Console.WriteLine("Fetching your group chats...");

        try
        {
            var graphClient = context.GetUserGraphClient();

            // Get all chats for the user
            var chats = await graphClient.Me.Chats.GetAsync();

            if (chats?.Value == null || chats.Value.Count == 0)
            {
                await context.Send("You don't have any chats.");
                return;
            }

            // Filter to only group chats
            var groupChats = chats.Value
                .Where(c => c.ChatType == ChatType.Group)
                .ToList();

            if (groupChats.Count == 0)
            {
                await context.Send("You are not a member of any group chats.");
                return;
            }

            var message = $"You are a member of {groupChats.Count} group chat(s):\n\n";

            foreach (var chat in groupChats)
            {
                var chatName = !string.IsNullOrEmpty(chat.Topic) ? chat.Topic : "Unnamed Group Chat";
                message += $"- {chatName}\n";
            }

            await context.Send(message);
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to fetch group chats. Please ensure you have the required permissions.");
        }
    }
}
