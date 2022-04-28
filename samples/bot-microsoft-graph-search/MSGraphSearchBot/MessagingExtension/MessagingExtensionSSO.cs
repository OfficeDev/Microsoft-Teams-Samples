using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using MSGraphSearchSample.Constants;
using MSGraphSearchSample.Constants.MessagingExtension;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Bots
{
    public partial class Bot<T> : TeamsActivityHandler
        where T : Dialog
    {
        /// <summary>
        /// Handle Single sign-on process
        /// You don't need to change anything here
        /// </summary>
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            JObject valueObject = JObject.FromObject(turnContext.Activity.Value);
            if (valueObject["authentication"] != null)
            {
                JObject authenticationObject = JObject.FromObject(valueObject["authentication"]);
                if (authenticationObject["token"] != null)
                {
                    //If the token is NOT exchangeable, then return 412 to require user consent
                    if (await TokenIsExchangeable(turnContext, cancellationToken))
                    {
                        return await base.OnInvokeActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var response = new InvokeResponse();
                        response.Status = 412;
                        return response;
                    }
                }
            }
            return await base.OnInvokeActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
        }

        // Handle the authentication commands (Sign out, Show profile)
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            if (action.CommandId.ToUpper() == CommandIds.ShowProfile)
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
                _graphService.SetAccessToken(tokenResponse.Token);

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetProfileCard("USERNAME"),
                            Height = 250,
                            Width = 400,
                            Title = "Adaptive Card: Inputs",
                        },
                    },
                };
            }

            if (action.CommandId.ToUpper() == CommandIds.SignOutCommand)
            {
                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                await userTokenClient.SignOutUserAsync(turnContext.Activity.From.Id, _appConfig.ConnectionName, turnContext.Activity.ChannelId, cancellationToken).ConfigureAwait(false);

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = new Attachment
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
            return null;
        }

        private async Task<bool> TokenIsExchangeable(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            TokenResponse tokenExchangeResponse = null;
            try
            {
                JObject valueObject = JObject.FromObject(turnContext.Activity.Value);
                var tokenExchangeRequest =
                ((JObject)valueObject["authentication"])?.ToObject<TokenExchangeInvokeRequest>();
                tokenExchangeResponse = await (turnContext.Adapter as IExtendedUserTokenProvider).ExchangeTokenAsync(
                 turnContext,
                 _appConfig.ConnectionName,
                 turnContext.Activity.From.Id,
                 new TokenExchangeRequest
                 {
                     Token = tokenExchangeRequest.Token,
                 },
                 cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                //ignore exceptions
                //if token exchange failed for any reason, tokenExchangeResponse above remains null, and a failure invoke response is sent to the caller.
                //This ensures the caller knows that the invoke has failed.
            }
            if (tokenExchangeResponse == null || string.IsNullOrEmpty(tokenExchangeResponse.Token))
            {
                return false;
            }
            return true;
        }

        // Get the sign link from the state
        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(_appConfig.ConnectionName, turnContext.Activity, null, cancellationToken).ConfigureAwait(false);
            return resource.SignInLink;
        }

        // Get profile adaptive card
        private static Attachment GetProfileCard(string Name)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Hello, {Name}",
                Size = AdaptiveTextSize.ExtraLarge
            });

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
        // Handle token response (signing in the current user)
        private async Task<MessagingExtensionResponse> GetTokenResponse(string state, ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
                // There is no token, so the user has not signed in yet.

                // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);

                return new MessagingExtensionResponse
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
            else
            {
                // set access token
                _graphService.SetAccessToken(tokenResponse.Token);
                return null;
            }

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
            var tokenResponse = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, _appConfig.ConnectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
            return tokenResponse;
        }

    }
}
