﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    //public class TeamsMessagingExtensionsSearchAuthConfigBot : DialogBot<T>
    public class TeamsMessagingExtensionsSearchAuthConfigBot<T> : DialogBot<T>
      where T : Dialog
    {
        private readonly string _connectionName;
        private readonly string _siteUrl;
        private readonly UserState _userState;
        private readonly IStatePropertyAccessor<string> _userConfigProperty;

        public TeamsMessagingExtensionsSearchAuthConfigBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, IConfiguration configuration)
           : base(conversationState, userState, dialog, logger, configuration)
        {
            _connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _siteUrl = configuration["SiteUrl"] ?? throw new NullReferenceException("SiteUrl");
            _userState = userState ?? throw new NullReferenceException(nameof(userState));
            _userConfigProperty = userState.CreateProperty<string>("UserConfiguration");
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // After the turn is complete, persist any UserState changes.
            await _userState.SaveChangesAsync(turnContext);
        }

        protected async override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            var tokenResponse = await GetTokenResponse(turnContext, query.State, cancellationToken);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
                // There is no token, so the user has not signed in yet.
                // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                var signInLink = await (turnContext.Adapter as IUserTokenProvider).GetOauthSignInLinkAsync(turnContext, _connectionName, cancellationToken);
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

            var client = new SimpleGraphClient(tokenResponse.Token);
            var profile = await client.GetMyProfile();
            var heroCard = new ThumbnailCard
            {
                Title = "Thumbnail Card",
                Text = $"Hello, {profile.DisplayName}",
                Images = new List<CardImage> { new CardImage("https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png") },
            };
            var attachments = new MessagingExtensionAttachment(HeroCard.ContentType, null, heroCard);
            var result = new MessagingExtensionResult("list", "result", new[] { attachments });
            return new MessagingExtensionResponse(result);
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            // The user has requested the Messaging Extension Configuration page.  
            var escapedSettings = string.Empty;
            var userConfigSettings = await _userConfigProperty.GetAsync(turnContext, () => string.Empty);
            if (!string.IsNullOrEmpty(userConfigSettings))
            {
                escapedSettings = Uri.EscapeDataString(userConfigSettings);
            }
            return new MessagingExtensionResponse
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
                                Value = $"{_siteUrl}/searchSettings.html?settings={escapedSettings}",
                            },
                        },
                    },
                },
            };
        }

        protected override async Task OnTeamsMessagingExtensionConfigurationSettingAsync(ITurnContext<IInvokeActivity> turnContext, JObject settings, CancellationToken cancellationToken)
        {
            // When the user submits the settings page, this event is fired.
            if (settings["state"] != null)
            {
                var userConfigSettings = settings["state"].ToString();
                await _userConfigProperty.SetAsync(turnContext, userConfigSettings, cancellationToken);
            }
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery action, CancellationToken cancellationToken)
        {

            var text = action?.Parameters?[0]?.Name as string ?? string.Empty;
            var attachments = new List<MessagingExtensionAttachment>();
            var tokenResponse = await GetTokenResponse(turnContext, action.State, cancellationToken);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
                // There is no token, so the user has not signed in yet.
                // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                var signInLink = await (turnContext.Adapter as IUserTokenProvider).GetOauthSignInLinkAsync(turnContext, _connectionName, cancellationToken);
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

            var client = new SimpleGraphClient(tokenResponse.Token);
            var me = await client.GetMyProfile();
            // await client.GetPhotoAsync();
            var paths = new[] { ".", "Resources", "UserDetailsCard.json" };
            string filepath = Path.Combine(paths);
            var previewcard = new ThumbnailCard
            {
                Title = me.DisplayName,
                Images = new List<CardImage> { new CardImage { Url = "https://devicecapabilities.blob.core.windows.net/filestorage/UserLogo.png" } }
            };

            var attachment = new MessagingExtensionAttachment
            {
                ContentType = ThumbnailCard.ContentType,
                Content = previewcard,
                Preview = previewcard.ToAttachment()
            };
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            };
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            // The Preview card's Tap should have a Value property assigned, this will be returned to the bot in this event. 
            var (packageId, version, description, projectUrl, iconUrl) = query.ToObject<(string, string, string, string, string)>();

            // We take every row of the results and wrap them in cards wrapped in in MessagingExtensionAttachment objects.
            // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.
            var card = new ThumbnailCard
            {
                Title = $"{packageId}, {version}",
                Subtitle = description,
                Buttons = new List<CardAction>
                    {
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "Nuget Package", Value = $"https://www.nuget.org/packages/{packageId}" },
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "Project", Value = projectUrl },
                    },
            };

            if (!string.IsNullOrEmpty(iconUrl))
            {
                card.Images = new List<CardImage>() { new CardImage(iconUrl, "Icon") };
            }

            var attachment = new MessagingExtensionAttachment
            {
                ContentType = ThumbnailCard.ContentType,
                Content = card,
            };

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            });
        }

        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            // This method is to handle the 'Close' button on the confirmation Task Module after the user signs out.
            return Task.FromResult(new MessagingExtensionActionResponse());
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            if (action.CommandId.ToUpper() == "SHOWPROFILE")
            {
                var state = action.State; // Check the state value
                var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.

                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var signInLink = await (turnContext.Adapter as IUserTokenProvider).GetOauthSignInLinkAsync(turnContext, _connectionName, cancellationToken);

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

                var profile = await client.GetMyProfile();

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

            if (action.CommandId.ToUpper() == "SIGNOUTCOMMAND")
            {
                await (turnContext.Adapter as IUserTokenProvider).SignOutUserAsync(turnContext, _connectionName, turnContext.Activity.From.Id, cancellationToken);

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
                 _connectionName,
                 turnContext.Activity.From.Id,
                 new TokenExchangeRequest
                 {
                     Token = tokenExchangeRequest.Token,
                 },
                 cancellationToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031 //Do not catch general exception types (ignoring, see comment below)
            catch
#pragma warning restore CA1031 //Do not catch general exception types
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

        public static Attachment UserDetails_Attachment(string Name, string Mail)
        {
            var AppCountCard = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer
                    {
                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text=$"User Details",
                                Size=AdaptiveTextSize.Large,
                                Wrap=true
                            },

                            new AdaptiveColumnSet()
                            {
                                Columns=new List<AdaptiveColumn>()
                                {
                                   new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             new AdaptiveTextBlock(){Text="Name :  "+Name,Color=AdaptiveTextColor.Accent,Size=AdaptiveTextSize.Medium,HorizontalAlignment=AdaptiveHorizontalAlignment.Center,Spacing=AdaptiveSpacing.None}
                                         }
                                    }
                                }
                            },
                             new AdaptiveColumnSet()
                            {
                                Columns=new List<AdaptiveColumn>()
                                {
                                   new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             new AdaptiveTextBlock(){Text="mail  :  "+Mail,Color=AdaptiveTextColor.Accent,Size=AdaptiveTextSize.Medium,HorizontalAlignment=AdaptiveHorizontalAlignment.Center,Spacing=AdaptiveSpacing.None}
                                         }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var acard = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = AppCountCard
            };
            return acard;
        }

        public Attachment FetchAdaptive(string filepath)
        {
            var adaptiveCardJson = File.ReadAllText(filepath);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson)
            };
            return adaptiveCardAttachment;
        }

        private static Attachment GetProfileCard(Graph.User profile)
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
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

    }
}
