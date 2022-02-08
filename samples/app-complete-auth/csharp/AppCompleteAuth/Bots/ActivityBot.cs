// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using AppCompleteAuth.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppCompleteAuth.Bots
{
    public class ActivityBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        private readonly string _applicationBaseUrl;
        private readonly string _connectionName;

        public ActivityBot(IConfiguration configuration, ConversationState conversationState, T dialog)
        {
            _connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
            ConversationState = conversationState;
            Dialog = dialog;
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
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Handle when a message is addressed to the bot
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// For more information on bot messaging in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet#receive-a-message .
        /// </remarks>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                var userCommand = turnContext.Activity.Text.ToLower().Trim();

                if (userCommand == "sso" || userCommand == "logout" || userCommand == "otheridentityprovider" || userCommand == "usingcredentials")
                {
                    // Run the Dialog with the new message Activity.
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(GetLoginOptionCard()));
                }
            }

            return;
        }

        /// <summary>
        /// Invoked when the user askfor sign in.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(turnContext.Activity.Value);
            var state = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.State;

            if (state.ToString() == "CancelledByUser")
            {
                await turnContext.SendActivityAsync("Sign in cancelled by user");
            }
            else
            {
                var cred = JObject.Parse(state);
                var userName = (string)cred.ToObject<CardTaskFetchValue<string>>()?.UserName;
                var password = (string)cred.ToObject<CardTaskFetchValue<string>>()?.Password;
                if (userName == Constant.UserName && password == Constant.Password)
                {
                    await turnContext.SendActivityAsync("Authentication Successful");
                }
                else
                {
                    await turnContext.SendActivityAsync("Invalid username or password");
                }    
            } 
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(action.State))
            {
                JObject asJobject = JObject.Parse(action.State);
                var userName = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.UserName;
                var password = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.Password;
                if (userName == Constant.UserName && password == Constant.Password)
                {
                    await turnContext.SendActivityAsync("Authentication Successful");
                }
                else
                {
                    await turnContext.SendActivityAsync("Invalid username or password");
                }

                return new MessagingExtensionActionResponse();
            }
            else if (action.CommandId.ToUpper() == "SSO")
            {
                var state = action.State; // Check the state value
                var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.

                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);

                    return new MessagingExtensionActionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "auth",
                            SuggestedActions = new MessagingExtensionSuggestedAction
                            {
                                Actions = new List<CardAction>
                                {
                                    new CardAction
                                    {
                                        Type = ActionTypes.OpenUrl,
                                        Value = signInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                            },
                        },
                    };
                }

                var client = new SimpleGraphClient(tokenResponse.Token);

                var profile = await client.GetMeAsync();

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetProfileCard(profile),
                            Height = 250,
                            Width = 400,
                            Title = "Adaptive Card: Inputs",
                        },
                    },
                };
            }

            else if (action.CommandId.ToUpper() == "SIGNOUTCOMMAND")
            {
                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                await userTokenClient.SignOutUserAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, cancellationToken).ConfigureAwait(false);

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = new Microsoft.Bot.Schema.Attachment
                            {
                                Content = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
                                {
                                    Body = new List<AdaptiveElement>() { new AdaptiveTextBlock() { Text = "You have been signed out." } },
                                    Actions = new List<AdaptiveAction>() { new AdaptiveSubmitAction() { Title = "Close" } },
                                },
                                ContentType = AdaptiveCard.ContentType,
                            },
                            Height = 200,
                            Width = 400,
                            Title = "Adaptive Card: Inputs",
                        },
                    },
                };
            }

            else if (action.CommandId.ToUpper() == "USERCREDENTIALS")
            {
                return new MessagingExtensionActionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "config",
                        SuggestedActions = new MessagingExtensionSuggestedAction
                        {
                            Actions = new List<CardAction>
                            {
                                new CardAction
                                {
                                    Type = ActionTypes.OpenUrl,
                                    Value = $"{_applicationBaseUrl}/popUpSignin?from=msgext"
                                },
                            },
                        },
                    },
                };
            }
            else
            {
                return null;
            }
        }

        private static Microsoft.Bot.Schema.Attachment GetLoginOptionCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Login options",
                Text = "Select a login option",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack,title:"Using credentials", value: "usingcredentials", text:"usingcredentials", displayText:"Using credentials"),
                    new CardAction(ActionTypes.MessageBack,title:"Other identity provider", value: "otheridentityprovider", text:"otheridentityprovider", displayText:"Other identity provider"),
                    new CardAction(ActionTypes.MessageBack,title:"SSO authentication", value: "sso", text:"sso", displayText:"SSO authentication")
                }
            };

            return heroCard.ToAttachment();
        }

        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken).ConfigureAwait(false);
            return resource.SignInLink;
        }

        private static Microsoft.Bot.Schema.Attachment GetProfileCard(User profile)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Hello, {profile.DisplayName}",
                Size = AdaptiveTextSize.ExtraLarge
            });

            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri("http://adaptivecards.io/content/cats/1.png")
            });
            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
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