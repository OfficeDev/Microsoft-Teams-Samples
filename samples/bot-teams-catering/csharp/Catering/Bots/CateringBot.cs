// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Catering.Cards;
using System.Net.Http;
using Catering.Models;
using System.Net;
using AdaptiveCards;
using Microsoft.Bot.AdaptiveCards;
using System.Linq;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;

namespace Catering
{
    // This bot will respond to the user's input with an Adaptive Card.
    // Adaptive Cards are a way for developers to exchange card content
    // in a common and consistent way. A simple open card format enables
    // an ecosystem of shared tooling, seamless integration between apps,
    // and native cross-platform performance on any device.
    // For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    // This is a Transient lifetime service. Transient lifetime services are created
    // each time they're requested. For each Activity received, a new instance of this
    // class is created. Objects that are expensive to construct, or have a lifetime
    // beyond the single turn, should be carefully managed.

    public class CateringBot : ActivityHandler 
    {
        private const string WelcomeText = "Welcome to the Adaptive Cards 2.0 Bot. This bot will introduce you to Action.Execute in Adaptive Cards.";
        private BotState _userState;
        private CateringDb _cateringDb;
        private readonly CateringRecognizer _cateringRecognizer;
        private readonly IConfiguration _configuration;

        public CateringBot(UserState userState, CateringDb cateringDb, CateringRecognizer cateringRecognizer, IConfiguration configuration)
        {
            _userState = userState;
            _cateringDb = cateringDb;
            _cateringRecognizer = cateringRecognizer;
            _configuration = configuration;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.ChannelId == "directline" || turnContext.Activity.ChannelId == "webchat")
            {
                await SendWelcomeMessageAsync(turnContext, cancellationToken);
            }
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text.ToLowerInvariant();
            if (text.Contains("recent"))
            {

                var latestOrders = await _cateringDb.GetRecentOrdersAsync();
                var users = latestOrders.Items;
                await SendHttpToTeams(HttpMethod.Post, MessageFactory.Attachment(new CardResource("RecentOrders.json").AsAttachment(
                            new
                            {
                                users = users.Select(u => new
                                {
                                    lunch = new
                                    {
                                        entre = String.IsNullOrEmpty(u.Lunch.Entre) ? "N/A" : u.Lunch.Entre,
                                        drink = String.IsNullOrEmpty(u.Lunch.Drink) ? "N/A" : u.Lunch.Drink,
                                        orderTimestamp = u.Lunch.OrderTimestamp.ToShortTimeString() + " " + u.Lunch.OrderTimestamp.ToShortDateString()
                                    }
                                }).ToList()
                            })), turnContext.Activity.Conversation.Id);

            }
            else
            {
                await SendHttpToTeams(HttpMethod.Post, MessageFactory.Attachment(new CardResource("EntreOptions.json").AsAttachment()), turnContext.Activity.Conversation.Id);
            }
        }


        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (AdaptiveCardInvokeValidator.IsAdaptiveCardAction(turnContext))
            {
                var userSA = _userState.CreateProperty<User>(nameof(User));
                var user = await userSA.GetAsync(turnContext, () => new User() { Id = turnContext.Activity.From.Id });

                try
                {
                    AdaptiveCardInvoke request = AdaptiveCardInvokeValidator.ValidateRequest(turnContext);

                    if (request.Action.Verb == "order")
                    {
                        var cardOptions = AdaptiveCardInvokeValidator.ValidateAction<CardOptions>(request);

                        // process action
                        var responseBody = await ProcessOrderAction(user, cardOptions);

                        return CreateInvokeResponse(responseBody);
                    }
                    else
                    {
                        AdaptiveCardActionException.VerbNotSupported(request.Action.Type);
                    }
                }
                catch (AdaptiveCardActionException e)
                {
                    return CreateInvokeResponse(HttpStatusCode.OK, e.Response);
                }
            }

