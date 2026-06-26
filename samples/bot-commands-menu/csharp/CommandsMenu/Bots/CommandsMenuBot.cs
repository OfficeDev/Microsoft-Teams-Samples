using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CommandsMenu.Bots
{
    public class CommandsMenuBot : TeamsActivityHandler
    {
        // Paths to the adaptive card JSON templates
        private readonly string _flightsDetailsCardTemplate = Path.Combine(".", "Resources", "flightsDetails.json");
        private readonly string _searchHotelsCardTemplate = Path.Combine(".", "Resources", "searchHotels.json");

        /// <summary>
        /// Handles incoming message activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Remove the bot's mention from the message text
            turnContext.Activity.RemoveRecipientMention();

            if (turnContext.Activity.Text != null)
            {
                // Normalize the message text
                var text = turnContext.Activity.Text.Trim().ToLower();

                // Check if the message contains specific keywords and respond accordingly
                if (text.Contains("search flights"))
                    await SendAdaptiveCardAsync(turnContext, _flightsDetailsCardTemplate, cancellationToken);
                else if (text.Contains("search hotels"))
                    await SendAdaptiveCardAsync(turnContext, _searchHotelsCardTemplate, cancellationToken);
                else if (text.Contains("help"))
                    await turnContext.SendActivityAsync(MessageFactory.Text("Displays this help message."), cancellationToken);
                else if (text.Contains("best time to fly"))
                    await turnContext.SendActivityAsync(MessageFactory.Text("Best time to fly to London for a 5 day trip is summer."), cancellationToken);
            }
            else if (turnContext.Activity.Value != null)
            {
                // Respond with hotel search details if the activity has a value (e.g., from an adaptive card)
                await turnContext.SendActivityAsync(MessageFactory.Text("Hotel search details are: " + turnContext.Activity.Value), cancellationToken);
            }
        }

        /// <summary>
        /// Sends an adaptive card from the specified template file.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cardTemplatePath">The path to the adaptive card JSON template file.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task SendAdaptiveCardAsync(ITurnContext<IMessageActivity> turnContext, string cardTemplatePath, CancellationToken cancellationToken)
        {
            // Read the adaptive card JSON template asynchronously to improve performance
            var cardJSON = await File.ReadAllTextAsync(cardTemplatePath);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            // Send the adaptive card as an attachment
            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
        }
    }
}
