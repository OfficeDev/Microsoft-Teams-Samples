// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Cards;

namespace SuggestedActions.Controllers
{
    [TeamsController]
    public class Controller
    {
        private readonly IConfiguration _configuration;

        public Controller(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Message received");
            
            var text = activity.Text?.ToLowerInvariant() ?? string.Empty;
            var responseText = ProcessInput(text);

            await client.Typing();
            await client.Send(responseText);
            await SendSuggestedActionsCardAsync(client, log);
        }

        [Microsoft.Teams.Apps.Activities.Conversation.MembersAdded]
        public async Task OnMembersAdded([Context] ConversationUpdateActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            foreach (var member in activity.MembersAdded)
            {
                if (member.Id != activity.Recipient.Id)
                {
                    await client.Send("Welcome to the suggested actions bot. This bot will introduce you to suggested actions. Please answer the question:");
                    await SendSuggestedActionsCardAsync(client, log);
                }
            }
        }

        /// <summary>
        /// Processes the user's input and returns the appropriate response text.
        /// </summary>
        /// <param name="text">The user's input text.</param>
        /// <returns>The response text based on the user's input.</returns>
        private static string ProcessInput(string text)
        {
            const string colorText = "How can I assist you today?";
            return text switch
            {
                "hello" => $"Hello, {colorText}",
                "welcome" => $"Welcome, {colorText}",
                _ => "Please select one action",
            };
        }

        /// <summary>
        /// Creates and sends a message with suggested actions to the user.
        /// Uses native Teams SDK SuggestedActions with IMBack action type.
        /// When the user clicks a button, the text value will be displayed in the channel as if the user typed it,
        /// and will automatically trigger the OnMessage handler.
        /// </summary>
        private async Task SendSuggestedActionsCardAsync(IContext.Client client, Microsoft.Teams.Common.Logging.ILogger? log)
        {
            log?.Info("Sending suggested actions");

            // Create MessageActivity with SuggestedActions using native Teams SDK
            var message = new MessageActivity()
            {
                Text = "Choose one of the action from the suggested action?",
                SuggestedActions = new Microsoft.Teams.Api.SuggestedActions()
                    .AddAction(new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack)
                    {
                        Title = "Hello",
                        Value = "Hello"
                    })
                    .AddAction(new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack)
                    {
                        Title = "Welcome",
                        Value = "Welcome"
                    })
                    .AddAction(new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack)
                    {
                        Title = "@SuggestedActionsBot",
                        Value = "@SuggestedActionsBot"
                    })
            };

            await client.Send(message);
        }
    }
}