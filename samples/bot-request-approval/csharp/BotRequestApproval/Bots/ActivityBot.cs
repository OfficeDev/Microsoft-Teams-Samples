// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Templating;
using BotRequestApproval.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace BotRequestApproval.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        public readonly IConfiguration _configuration;
        protected readonly BotState _conversationState;
        protected readonly IStatePropertyAccessor<List<RequestDetails>> _RequestDetails;
        private readonly IWebHostEnvironment _env;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        public ActivityBot(IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences, ConversationState conversationState, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _conversationReferences = conversationReferences;
            _env = env;
            _conversationState = conversationState;
            _RequestDetails = conversationState.CreateProperty<List<RequestDetails>>(nameof(List<RequestDetails>));
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.AadObjectId, conversationReference, (key, newValue) => conversationReference);
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value!= null)
            {
                var asJobject = JObject.FromObject(turnContext.Activity.Value);
                var title = (string)asJobject.ToObject<AdaptiveCardAction<string>>()?.RequestTitle;
                var description = (string)asJobject.ToObject<AdaptiveCardAction<string>>()?.RequestDescription;
                var managerID = (string)asJobject.ToObject<AdaptiveCardAction<string>>()?.ManagerDetails;

                var cardPayload = this.GetCardPayload("RequestDetailsCardForUser.json");
                var template = new AdaptiveCardTemplate(cardPayload);
               
                var requestdetails = new RequestDetails()
                {
                    TaskId = Guid.NewGuid(),
                    Title = title,
                    Description = description,
                    UserName = turnContext.Activity.From.Name,
                    Status = "Pending",
                };

                var card = template.Expand(requestdetails);
                AdaptiveCard adaptiveCard = AdaptiveCard.FromJson(card).Card;
                var attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = adaptiveCard,
                };
                var usercardId = await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
                await SaveRequestDetailsAsync(turnContext, requestdetails);

                var conversationReference = new ConversationReference();

                _conversationReferences.TryGetValue(managerID, out conversationReference);
                // Send notification to all the members
                    if(conversationReference != null)
                    {
                        await turnContext.Adapter.ContinueConversationAsync(_configuration["MicrosoftAppId"], conversationReference, BotCallback, cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("Please install the app for manager.");
                    }
            }
            else
            {
                AddConversationReference(turnContext.Activity as Activity);
                var cardPayload = this.GetCardPayload("RequestCard.json");
                AdaptiveCard adaptiveCard = AdaptiveCard.FromJson(cardPayload).Card;
                var attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = adaptiveCard,
                };
                
                await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            }  
        }

        /// <summary>
        /// Handle request from bot.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Invoked when bot (like a user) are added to the conversation.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    AddConversationReference(turnContext.Activity as Activity);
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome! With this sample you can checkin your location (use command 'checkin') and view your checked in location(use command 'viewcheckin')."), cancellationToken);
                }
            }
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Proactive hello.");
        }

        // Save user details in json file.
        private async Task SaveRequestDetailsAsync(ITurnContext<IMessageActivity> turnContext, RequestDetails requestDetails)
        {
            var currentDetail = await _RequestDetails.GetAsync(turnContext, () => new List<RequestDetails>());
            List<RequestDetails> userRequestList = new List<RequestDetails>();
            if (currentDetail == null)
            {
                userRequestList.Add(requestDetails);
                currentDetail = userRequestList;
                await this._RequestDetails.SetAsync(turnContext, currentDetail);
            }
            else
            {
                userRequestList = currentDetail;
                userRequestList.Add(requestDetails);
                currentDetail = userRequestList;
                await _RequestDetails.SetAsync(turnContext, currentDetail);
            }
        }

        /// <summary>
        /// Get card payload from memory.
        /// </summary>
        /// <param name="cardCacheKey">Card cache key.</param>
        /// <param name="cardJSONTemplateFileName">File name of JSON adaptive card template with file extension as .json to be provided.</param>
        /// <returns>Returns json adaptive card payload string.</returns>
        private string GetCardPayload(string cardJSONTemplateFileName)
        {

                var cardJsonFilePath = Path.Combine(_env.ContentRootPath, $".\\Cards\\{cardJSONTemplateFileName}");
                var  cardPayload = File.ReadAllText(cardJsonFilePath);

            return cardPayload;
        }
    }
}