// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

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
using TabWithAdpativeCardFlow.Helpers;
using Newtonsoft.Json;
using TabWithAdpativeCardFlow.Models;
using Newtonsoft.Json.Linq;

namespace TabWithAdpativeCardFlow.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _connectionName;
        private readonly string _applicationBaseUrl;

        public ActivityBot(IConfiguration configuration)
        {
            _connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        /// <summary>
        /// Invoked when an fetch activity is recieved for tab.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="tabRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Tab response.</returns>
        protected override async Task<TabResponse> OnTeamsTabFetchAsync(ITurnContext<IInvokeActivity> turnContext, TabRequest tabRequest, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            if (tabRequest.TabEntityContext.TabEntityId == "homeTab")
            {

                // Check the state value
                var state = JsonConvert.DeserializeObject<AdaptiveCardAction>(turnContext.Activity.Value.ToString());
                var tokenResponse = await GetTokenResponse(turnContext, state.State, cancellationToken);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.
                    var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken);

                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var signInLink = resource.SignInLink;

                    return new TabResponse
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
                    };
                }

                var client = new SimpleGraphClient(tokenResponse.Token);
                var profile = await client.GetUserProfile();
                var userPhoto = await client.GetPublicURLForProfilePhoto(_applicationBaseUrl);

                return new TabResponse
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
                                    Card = CardHelper.GetSampleAdaptiveCard1(userPhoto, profile.DisplayName)
                                },
                                new TabResponseCard
                                {
                                    Card = CardHelper.GetSampleAdaptiveCard2()
                                },
                            },
                        },
                    },
                };
            }
            else
            {
                return new TabResponse
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
                                    Card = CardHelper.GetSampleAdaptiveCard3()
                                },
                            },
                        },
                    },
                };
            }
        }

        /// <summary>
        /// Invoked when an submit activity is recieved for tab.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="tabSubmit"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Tab response.</returns>
        protected async override Task<TabResponse> OnTeamsTabSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TabSubmit tabSubmit, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                await userTokenClient.SignOutUserAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, cancellationToken);

                return new TabResponse
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
                                    Card = CardHelper.GetSignOutCard()
                                },
                            },
                        },
                    },
                };
        }

        /// <summary>
        /// Handle task module is fetch.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name = "taskModuleRequest" >The task module invoke request value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var taskInfo = new TaskModuleResponse();
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var buttonType = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.Id;

            if(buttonType == "youTubeVideo")
            {
                var videoId = asJobject.GetValue("youTubeVideoId")?.ToString();
                taskInfo.Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = "https://www.youtube.com/embed/"+ videoId,
                        Height = 1000,
                        Width = 700,
                        Title = "Youtube video",
                    },
                };
            }
            else
            {
                taskInfo.Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Card = CardHelper.GetAdaptiveCardForTaskModule(),
                        Height = 200,
                        Width = 350,
                        Title = "Sample Adaptive Card",
                    },
                };
            }
            
            return Task.FromResult(taskInfo);
        }

        /// <summary>
        /// Handle task module is submit.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name = "taskModuleRequest" >The task module invoke request value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var taskInfo = new TaskModuleResponse();
            taskInfo.Task = new TaskModuleContinueResponse
            {
                Type = "continue",
                Value = new TaskModuleTaskInfo()
                {
                    Card = CardHelper.GetTaskModuleSubmitCard(),
                    Height = 200,
                    Width = 350,
                    Title = "Sample Adaptive Card",
                },
            };

            return Task.FromResult(taskInfo);
        }

        /// <summary>
        /// Get token response on basis of state.
        /// </summary>
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