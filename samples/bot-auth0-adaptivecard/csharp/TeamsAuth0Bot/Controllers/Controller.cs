// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using TeamsAuth0Bot.Services;

namespace TeamsAuth0Bot.Controllers
{
    [TeamsController]
    public class Controller
    {
        private readonly ConfigOptions _config;
        private readonly TokenStore _authToken;
        private readonly IHttpClientFactory _clientFactory;

        public Controller(ConfigOptions config, TokenStore tokenStore, IHttpClientFactory clientFactory)
        {
            _config = config;
            _authToken = tokenStore;
            _clientFactory = clientFactory;
        }

        // Handles incoming messages, manages authentication, and displays user profile or login card
        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            var userId = activity.From.Id;
            var text = activity.Text?.Trim().ToLowerInvariant();

            log.Info($"[CONFIG] Teams ClientId: {_config.Teams?.ClientId ?? "NULL"}");
            log.Info($"[CONFIG] Auth0 Domain: {_config.Auth0?.Domain ?? "NULL"}");

            if (text == "logout")
            {
                _authToken.RemoveToken(userId);

                var logoutUrl = $"https://{_config.Auth0.Domain}/v2/logout?client_id={_config.Auth0.ClientId}";

                var card = CreateHeroCard("You've been logged out.", "Logout from Auth0", logoutUrl);

                await client.Send(card);
                return;
            }

            if (_authToken.TryGetToken(userId, out var accessToken))
            {
                if (text == "profile details")
                {
                    var httpClient = _clientFactory.CreateClient();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var userInfoEndpoint = $"https://{_config.Auth0.Domain}/userinfo";
                    var response = await httpClient.GetAsync(userInfoEndpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var profileData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);

                        var card = new AdaptiveCard
                        {
                            Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                            Body = new List<CardElement>
                            {
                                new TextBlock("Auth0 Profile")
                                {
                                    Size = TextSize.Large,
                                    Weight = TextWeight.Bolder,
                                    Wrap = true
                                },
                                new Image(profileData["picture"].GetString())
                                {
                                    Size = Size.Medium,
                                    Style = ImageStyle.Person
                                },
                                new TextBlock($"Name: {profileData["name"].GetString()}")
                                {
                                    Wrap = true
                                },
                                new TextBlock($"Email: {profileData["email"].GetString()}")
                                {
                                    Wrap = true
                                }
                            }
                        };

                        await client.Send(card);
                        await client.Send($"Your profile Data: {JsonSerializer.Serialize(profileData)}");
                    }
                    else
                    {
                        await client.Send("Failed to fetch profile details.");
                    }
                }
                else
                {
                    await client.Send("Say 'profile details' to get your Auth0 profile or 'logout' to log out.");
                }
            }
            else
            {
                var loginUrl = GenerateLoginUrl(userId);

                var card = CreateHeroCard("Login Required", "Login", loginUrl);

                await client.Send(card);
            }
        }

        // Welcomes new members when they are added to the conversation
        [Conversation.MembersAdded]
        public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            var welcomeText = "Type anything to get a login card";
            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient.Id)
                {
                    await context.Send(welcomeText);
                }
            }
        }

        // Generates Auth0 authorization URL with callback and user state
        private string GenerateLoginUrl(string userId)
        {
            var domain = _config.Auth0.Domain;
            if (!domain.StartsWith("https://"))
            {
                domain = "https://" + domain;
            }

            var authUrl = $"{domain}/authorize" +
                          $"?response_type=code&client_id={_config.Auth0.ClientId}" +
                          $"&redirect_uri={_config.ApplicationUrl}/api/auth/callback" +
                          $"&scope=openid profile email" +
                          $"&state={HttpUtility.UrlEncode(userId)}";

            return authUrl;
        }

        // Creates an adaptive card with a title and action button
        private static AdaptiveCard CreateHeroCard(string title, string buttonTitle, string buttonUrl)
        {
            return new AdaptiveCard
            {
                Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                Body = new List<CardElement>
                {
                    new TextBlock(title)
                    {
                        Size = TextSize.Large,
                        Weight = TextWeight.Bolder,
                        Wrap = true
                    }
                },
                Actions = new List<Microsoft.Teams.Cards.Action>
                {
                    new OpenUrlAction(buttonUrl)
                    {
                        Title = buttonTitle
                    }
                }
            };
        }

        // Removes carriage return and newline characters for safe logging
        private static string SanitizeForLog(string input)
        {
            if (input == null) return "";
            return input.Replace("\r", "").Replace("\n", "");
        }
    }
}