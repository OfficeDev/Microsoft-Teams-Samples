using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace bot_commands_menu.Controllers
{
    [TeamsController]
    public class Controller()
    {
        // Paths to the adaptive card JSON templates
        private readonly string _flightsDetailsCardTemplate = Path.Combine(".", "Resources", "flightsDetails.json");
        private readonly string _searchHotelsCardTemplate = Path.Combine(".", "Resources", "searchHotels.json");

        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Message received from user");

            // Remove bot mention from the message text if present
            var text = RemoveBotMention(activity.Text ?? "");

            if (!string.IsNullOrEmpty(text))
            {
                // Normalize the message text
                var normalizedText = text.Trim().ToLower();

                // Check if the message contains specific keywords and respond accordingly
                if (normalizedText.Contains("search flights"))
                {
                    log.Info("Sending flight search card");
                    await SendAdaptiveCardAsync(client, _flightsDetailsCardTemplate);
                }
                else if (normalizedText.Contains("search hotels"))
                {
                    log.Info("Sending hotel search card");
                    await SendAdaptiveCardAsync(client, _searchHotelsCardTemplate);
                }
                else if (normalizedText.Contains("help"))
                {
                    await client.Send("Displays this help message.\n\nAvailable commands:\n- **search flights** - Search for flight details\n- **search hotels** - Search for hotels\n- **best time to fly** - Get travel recommendations\n- **help** - Display this help message");
                }
                else if (normalizedText.Contains("best time to fly"))
                {
                    await client.Send("Best time to fly to London for a 5 day trip is summer.");
                }
                else
                {
                    await client.Send($"You said '{text}'\n\nTry these commands:\n- search flights\n- search hotels\n- best time to fly\n- help");
                }
            }
            else if (activity.Value != null)
            {
                // Handle adaptive card submission
                log.Info("Handling adaptive card submission");
                var submittedData = JsonSerializer.Serialize(activity.Value, new JsonSerializerOptions { WriteIndented = true });
                await client.Send($"Hotel search details are:\n```\n{submittedData}\n```");
            }
        }

        [Conversation.MembersAdded]
        public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            var welcomeText = "Welcome! How can I help you today?\n\n" +
                             "Try these commands:\n" +
                             "- **search flights** - Search for flight details\n" +
                             "- **search hotels** - Search for hotels\n" +
                             "- **best time to fly** - Get travel recommendations\n" +
                             "- **help** - Display help message";

            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient.Id)
                {
                    await context.Send(welcomeText);
                }
            }
        }

        /// <summary>
        /// Helper method to send an adaptive card.
        /// </summary>
        private async Task SendAdaptiveCardAsync(IContext.Client client, string cardTemplatePath)
        {
            try
            {
                // Read the JSON template
                var cardJSON = await File.ReadAllTextAsync(cardTemplatePath);

                // Deserialize the card JSON into an AdaptiveCard using the provided static method
                var adaptiveCard = AdaptiveCard.Deserialize(cardJSON);

                if (adaptiveCard == null)
                {
                    await client.Send("Error: Failed to deserialize adaptive card.");
                    return;
                }

                // Send the adaptive card using the correct overload
                await client.Send(adaptiveCard);
            }
            catch (Exception ex)
            {
                await client.Send($"Error sending card: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes bot mention from the message text.
        /// </summary>
        private string RemoveBotMention(string text)
        {
            // Remove @BotName mentions using regex
            var mentionRegex = new Regex(@"<at[^>]*>.*?</at>\s*", RegexOptions.IgnoreCase);
            return mentionRegex.Replace(text, "").Trim();
        }
    }
}