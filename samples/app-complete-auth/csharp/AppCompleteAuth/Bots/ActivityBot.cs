// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using AppCompleteAuth.helper;
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
using System.Collections.Concurrent;
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
        private readonly string _botConnectionName;
        private readonly string _facebookConnectionName;
        private readonly ConcurrentDictionary<string, bool> _isSignedIn;

        public ActivityBot(IConfiguration configuration, ConversationState conversationState, T dialog, ConcurrentDictionary<string, bool> isSignedIn)
        {
            _botConnectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _facebookConnectionName = configuration["FacebookConnectionName"] ?? throw new NullReferenceException("FacebookConnectionName");
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
            ConversationState = conversationState;
            Dialog = dialog;
            _isSignedIn = isSignedIn;
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
                // Run the Dialog with the new message Activity.
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
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
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        /// <summary>
        /// Invoked when an app based link query activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="query">The invoke request body type for app-based link query</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Response for the query.</returns>
        protected async override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            var state = query.State;
            bool isSignedIn;
            _isSignedIn.TryGetValue("isSignedIn", out isSignedIn);
            var previewCard = new ThumbnailCard();
            var attachment = new MessagingExtensionAttachment();

            if (string.IsNullOrEmpty(state))
            {
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
                                    Value = $"{_applicationBaseUrl}/config",
                                    Title = "Select login option",
                                },
                            },
                        },
                    },
                };
            }
            else if (state == "sso" || isSignedIn)
            {
                var tokenResponse = await GetTokenResponse(turnContext, _botConnectionName, state, cancellationToken);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    isSignedIn = true;
                    _isSignedIn.AddOrUpdate("isSignedIn", true, (key, newValue) => true);
                    var signInLink = await GetSignInLinkAsync(turnContext, _botConnectionName, cancellationToken).ConfigureAwait(false);

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
                var profile = await client.GetMeAsync();
                var photo = await client.GetPhotoAsync();
                var title = !string.IsNullOrEmpty(profile.JobTitle) ? profile.JobTitle : "Unknown";
                previewCard = new ThumbnailCard
                {
                    Title = $"Hello! {profile.DisplayName}",
                    Text = $"Job title: {title}",
                    Subtitle = $"Email: {profile.UserPrincipalName}",
                    Images = new List<CardImage> { new CardImage { Url = photo } }
                };
                attachment = new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = previewCard,
                    Preview = previewCard.ToAttachment()
                };

                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = "list",
                        Attachments = new List<MessagingExtensionAttachment> { attachment }
                    },
                };
            }
            else if ( state == "usercredentials" || state.Contains("userName") || isSignedIn)
            {
                if (state.Contains("userName"))
                {
                    JObject asJobject = JObject.Parse(state);
                    var userName = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.UserName;
                    var password = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.Password;

                    if (userName == Constant.UserName && password == Constant.Password)
                    {
                        previewCard = new ThumbnailCard
                        {
                            Title = $"Hello! Test user",
                            Text = "Job title: Data Scientist",
                            Subtitle = "Email: testaccount@test123.onmicrosoft.com",
                            Images = new List<CardImage> { new CardImage { Url = "https://media.istockphoto.com/vectors/profile-placeholder-image-gray-silhouette-no-photo-vector-id1016744034?k=20&m=1016744034&s=612x612&w=0&h=kjCAwH5GOC3n3YRTHBaLDsLIuF8P3kkAJc9RvfiYWBY=" } }
                        };
                        attachment = new MessagingExtensionAttachment
                        {
                            ContentType = ThumbnailCard.ContentType,
                            Content = previewCard,
                            Preview = previewCard.ToAttachment()
                        };

                        return new MessagingExtensionResponse
                        {
                            ComposeExtension = new MessagingExtensionResult
                            {
                                Type = "result",
                                AttachmentLayout = "list",
                                Attachments = new List<MessagingExtensionAttachment> { attachment }
                            },
                        };
                    }
                    else
                    {
                        previewCard = new ThumbnailCard
                        {
                            Title = "Invalid credentials",
                        };
                        attachment = new MessagingExtensionAttachment
                        {
                            ContentType = ThumbnailCard.ContentType,
                            Content = previewCard,
                            Preview = previewCard.ToAttachment()
                        };

                        return new MessagingExtensionResponse
                        {
                            ComposeExtension = new MessagingExtensionResult
                            {
                                Type = "result",
                                AttachmentLayout = "list",
                                Attachments = new List<MessagingExtensionAttachment> { attachment }
                            },
                        };
                    }
                }
                else
                {
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
                                        Value = $"{_applicationBaseUrl}/popUpSignin?from=msgext",
                                        Title = "Using username/password",
                                    },
                                },
                            },
                        },
                    };
                }
            }
            else {
                var facebooktokenResponse = await GetTokenResponse(turnContext, _facebookConnectionName, state, cancellationToken);

                if (facebooktokenResponse == null || string.IsNullOrEmpty(facebooktokenResponse.Token))
                {
                    _isSignedIn.AddOrUpdate("isSignedIn", false, (key, newValue) => false);
                    var fbsignInLink = await GetSignInLinkAsync(turnContext, _facebookConnectionName, cancellationToken).ConfigureAwait(false);

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
                                        Value = fbsignInLink,
                                        Title = "Facebook auth",
                                    },
                                },
                            },
                        },
                    };
                }

                FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(facebooktokenResponse.Token);
                previewCard = new ThumbnailCard
                {
                    Title = $"Hello! {fbProfile.Name}",
                    Images = new List<CardImage> { new CardImage { Url = fbProfile.ProfilePicture.data.url } }
                };
                attachment = new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = previewCard,
                    Preview = previewCard.ToAttachment()
                };

                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = "list",
                        Attachments = new List<MessagingExtensionAttachment> { attachment }
                    },
                };
            }
        }

        /// <summary>
        /// Invoked when a Messaging Extension Query activity is received from the connector
        /// </summary>
        /// <param name="turnContext"> Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action"> The query for the search command.</param>
        /// <param name="cancellationToken"> A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns> The Messaging Extension Response for the query.</returns>
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery action, CancellationToken cancellationToken)
        {
            bool isSignedIn;
            _isSignedIn.TryGetValue("isSignedIn", out isSignedIn);
            var previewCard = new ThumbnailCard();
            var attachment = new MessagingExtensionAttachment();
            if (string.IsNullOrEmpty(action.State))
            {
                _isSignedIn.AddOrUpdate("isSignedIn", false, (key, newValue) => false);
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
                                    Value = $"{_applicationBaseUrl}/config",
                                    Title = "Select login option",
                                },
                            },
                        },
                    },
                };
            }
            else if (action.State == "usercredentials" || action.State.Contains("userName"))
            {
                if (action.State.Contains("userName"))
                {
                    JObject asJobject = JObject.Parse(action.State);
                    var userName = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.UserName;
                    var password = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.Password;
                    if (userName == Constant.UserName && password == Constant.Password)
                    {
                        previewCard = new ThumbnailCard
                        {
                            Title = $"Hello! Test user",
                            Text = "Job title: Data Scientist",
                            Subtitle = "Email: testaccount@test123.onmicrosoft.com",
                            Images = new List<CardImage> { new CardImage { Url = "https://media.istockphoto.com/vectors/profile-placeholder-image-gray-silhouette-no-photo-vector-id1016744034?k=20&m=1016744034&s=612x612&w=0&h=kjCAwH5GOC3n3YRTHBaLDsLIuF8P3kkAJc9RvfiYWBY=" } }
                        };
                        attachment = new MessagingExtensionAttachment
                        {
                            ContentType = ThumbnailCard.ContentType,
                            Content = previewCard,
                            Preview = previewCard.ToAttachment()
                        };
                        return new MessagingExtensionResponse
                        {
                            ComposeExtension = new MessagingExtensionResult
                            {
                                Type = "result",
                                AttachmentLayout = "list",
                                Attachments = new List<MessagingExtensionAttachment> { attachment }
                            },
                        };
                    }
                    else
                    {
                        previewCard = new ThumbnailCard
                        {
                            Title = "Invalid credentials",
                        };
                        attachment = new MessagingExtensionAttachment
                        {
                            ContentType = ThumbnailCard.ContentType,
                            Content = previewCard,
                            Preview = previewCard.ToAttachment()
                        };
                        return new MessagingExtensionResponse
                        {
                            ComposeExtension = new MessagingExtensionResult
                            {
                                Type = "result",
                                AttachmentLayout = "list",
                                Attachments = new List<MessagingExtensionAttachment> { attachment }
                            },
                        };
                    }
                }
                else {

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
                                    Value = $"{_applicationBaseUrl}/popUpSignin?from=msgext",
                                    Title = "Using username/password",
                                },
                            },
                            },
                        },
                    };
                }
            }
            else if (action.State == "sso" || isSignedIn)
            {
                var state = action.State;
                var tokenResponse = await GetTokenResponse(turnContext, _botConnectionName, state, cancellationToken);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    isSignedIn = true;
                    _isSignedIn.AddOrUpdate("isSignedIn", true, (key, newValue) => true);
                    var signInLink = await GetSignInLinkAsync(turnContext, _botConnectionName, cancellationToken).ConfigureAwait(false);

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

                var profile = await client.GetMeAsync();
                var photo = await client.GetPhotoAsync();
                var title = !string.IsNullOrEmpty(profile.JobTitle) ?
                        profile.JobTitle : "Unknown";
                previewCard = new ThumbnailCard
                {
                    Title = $"Hello! {profile.DisplayName}",
                    Text = $"Job title: {title}",
                    Subtitle = $"Email: {profile.UserPrincipalName}",
                    Images = new List<CardImage> { new CardImage { Url = photo } }
                };
                attachment = new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = previewCard,
                    Preview = previewCard.ToAttachment()
                };

                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = "list",
                        Attachments = new List<MessagingExtensionAttachment> { attachment }
                    },
                };
            }
            else
            {
                var state = action.State;
                var facebooktokenResponse = await GetTokenResponse(turnContext, _facebookConnectionName, state, cancellationToken);

                if (facebooktokenResponse == null || string.IsNullOrEmpty(facebooktokenResponse.Token))
                {
                    _isSignedIn.AddOrUpdate("isSignedIn", false, (key, newValue) => false);
                    var fbsignInLink = await GetSignInLinkAsync(turnContext, _facebookConnectionName, cancellationToken).ConfigureAwait(false);

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
                                        Value = fbsignInLink,
                                        Title = "Facebook auth",
                                    },
                                },
                            },
                        },
                    };
                }

                FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(facebooktokenResponse.Token);
                previewCard = new ThumbnailCard
                {
                    Title = $"Hello! {fbProfile.Name}",
                    Images = new List<CardImage> { new CardImage { Url = fbProfile.ProfilePicture.data.url } }
                };
                attachment = new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = previewCard,
                    Preview = previewCard.ToAttachment()
                };

                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = "list",
                        Attachments = new List<MessagingExtensionAttachment> { attachment }
                    },
                };
            }
        }

        /// <summary>
        /// When OnTurn method receives a submit invoke activity on bot turn, it calls this method.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Provides context for a turn of a bot and.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents a task module response.</returns>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            if ((!string.IsNullOrEmpty(action.State) && action.CommandId.ToLower() == "sso") || action.CommandId.ToLower() == "sso")
            {
                var state = action.State; // Check the state value
                var tokenResponse = await GetTokenResponse(turnContext, _botConnectionName, state, cancellationToken);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.

                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var signInLink = await GetSignInLinkAsync(turnContext, _botConnectionName, cancellationToken).ConfigureAwait(false);

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
                var photo = await client.GetPhotoAsync();
                var title = !string.IsNullOrEmpty(profile.JobTitle) ?
                        profile.JobTitle : "Unknown";

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetSSOProfileCard(profile, photo, title),
                            Height = 250,
                            Width = 400,
                            Title = "User information card",
                        },
                    },
                };
            }

            if ((!string.IsNullOrEmpty(action.State) && action.CommandId.ToLower() == "facebooklogin") || action.CommandId.ToLower() == "facebooklogin")
            {
                var state = action.State; // Check the state value
                var tokenResponse = await GetTokenResponse(turnContext, _facebookConnectionName, state, cancellationToken);
                
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.

                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var signInLink = await GetSignInLinkAsync(turnContext, _facebookConnectionName, cancellationToken).ConfigureAwait(false);

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
                                        Title = "Facebook auth",
                                    },
                                },
                            },
                        },
                    };
                }

                FacebookProfile profile = await FacebookHelper.GetFacebookProfileName(tokenResponse.Token);

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetFacebookProfile(profile),
                            Height = 250,
                            Width = 400,
                            Title = "User information card",
                        },
                    },
                };
            }
            else if ((!string.IsNullOrEmpty(action.State) && action.CommandId.ToLower() == "usercredentials")|| action.CommandId.ToLower() == "usercredentials")
            {
                if (!string.IsNullOrEmpty(action.State))
                {
                    JObject asJobject = JObject.Parse(action.State);
                    var userName = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.UserName;
                    var password = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.Password;

                    if (userName == Constant.UserName && password == Constant.Password)
                    {
                        return new MessagingExtensionActionResponse
                        {
                            Task = new TaskModuleContinueResponse
                            {
                                Value = new TaskModuleTaskInfo
                                {
                                    Card = GetProfileCard(),
                                    Height = 200,
                                    Width = 400,
                                    Title = "Using credentials",
                                },
                            },
                         };
                    }
                    else
                    {
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
                                            Body = new List<AdaptiveElement>() { new AdaptiveTextBlock() { Text = "Invalid username or password" } }
                                        },
                                        ContentType = AdaptiveCard.ContentType,
                                    },
                                    Height = 200,
                                    Width = 400,
                                    Title = "Using credentials",
                                },
                            },
                        };
                    }
                }
                else
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
            }
            else
            {
                return null;
            }
        }

        // Get sign in link.
        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, string connectionName ,CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(connectionName, turnContext.Activity as Activity, null, cancellationToken).ConfigureAwait(false);
            
            return resource.SignInLink;
        }

        // Get user profile card.
        private static Microsoft.Bot.Schema.Attachment GetSSOProfileCard(User profile, string photo,string title)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"User Information",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                Url = new Uri(photo),
                                Size = AdaptiveImageSize.Medium,
                                Style = AdaptiveImageStyle.Person
                            }
                        },
                        Width ="auto"

                    },
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text =  $"Hello! {profile.DisplayName}",
                                Weight = AdaptiveTextWeight.Bolder,
                                IsSubtle = true
                            }
                        },
                        Width ="stretch"
                    }
                }
            });

            card.Body.Add(new AdaptiveFactSet()
            {
                Separator = true,
                Facts =
                {
                    new AdaptiveFact
                    {
                        Title = "Job title :",
                        Value = $"{title}"
                    },
                    new AdaptiveFact
                    {
                        Title = "Email :",
                        Value = $"{profile.UserPrincipalName}"
                    },
                }
            });
            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        // Get facebook peofile card.
        private static Microsoft.Bot.Schema.Attachment GetFacebookProfile(FacebookProfile profile)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"User Information",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                Url = new Uri(profile.ProfilePicture.data.url),
                                Size = AdaptiveImageSize.Medium,
                                Style = AdaptiveImageStyle.Person
                            }
                        },
                        Width ="auto"

                    },
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text =  $"Hello! {profile.Name}",
                                Weight = AdaptiveTextWeight.Bolder,
                                IsSubtle = true
                            }
                        },
                        Width ="stretch"
                    }
                }
            });

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        // Get token response.
        private async Task<TokenResponse> GetTokenResponse(ITurnContext<IInvokeActivity> turnContext,string connectionName, string state, CancellationToken cancellationToken)
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
            var tokenResponse = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, connectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
            
            return tokenResponse;
        }

        // Get user profile card.
        private Microsoft.Bot.Schema.Attachment GetProfileCard()
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"User Information",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                Url = new Uri("https://media.istockphoto.com/vectors/profile-placeholder-image-gray-silhouette-no-photo-vector-id1016744034?k=20&m=1016744034&s=612x612&w=0&h=kjCAwH5GOC3n3YRTHBaLDsLIuF8P3kkAJc9RvfiYWBY="),
                                Size = AdaptiveImageSize.Medium,
                                Style = AdaptiveImageStyle.Person
                            }
                        },
                        Width ="auto"
                    },
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text =  "Hello! Test user",
                                Weight = AdaptiveTextWeight.Bolder,
                                IsSubtle = true
                            }
                        },
                        Width ="stretch"
                    }
                }
            });
            card.Body.Add(new AdaptiveFactSet()
            {
                Separator = true,
                Facts =
                {
                    new AdaptiveFact
                    {
                        Title = "Job title:",
                        Value = "Data scientist"
                    },
                    new AdaptiveFact
                    {
                        Title = "Email :",
                        Value = "testaccount@test123.onmicrosoft.com"
                    }
                }
            });

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}