// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Catering.Cards;
using Catering.Models;
using Microsoft.Bot.AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace Catering
{
    public class CateringBot : ActivityHandler
    {
        private const string WelcomeText = "Welcome to the Adaptive Cards 2.0 Bot. This bot will introduce you to Action.Execute in Adaptive Cards.";

        private readonly UserState _userState;
        private readonly CateringDb _cateringDb;
        private readonly CateringRecognizer _cateringRecognizer;
        private readonly IConfiguration _configuration;

        public CateringBot(UserState userState, CateringDb cateringDb, CateringRecognizer cateringRecognizer, IConfiguration configuration)
        {
            _userState = userState;
            _cateringDb = cateringDb;
            _cateringRecognizer = cateringRecognizer;
            _configuration = configuration;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
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
            var text = turnContext.Activity.Text?.ToLowerInvariant();
            if (text?.Contains("recent") == true)
            {
                var latestOrders = await _cateringDb.GetRecentOrdersAsync();
                var users = latestOrders.Items ?? new List<User>();
                if (users.Count > 0)
                {
                    await SendHttpToTeams(HttpMethod.Post, MessageFactory.Attachment(new CardResource("RecentOrders.json").AsAttachment(
                        new
                        {
                            users = users.Select(u => new
                            {
                                lunch = new
                                {
                                    entre = string.IsNullOrEmpty(u.Lunch.Entre) ? "N/A" : u.Lunch.Entre,
                                    drink = string.IsNullOrEmpty(u.Lunch.Drink) ? "N/A" : u.Lunch.Drink,
                                    orderTimestamp = $"{u.Lunch.OrderTimestamp:t} {u.Lunch.OrderTimestamp:d}"
                                }
                            }).ToList()
                        })), turnContext.Activity.Conversation.Id);
                }
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
                    var request = AdaptiveCardInvokeValidator.ValidateRequest(turnContext);

                    if (request.Action.Verb == "order")
                    {
                        var cardOptions = AdaptiveCardInvokeValidator.ValidateAction<CardOptions>(request);
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

            return null!;
        }

        private async Task<Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse> ProcessOrderAction(User user, CardOptions cardOptions)
        {
            if ((Card)cardOptions.CurrentCard == Card.Entre)
            {
                if (!string.IsNullOrWhiteSpace(cardOptions.Custom))
                {
                    if (!await _cateringRecognizer.ValidateEntre(cardOptions.Custom))
                    {
                        return RedoEntreCardResponse(new Lunch() { Entre = cardOptions.Custom });
                    }
                    cardOptions.Option = cardOptions.Custom;
                }

                user.Lunch.Entre = cardOptions.Option;
            }
            else if ((Card)cardOptions.CurrentCard == Card.Drink)
            {
                if (!string.IsNullOrWhiteSpace(cardOptions.Custom))
                {
                    if (!await _cateringRecognizer.ValidateDrink(cardOptions.Custom))
                    {
                        return RedoDrinkCardResponse(new Lunch() { Drink = cardOptions.Custom });
                    }

                    cardOptions.Option = cardOptions.Custom;
                }

                user.Lunch.Drink = cardOptions.Option;
            }

            return (Card)cardOptions.NextCardToSend switch
            {
                Card.Drink => DrinkCardResponse(),
                Card.Entre => EntreCardResponse(),
                Card.Review => ReviewCardResponse(user),
                Card.ReviewAll => RecentOrdersCardResponse((await _cateringDb.GetRecentOrdersAsync()).Items),
                Card.Confirmation => ConfirmationCardResponse(await _cateringDb.UpsertOrderAsync(user)),
                _ => throw new NotImplementedException("No card matches that nextCardToSend.")
            };
        }

        private static InvokeResponse CreateInvokeResponse(HttpStatusCode statusCode, object? body = null)
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
                    await turnContext.SendActivityAsync("Type anything to see a card here, type email to send a card through Outlook, or type recents to see recent orders.");
                }
            }
        }

        #region Cards As InvokeResponses

        private Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse CardResponse(string cardFileName)
        {
            return new Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new CardResource(cardFileName).AsJObject()
            };
        }

        private Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse CardResponse<T>(string cardFileName, T data)
        {
            return new Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new CardResource(cardFileName).AsJObject(data)
            };
        }

        private Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse DrinkCardResponse()
        {
            return CardResponse("DrinkOptions.json");
        }

        private Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse EntreCardResponse()
        {
            return CardResponse("EntreOptions.json");
        }

        private Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse ReviewCardResponse(User user)
        {
            return CardResponse("ReviewOrder.json", user.Lunch);
        }

        private Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse RedoDrinkCardResponse(Lunch lunch)
        {
            return CardResponse("RedoDrinkOptions.json", lunch);
        }

        private Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse RedoEntreCardResponse(Lunch lunch)
        {
            return CardResponse("RedoEntreOptions.json", lunch);
        }

        private Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse RecentOrdersCardResponse(IList<User>? users)
        {
            var userList = users ?? new List<User>();
            return CardResponse("RecentOrders.json",
                new
                {
                    users = userList.Select(u => new
                    {
                        lunch = new
                        {
                            entre = string.IsNullOrEmpty(u.Lunch.Entre) ? "N/A" : u.Lunch.Entre,
                            drink = string.IsNullOrEmpty(u.Lunch.Drink) ? "N/A" : u.Lunch.Drink,
                            orderTimestamp = $"{u.Lunch.OrderTimestamp:t} {u.Lunch.OrderTimestamp:d}"
                        }
                    }).ToList()
                });
        }

        private Microsoft.Bot.AdaptiveCards.AdaptiveCardInvokeResponse ConfirmationCardResponse(User? user = null)
        {
            return CardResponse("Confirmation.json");
        }

        private async Task<string> GetAccessToken()
        {
            var app = ConfidentialClientApplicationBuilder
                .Create(_configuration["MicrosoftAppId"])
                .WithClientSecret(_configuration["MicrosoftAppPassword"])
                .WithAuthority(new Uri($"{_configuration["MicrosoftLoginUri"]}/botframework.com"))
                .Build();

            var authResult = await app.AcquireTokenForClient(new string[] { _configuration["BotFrameworkUri"] + ".default" }).ExecuteAsync();
            return authResult.AccessToken;
        }

        private async Task<string> SendHttpToTeams(HttpMethod method, IActivity activity, string convId, string? messageId = null)
        {
            var token = await GetAccessToken();
            var requestAsString = JsonConvert.SerializeObject(activity);

            var headers = new Dictionary<string, string>
            {
                { "User-Agent", "UniversalBot" },
                { "Authorization", $"Bearer {token}" }
            };

            var path = $"/conversations/{convId}/activities";
            string requestUri = _configuration["BotServiceUrl"] + path;

            var request = new HttpRequestMessage(method, requestUri);

            foreach (var entry in headers)
            {
                request.Headers.TryAddWithoutValidation(entry.Key, entry.Value);
            }

            request.Content = new StringContent(requestAsString, System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request);
            var payloadAsString = await response.Content.ReadAsStringAsync();
            var payload = JsonConvert.DeserializeObject<ResourceResponse>(payloadAsString);
            return payload?.Id ?? string.Empty;
        }

        #endregion
    }
}
