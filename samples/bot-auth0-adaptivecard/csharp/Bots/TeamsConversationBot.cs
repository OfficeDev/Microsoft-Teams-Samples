// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Collections.Concurrent;
using System.Collections;
using Microsoft.AspNetCore.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Web;
using static Microsoft.BotBuilderSamples.Controllers.AuthController;
using AdaptiveCards;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsConversationBot : TeamsActivityHandler
    {
        private string _appUrl;
        private string _authDomain;
        private string _authClientId;
        private readonly TokenStore _authToken;
        private readonly IHttpClientFactory _clientFactory;
        private static int _counter = 0;

        public TeamsConversationBot(IConfiguration config, IHttpClientFactory clientFactory, TokenStore tokenStore)
        {
            _authDomain = config["Auth0:Domain"];
            _authClientId = config["Auth0:ClientId"];
            _clientFactory = clientFactory;
            _appUrl = config["ApplicationUrl"];
            _authToken = tokenStore;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userId = turnContext.Activity.From.Id;
            var text = turnContext.Activity.Text?.Trim().ToLowerInvariant();

            if (text == "logout")
            {
                _authToken.RemoveToken(userId); // You need to implement this method in your token store

                // Optional: Auth0 logout URL (will clear Auth0 session too)
                var logoutUrl = $"https://{_authDomain}/v2/logout?client_id={_authClientId}";

                var card = new HeroCard
                {
                    Title = "You've been logged out.",
                    Buttons = new List<CardAction>
                        {
                            new CardAction(ActionTypes.OpenUrl, "Logout from Auth0", value: logoutUrl)
                        }
                };

                await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
                return;
            }

            if (_authToken.TryGetToken(userId, out var accessToken))
            {
                if (text == "profile details")
                {
                    var client = _clientFactory.CreateClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var userInfoEndpoint = $"https://{_authDomain}/userinfo";
                    var response = await client.GetAsync(userInfoEndpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var profileData = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                        var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3))
                        {
                            Body = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock("Auth0 Profile")
                                {
                                    Size = AdaptiveTextSize.Large,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Wrap = true
                                },
                                new AdaptiveImage(profileData["picture"].ToString())
                                {
                                    Size = AdaptiveImageSize.Medium,
                                    Style = AdaptiveImageStyle.Person
                                },
                                new AdaptiveTextBlock($"Name: {profileData["name"]}")
                                {
                                    Wrap = true,
                                    Weight = AdaptiveTextWeight.Default
                                },
                                new AdaptiveTextBlock($"Email: {profileData["email"]}")
                                {
                                    Wrap = true,
                                    Weight = AdaptiveTextWeight.Default
                                }
                            }
                        };

                        var attachment = new Attachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = card
                        };

                        await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);

                        await turnContext.SendActivityAsync(MessageFactory.Text($"Your profile Data: {JsonSerializer.Serialize(profileData)}"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Failed to fetch profile details."), cancellationToken);
                    }
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Say 'profile details' to get your Auth0 profile or 'logout' to log out."), cancellationToken);
                }
            }
            else
            {
                // No token yet
                var loginUrl = GenerateLoginUrl(userId);

                var card = new HeroCard
                {
                    Title = "Login Required",
                    Buttons = new List<CardAction>
            {
                new CardAction(ActionTypes.OpenUrl, "Login", value: loginUrl)
            }
                };

                await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
            }
        }

        private string GenerateLoginUrl(string userId)
        {
            var domain = _authDomain;
            if (!domain.StartsWith("https://"))
            {
                domain = "https://" + domain;
            }

            var authUrl = $"{domain}/authorize" +
                          $"?response_type=code&client_id={_authClientId}" +
                          $"&redirect_uri={_appUrl}/api/auth/callback" +
                          $"&scope=openid profile email" +
                          $"&state={HttpUtility.UrlEncode(userId)}";

            return authUrl;
        }
    }
}