            return null;
        }

        private async Task<AdaptiveCardInvokeResponse> ProcessOrderAction(User user, CardOptions cardOptions)
        {
            if ((Card)cardOptions.currentCard == Card.Entre)
            {
                if (!string.IsNullOrEmpty(cardOptions.custom))
                {
                    if (!await _cateringRecognizer.ValidateEntre(cardOptions.custom))
                    {
                        return RedoEntreCardResponse(new Lunch() { Entre = cardOptions.custom });
                    }
                    cardOptions.option = cardOptions.custom;
                }

                user.Lunch.Entre = cardOptions.option;
            }
            else if ((Card)cardOptions.currentCard == Card.Drink)
            {
                if (!string.IsNullOrEmpty(cardOptions.custom))
                {
                    if (!await _cateringRecognizer.ValidateDrink(cardOptions.custom))
                    {
                        return RedoDrinkCardResponse(new Lunch() { Drink = cardOptions.custom });
                    }

                    cardOptions.option = cardOptions.custom;
                }

                user.Lunch.Drink = cardOptions.option;
            }

            AdaptiveCardInvokeResponse responseBody = null;
            switch ((Card)cardOptions.nextCardToSend)
            {
                case Card.Drink:
                    responseBody = DrinkCardResponse();
                    break;
                case Card.Entre:
                    responseBody = EntreCardResponse();
                    break;
                case Card.Review:
                    responseBody = ReviewCardResponse(user);
                    break;
                case Card.ReviewAll:
                    var latestOrders = await _cateringDb.GetRecentOrdersAsync();
                    responseBody = RecentOrdersCardResponse(latestOrders.Items);
                    break;
                case Card.Confirmation:
                    await _cateringDb.UpsertOrderAsync(user);
                    responseBody = ConfirmationCardResponse();
                    break;
                default:
                    throw new NotImplementedException("No card matches that nextCardToSend.");
            }

            return responseBody;
        }

        private static InvokeResponse CreateInvokeResponse(HttpStatusCode statusCode, object body = null)
        {
            return new InvokeResponse()
            {
                Status = (int)statusCode,
                Body = body
            };
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var message = MessageFactory.Text(WelcomeText);
                    await turnContext.SendActivityAsync(message, cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync($"Type anything to see a card here, type email to send a card through Outlook, or type recents to see recent orders.");
                }
            }
        }

        #region Cards As InvokeResponses

        private AdaptiveCardInvokeResponse CardResponse(string cardFileName)
        {
            return new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new CardResource(cardFileName).AsJObject()
            };
        }

        private AdaptiveCardInvokeResponse CardResponse<T>(string cardFileName, T data)
        {
            return new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new CardResource(cardFileName).AsJObject(data)
            };
        }

        private AdaptiveCardInvokeResponse DrinkCardResponse()
        {
            return CardResponse("DrinkOptions.json");
        }

        private AdaptiveCardInvokeResponse EntreCardResponse()
        {
            return CardResponse("EntreOptions.json");
        }

        private AdaptiveCardInvokeResponse ReviewCardResponse(User user)
        {
            return CardResponse("ReviewOrder.json", user.Lunch);
        }

        private AdaptiveCardInvokeResponse RedoDrinkCardResponse(Lunch lunch)
        {
            return CardResponse("RedoDrinkOptions.json", lunch);
        }

        private AdaptiveCardInvokeResponse RedoEntreCardResponse(Lunch lunch)
        {
            return CardResponse("RedoEntreOptions.json", lunch);
        }

        private AdaptiveCardInvokeResponse RecentOrdersCardResponse(IList<User> users)
        {
            return CardResponse("RecentOrders.json", 
                new
                {
                    users = users.Select(u => new 
                    { 
                        lunch = new
                        {
                            entre = String.IsNullOrEmpty(u.Lunch.Entre) ? "N/A" : u.Lunch.Entre,
                            drink = String.IsNullOrEmpty(u.Lunch.Drink) ? "N/A" : u.Lunch.Drink,
                            orderTimestamp = u.Lunch.OrderTimestamp.ToShortTimeString() + " " + u.Lunch.OrderTimestamp.ToShortDateString()
                        }
                    }).ToList()
                });
        }

        private AdaptiveCardInvokeResponse ConfirmationCardResponse()
        {
            return CardResponse("Confirmation.json");
        }

        private async Task<string> GetAccessToken()
        {
            var app = ConfidentialClientApplicationBuilder.Create(_configuration["MicrosoftAppId"])
                       .WithClientSecret(_configuration["MicrosoftAppPassword"])
                       .WithAuthority(new System.Uri($"{_configuration["MicrosoftLoginUri"]}/{"botframework.com"}"))
                       .Build();

            var authResult = await app.AcquireTokenForClient(new string[] { _configuration["BotFrameworkUri"] + ".default" }).ExecuteAsync();
            return authResult.AccessToken;
        }

        private async Task<string> SendHttpToTeams(HttpMethod method, IActivity activity, string convId, string messageId = null)
        {
            var token = await GetAccessToken();
            var requestAsString = JsonConvert.SerializeObject(activity);
            
            var headers = new Dictionary<string, string>
            {
                { "User-Agent", "UniversalBot" },
                { "Authorization", $"Bearer {token}" }
            };

            var path = $"/conversations/{convId}/activities";

            // The Bot Service Url needs to be dynamically fetched (and stored) from the Team. Recommendation is to capture the serviceUrl from the bot Payload and later re-use it to send proactive messages.
            string requestUri = _configuration["BotServiceUrl"] + path;

            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> entry in headers)
                {
                    request.Headers.TryAddWithoutValidation(entry.Key, entry.Value);
                }
            }

            request.Content = new StringContent(requestAsString, System.Text.Encoding.UTF8, "application/json");

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.SendAsync(request);
            var payloadAsString = await response.Content.ReadAsStringAsync();
            var payload = JsonConvert.DeserializeObject<ResourceResponse>(payloadAsString);
            return payload.Id;
        }
        #endregion
    }
}