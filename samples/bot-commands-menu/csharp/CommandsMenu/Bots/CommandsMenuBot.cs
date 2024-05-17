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
                    await SearchFlightsReaderCardAsync(turnContext, cancellationToken);
                else if (text.Contains("search hotels"))
                    await SearchHotelsReaderCardAsync(turnContext, cancellationToken);
                else if (text.Contains("help"))
                    await turnContext.SendActivityAsync(MessageFactory.Text("Displays this help message."));
                else if (text.Contains("best time to fly"))
                    await turnContext.SendActivityAsync(MessageFactory.Text("Best time to fly to London for a 5 day trip this summer."));
            }
            else if (turnContext.Activity.Value != null)
            {
                // Respond with hotel search details if the activity has a value (e.g., from an adaptive card)
                await turnContext.SendActivityAsync(MessageFactory.Text("Hotel search details are: " + turnContext.Activity.Value), cancellationToken);
            }
        }

        /// <summary>
        /// Sends an adaptive card for searching flights.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task SearchFlightsReaderCardAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Read the adaptive card JSON template for flight details
            var cardJSON = File.ReadAllText(_flightsDetailsCardTemplate);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            // Send the adaptive card as an attachment
            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
        }

        /// <summary>
        /// Sends an adaptive card for searching hotels.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task SearchHotelsReaderCardAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Read the adaptive card JSON template for hotel search
            var cardJSON = File.ReadAllText(_searchHotelsCardTemplate);
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