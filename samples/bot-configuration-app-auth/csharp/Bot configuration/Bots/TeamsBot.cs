using AdaptiveCards;
using Bogus.DataSets;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Botconfiguration.Bots
{
    /// <summary>
    /// TeamsBot class handles bot activities for Microsoft Teams.
    /// </summary>
    public class TeamsBot : TeamsActivityHandler
    {
        private string _chosenFlow = string.Empty;

        /// <summary>
        /// Handles the event when members are added to the channel.
        /// </summary>
        /// <param name="membersAdded">List of members added.</param>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            const string imagePath = "Images/configbutton.png";
            var imageData = Convert.ToBase64String(await File.ReadAllBytesAsync(imagePath));

            var adaptiveCardJson = $@"
                {{
                    ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                    ""type"": ""AdaptiveCard"",
                    ""version"": ""1.0"",
                    ""body"": [
                        {{
                            ""type"": ""TextBlock"",
                            ""text"": ""Hello and welcome! With this sample, you can experience the functionality of bot configuration. To access Bot configuration, click on the settings button in the bot description card."",
                            ""wrap"": true,
                            ""size"": ""large"",
                            ""weight"": ""bolder""
                        }},
                        {{
                            ""type"": ""Image"",
                            ""url"": ""data:image/png;base64,{imageData}"",
                            ""size"": ""auto""
                        }}
                    ],
                    ""fallbackText"": ""This card requires Adaptive Card support.""
                }}";

            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson)
            };

            var reply = MessageFactory.Attachment(attachment);
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Handles the event when a message activity is received.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (!string.IsNullOrEmpty(activity.Text))
            {
                var text = activity.Text.ToLower().Trim();
                if (text == "chosen flow" || text == "<at>typeahead search adaptive card</at> chosen flow")
                {
                    await turnContext.SendActivityAsync($"Bot configured for {_chosenFlow} flow", cancellationToken: cancellationToken);
                }
            }
            else if (activity.Value != null)
            {
                await turnContext.SendActivityAsync($"Selected option is: {activity.Value}", cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Handles the event when a configuration fetch is requested.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="configData">Configuration data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Configuration response.</returns>
        protected override Task<ConfigResponseBase> OnTeamsConfigFetchAsync(ITurnContext<IInvokeActivity> turnContext, JObject configData, CancellationToken cancellationToken)
        {
            var response = new ConfigResponse<BotConfigAuth>
            {
                Config = new BotConfigAuth
                {
                    SuggestedActions = new SuggestedActions
                    {
                        Actions = new List<CardAction>
                            {
                                new CardAction
                                {
                                    Type = ActionTypes.OpenUrl,
                                    Title = "Sign in to this app",
                                    Value = "https://example.com/auth"
                                }
                            }
                    },
                    Type = "auth"
                }
            };
            return Task.FromResult<ConfigResponseBase>(response);
        }

        /// <summary>
        /// Handles the event when a configuration submit is requested.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="configData">Configuration data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Configuration response.</returns>
        protected override Task<ConfigResponseBase> OnTeamsConfigSubmitAsync(ITurnContext<IInvokeActivity> turnContext, JObject configData, CancellationToken cancellationToken)
        {
            var response = new ConfigResponse<TaskModuleResponseBase>
            {
                Config = new TaskModuleMessageResponse
                {
                    Type = "message",
                    Value = "You have chosen to finish setting up bot"
                }
            };

            return Task.FromResult<ConfigResponseBase>(response);
        }
    }
}