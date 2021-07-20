// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using TabWithAdpativeCardFlow.Models;

namespace TabWithAdpativeCardFlow.Bots
{
    /// <summary>
    /// Bot Activity handler class
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _connectionName;

        private readonly OAuthClient oAuthClient;

        public ActivityBot(IConfiguration configuration, OAuthClient oAuthClient)
        {
            this.oAuthClient = oAuthClient ?? throw new ArgumentNullException(nameof(oAuthClient));
            _connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
        }

        /// <summary>
        /// Invoked when an invoke activity is received from the connector. Invoke activities can be used to communicate many different things.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var state = turnContext.TurnState.ToString(); // Check the state value

            var token = await this.oAuthClient.UserToken.GetAadTokensAsync(turnContext.Activity.From.Id, _connectionName, new Microsoft.Bot.Schema.AadResourceUrls { ResourceUrls = new string[] { "https://graph.microsoft.com/" } });
            var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
                // There is no token, so the user has not signed in yet.

                // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken);

                return CreateInvokeResponse(new TabResponse
                {
                    Tab = new TabResponsePayload
                    {
                        Type = "auth",
                        SuggestedActions = new TabSuggestedActions
                        {
                            Actions = new List<CardAction>
                            {
                                new CardAction
                                {
                                    Type = ActionTypes.OpenUrl,
                                    Value = signInLink,
                                    Title = "Sign in to this app",
                                },
                            },
                        },
                    },
                });
            }

            var client = new SimpleGraphClient(tokenResponse.Token);
            var profile = await client.GetMyProfile();

            if (turnContext.Activity.Name == "tab/fetch")
            {
                return CreateInvokeResponse(new TabResponse
                {
                    Tab = new TabResponsePayload
                    {
                        Type = "continue",
                        Value = new TabResponseCards
                        {
                            Cards = new List<TabResponseCard>
                            {
                                new TabResponseCard
                                {
                                    Card = GetAdaptiveCard1()
                                },
                                new TabResponseCard
                                {
                                    Card = GetAdaptiveCard2()
                                },
                            },
                        },
                    },
                });
            }
            else if (turnContext.Activity.Name == "tab/submit")
            {
                return CreateInvokeResponse(new TabResponse
                {
                    Tab = new TabResponsePayload
                    {
                        Type = "continue",
                        Value = new TabResponseCards
                        {
                            Cards = new List<TabResponseCard>
                            {
                                new TabResponseCard
                                {
                                    Card = GetAdaptiveCard2()
                                },
                            },
                        },
                    },
                });
            }
            else if (turnContext.Activity.Name == "task/fetch")
            {
                return CreateInvokeResponse(new TaskModuleResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Type = "continue",
                        Value = new TaskModuleTaskInfo()
                        {
                            Card = GetAdaptiveCardForTaskModule(),
                            Height = 250,
                            Width = 400,
                            Title = "Sample Adaptive Card",
                        },
                    },
                });
            }

            return null;
        }

        /// <summary>
        /// Sample Adaptive card
        /// </summary>
        /// <returns></returns>
        private AdaptiveCard GetAdaptiveCard1()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Hello:",
                        Size = AdaptiveTextSize.Medium,
                        Weight = AdaptiveTextWeight.Bolder,
                    },
                    new AdaptiveImage
                    {
                        Url = new Uri("https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg"),
                        AltText = "AlternativeText",
                        PixelHeight = 50,
                        PixelWidth = 50,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Hide Action card",
                    },

                    new AdaptiveSubmitAction
                    {
                        Title = "Show Task Module",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                        },
                    },
                },
            };

            return card;
        }

        /// <summary>
        /// Sample Adaptive card
        /// </summary>
        /// <returns></returns>
        private AdaptiveCard GetAdaptiveCard2()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveImage
                    {
                        Url = new Uri("https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg"),
                        AltText = "AlternativeText",
                        PixelHeight = 300,
                        PixelWidth = 400,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "tab/fetch is the first invoke request that your bot receives when a user opens an Adaptive Card tab. When your bot receives the request, it either sends a tab continue response or a tab auth response",
                        Wrap = true,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "tab/submit request is triggered to your bot with the corresponding data through the Action.Submit function of Adaptive Card",
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true,
                    },
                },

                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Click to submit",
                    },
                },
            };

            return card;
        }

        /// <summary>
        /// Sample Adaptive card for Task module
        /// </summary>
        /// <returns></returns>
        private Attachment GetAdaptiveCardForTaskModule()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Hello:",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveImage
                    {
                        Url = new Uri("https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg"),
                        AltText = "AlternativeText",
                        PixelHeight = 50,
                        PixelWidth = 50,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken).ConfigureAwait(false);
            return resource.SignInLink;
        }

        private async Task<TokenResponse> GetTokenResponse(ITurnContext<IInvokeActivity> turnContext, string state, CancellationToken cancellationToken)
        {
            var magicCode = string.Empty;

            if (!string.IsNullOrEmpty(state))
            {
                if (int.TryParse(state, out var parsed))
                {
                    magicCode = parsed.ToString();
                }
            }

            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var tokenResponse = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
            return tokenResponse;
        }
    }
}
