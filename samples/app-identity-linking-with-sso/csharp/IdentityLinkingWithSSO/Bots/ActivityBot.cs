// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using IdentityLinkingWithSSO.helper;
using IdentityLinkingWithSSO.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static IdentityLinkingWithSSO.Models.FacebookData;
using static IdentityLinkingWithSSO.Models.GoogleData;

namespace IdentityLinkingWithSSO.Bots
{
    public class ActivityBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        private readonly string _applicationBaseUrl;
        private readonly string _botConnectionName;
        private readonly string _facebookConnectionName;
        private readonly string _googleConnectionName;
        private readonly ConcurrentDictionary<string, List<UserMapData>> userMappingData;

        public ActivityBot(IConfiguration configuration, ConversationState conversationState, T dialog, ConcurrentDictionary<string, List<UserMapData>> mapdata)
        {
            _botConnectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _facebookConnectionName = configuration["FacebookConnectionName"] ?? throw new NullReferenceException("FacebookConnectionName");
            _googleConnectionName = configuration["GoogleConnectionName"] ?? throw new NullReferenceException("GoogleConnectionName");
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
            ConversationState = conversationState;
            userMappingData = mapdata;
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
            var previewCard = new ThumbnailCard();
            var attachment = new MessagingExtensionAttachment();
            var attachments = new List<MessagingExtensionAttachment>();

            var state = query.State;
            var tokenResponse = await GetTokenResponse(turnContext, _botConnectionName, state, cancellationToken);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
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
            else
            {
                List<UserMapData> currentList = new List<UserMapData>();
                List<UserMapData> userDetailsList = new List<UserMapData>();
                var googleData = new GoogleData();
                var facebookData = new FacebookData();
                userMappingData.TryGetValue("link", out currentList);
                var client = new SimpleGraphClient(tokenResponse.Token);
                var profile = await client.GetMeAsync();
                var photo = await client.GetPhotoAsync();
                var title = !string.IsNullOrEmpty(profile.JobTitle) ? profile.JobTitle : "Unknown";
                if (currentList == null)
                {
                    var userDetails = new UserMapData()
                    {
                        AadId = turnContext.Activity.From.AadObjectId,
                        isAadSignedIn = true,
                        AadToken = tokenResponse.Token,
                        isFacebookSignedIn = false,
                        isGoogleSignedIn = false
                    };

                    googleData.isGoogleSignedIn = false;
                    facebookData.isFacebookSignedIn = false;
                    userDetailsList.Add(userDetails);
                    userMappingData.AddOrUpdate("link", userDetailsList, (key, newvalue) => currentList);
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
                                    Value = $"{_applicationBaseUrl}/config?is_fb_signed_in=false&is_google_signed_in=false",
                                    Title = "Select login option",
                                },
                            },
                            },
                        },
                    };
                }

                else
                {
                    var data = currentList.Find(e => e.AadId == turnContext.Activity.From.AadObjectId);
                    if (data == null)
                    {
                        var userDetails = new UserMapData()
                        {
                            AadId = turnContext.Activity.From.AadObjectId,
                            isAadSignedIn = true,
                            AadToken = tokenResponse.Token,
                            isFacebookSignedIn = false,
                            isGoogleSignedIn = false

                        };

                        googleData.isGoogleSignedIn = false;
                        facebookData.isFacebookSignedIn = false;
                        currentList.Add(userDetails);
                        userMappingData.AddOrUpdate("link", currentList, (key, newvalue) => currentList);

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
                                            Value = $"{_applicationBaseUrl}/config?is_fb_signed_in=false&is_google_signed_in=false",
                                            Title = "Select login option",
                                        },
                                    },
                                },
                            },
                        };
                    }
                    else
                    {
                        data.AadToken = tokenResponse.Token;
                        var index = currentList.FindIndex(e => e.AadId == turnContext.Activity.From.AadObjectId);
                        googleData.isGoogleSignedIn = false;
                        facebookData.isFacebookSignedIn = false;
                        currentList[index] = data;
                        userMappingData.AddOrUpdate("link", currentList, (key, newvalue) => currentList);

                        if (!data.isFacebookSignedIn && state == null || !data.isGoogleSignedIn && state == null)
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
                                                Value = $"{_applicationBaseUrl}/config?is_fb_signed_in={data.isFacebookSignedIn}&is_google_signed_in={data.isGoogleSignedIn}",
                                                Title = "Select login option",
                                            },
                                        },
                                    },
                                },
                            };
                        }

                        else if (state == "ConnectWithFacebook" || SignInIndicator.is_fb_signed_in_search)
                        {
                            var facebooktokenResponse = await GetTokenResponse(turnContext, _facebookConnectionName, state, cancellationToken);

                            if (facebooktokenResponse == null || string.IsNullOrEmpty(facebooktokenResponse.Token))
                            {
                                SignInIndicator.is_fb_signed_in_search = true;
                                SignInIndicatorGoogle.is_google_signed_in_search = false;
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

                            else
                            {
                                SignInIndicator.is_fb_signed_in_search = false;
                                FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(facebooktokenResponse.Token);
                                data.FacebookToken = facebooktokenResponse.Token;
                                data.FacebookId = fbProfile.Id;
                                data.isFacebookSignedIn = true;
                                facebookData.isFacebookSignedIn = true;
                                facebookData.facebookName = fbProfile.Name;
                                facebookData.facebookProfileUrl = fbProfile.ProfilePicture;
                                currentList[index] = data;
                                userMappingData.AddOrUpdate("link", currentList, (key, newvalue) => currentList);
                                var adaptiveCard = GetProfileCard(profile, photo, title, facebookData, googleData);

                                var preview = new MessagingExtensionAttachment(
                                            contentType: HeroCard.ContentType,
                                            contentUrl: null,
                                            content: GetProfileCard(profile, photo, title, facebookData, googleData));

                                if (data.isGoogleSignedIn)
                                {
                                    var client2 = new HttpClient();
                                    client2.DefaultRequestHeaders.Accept.Clear();
                                    client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + data.GoogleToken);

                                    var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                                    var jboject = JsonConvert.DeserializeObject(json);
                                    var googleProfile = JObject.FromObject(jboject);
                                    var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                                    List<UserData> items = ((JArray)state1).Select(x => new UserData
                                    {
                                        DisplayName = (string)x["displayName"]
                                    }).ToList();

                                    var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                                    List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                                    {
                                        Url = (string)x["url"]
                                    }).ToList();

                                    var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                                    List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                                    {
                                        Value = (string)x["value"]
                                    }).ToList();

                                    var displayName = items[0].DisplayName;
                                    var photoUrl = items2[0].Url;
                                    var emailAddress = items3[0].Value;

                                    googleData.isGoogleSignedIn = true;
                                    googleData.googleEmail = emailAddress;
                                    googleData.googleName = displayName;
                                    googleData.googleProfileUrl = photoUrl;

                                    preview = new MessagingExtensionAttachment(
                                            contentType: HeroCard.ContentType,
                                            contentUrl: null,
                                            content: GetProfileCard(profile, photo, title, facebookData, googleData));

                                    return new MessagingExtensionResponse
                                    {
                                        ComposeExtension = new MessagingExtensionResult
                                        {
                                            Type = "result",
                                            AttachmentLayout = AttachmentLayoutTypes.List,
                                            Attachments = new List<MessagingExtensionAttachment>() {
                                            new MessagingExtensionAttachment
                                            {
                                                ContentType = AdaptiveCard.ContentType,
                                                Content = GetProfileCard(profile, photo, title, facebookData, googleData),
                                                Preview = preview,
                                            }
                                        }
                                        },
                                    };
                                }

                                else
                                {
                                    googleData.isGoogleSignedIn = false;
                                }

                                return new MessagingExtensionResponse
                                {
                                    ComposeExtension = new MessagingExtensionResult
                                    {
                                        Type = "result",
                                        AttachmentLayout = AttachmentLayoutTypes.List,
                                        Attachments = new List<MessagingExtensionAttachment>() {
                                            new MessagingExtensionAttachment
                                            {
                                                ContentType = AdaptiveCard.ContentType,
                                                Content = GetProfileCard(profile, photo, title, facebookData, googleData),
                                                Preview = preview,
                                            }
                                        }
                                    },
                                };

                            }
                        }

                        else if (state == "ConnectWithGoogle" || SignInIndicatorGoogle.is_google_signed_in_search)
                        {
                            var googleTokenResponse = await GetTokenResponse(turnContext, _googleConnectionName, query.State, cancellationToken);
                            if (googleTokenResponse == null || string.IsNullOrEmpty(googleTokenResponse.Token))
                            {
                                SignInIndicator.is_fb_signed_in_search = false;
                                SignInIndicatorGoogle.is_google_signed_in_search = true;
                                var googleLink = await GetSignInLinkAsync(turnContext, _googleConnectionName, cancellationToken).ConfigureAwait(false);

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
                                                    Value = googleLink,
                                                    Title = "Google auth",
                                                },
                                            },
                                        },
                                    },
                                };
                            }

                            else
                            {
                                SignInIndicatorGoogle.is_google_signed_in_search = false;
                                var client2 = new HttpClient();
                                client2.DefaultRequestHeaders.Accept.Clear();
                                client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + googleTokenResponse.Token);

                                var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                                var jboject = JsonConvert.DeserializeObject(json);
                                var googleProfile = JObject.FromObject(jboject);

                                var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                                List<UserData> items = ((JArray)state1).Select(x => new UserData
                                {
                                    DisplayName = (string)x["displayName"]
                                }).ToList();

                                var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                                List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                                {
                                    Url = (string)x["url"]
                                }).ToList();

                                var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                                List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                                {
                                    Value = (string)x["value"]
                                }).ToList();

                                var displayName = items[0].DisplayName;
                                var photoUrl = items2[0].Url;
                                var emailAddress = items3[0].Value;

                                data.GoogleToken = googleTokenResponse.Token;
                                data.GoogleId = emailAddress;
                                data.isGoogleSignedIn = true;
                                googleData.isGoogleSignedIn = true;
                                googleData.googleEmail = emailAddress;
                                googleData.googleName = displayName;
                                googleData.googleProfileUrl = photoUrl;
                                currentList[index] = data;
                                userMappingData.AddOrUpdate("link", currentList, (key, newvalue) => currentList);

                                var adaptiveCard = GetProfileCard(profile, photo, title, facebookData, googleData);

                                var preview = new MessagingExtensionAttachment(
                                            contentType: HeroCard.ContentType,
                                            contentUrl: null,
                                            content: GetProfileCard(profile, photo, title, facebookData, googleData));

                                if (data.isFacebookSignedIn)
                                {
                                    FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(data.FacebookToken);
                                    facebookData.isFacebookSignedIn = true;
                                    facebookData.facebookName = fbProfile.Name;
                                    facebookData.facebookProfileUrl = fbProfile.ProfilePicture;

                                    preview = new MessagingExtensionAttachment(
                                            contentType: HeroCard.ContentType,
                                            contentUrl: null,
                                            content: GetProfileCard(profile, photo, title, facebookData, googleData));

                                    return new MessagingExtensionResponse
                                    {
                                        ComposeExtension = new MessagingExtensionResult
                                        {
                                            Type = "result",
                                            AttachmentLayout = AttachmentLayoutTypes.List,
                                            //  Attachments = new List<MessagingExtensionAttachment>() { ssoAttachment, faceBookCardAttachment }
                                            Attachments = new List<MessagingExtensionAttachment>() {
                                            new MessagingExtensionAttachment
                                            {
                                                ContentType = AdaptiveCard.ContentType,
                                                Content = GetProfileCard(profile, photo, title, facebookData, googleData),
                                                Preview = preview,
                                            }
                                        }
                                        },
                                    };
                                }

                                else
                                {
                                    facebookData.isFacebookSignedIn = false;
                                }

                                return new MessagingExtensionResponse
                                {
                                    ComposeExtension = new MessagingExtensionResult
                                    {
                                        Type = "result",
                                        AttachmentLayout = AttachmentLayoutTypes.List,
                                        Attachments = new List<MessagingExtensionAttachment>() {
                                            new MessagingExtensionAttachment
                                            {
                                                ContentType = AdaptiveCard.ContentType,
                                                Content = GetProfileCard(profile, photo, title, facebookData, googleData),
                                                Preview = preview,
                                            }
                                        }
                                    },
                                };
                            }
                        }

                        else
                        {
                            FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(data.FacebookToken);
                            facebookData.isFacebookSignedIn = true;
                            facebookData.facebookName = fbProfile.Name;
                            facebookData.facebookProfileUrl = fbProfile.ProfilePicture;

                            var client2 = new HttpClient();
                            client2.DefaultRequestHeaders.Accept.Clear();
                            client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + data.GoogleToken);

                            var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                            var jboject = JsonConvert.DeserializeObject(json);
                            var googleProfile = JObject.FromObject(jboject);
                            var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                            List<UserData> items = ((JArray)state1).Select(x => new UserData
                            {
                                DisplayName = (string)x["displayName"]
                            }).ToList();

                            var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                            List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                            {
                                Url = (string)x["url"]
                            }).ToList();

                            var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                            List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                            {
                                Value = (string)x["value"]
                            }).ToList();

                            var displayName = items[0].DisplayName;
                            var photoUrl = items2[0].Url;
                            var emailAddress = items3[0].Value;
                            googleData.isGoogleSignedIn = true;
                            googleData.googleEmail = emailAddress;
                            googleData.googleName = displayName;
                            googleData.googleProfileUrl = photoUrl;

                            var adaptiveCard = GetProfileCard(profile, photo, title, facebookData, googleData);

                            var preview = new MessagingExtensionAttachment(
                                        contentType: HeroCard.ContentType,
                                        contentUrl: null,
                                        content: GetProfileCard(profile, photo, title, facebookData, googleData));

                            return new MessagingExtensionResponse
                            {
                                ComposeExtension = new MessagingExtensionResult
                                {
                                    Type = "result",
                                    AttachmentLayout = AttachmentLayoutTypes.List,
                                    Attachments = new List<MessagingExtensionAttachment>() {
                                            new MessagingExtensionAttachment
                                            {
                                                ContentType = AdaptiveCard.ContentType,
                                                Content = GetProfileCard(profile, photo, title, facebookData, googleData),
                                                Preview = preview,
                                            }
                                        }
                                },
                            };
                        }
                    }
                }
            }
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var actionData = JObject.FromObject(action.Data);
            var state = (string)actionData.ToObject<CardTaskFetchValue<string>>()?.Id;
            var fbConnect = "connectWithFacebook";
            var googleConnect = "connectWithGoogle";
            var googleData = new GoogleData();
            var facebookData = new FacebookData();
            List<UserMapData> currentList = new List<UserMapData>();
            userMappingData.TryGetValue("me", out currentList);
            var data = currentList.Find(e => e.AadId == turnContext.Activity.From.AadObjectId);
            var index = currentList.FindIndex(e => e.AadId == turnContext.Activity.From.AadObjectId);

            var client = new SimpleGraphClient(data.AadToken);

            var profile = await client.GetMeAsync();
            var photo = await client.GetPhotoAsync();
            var title = !string.IsNullOrEmpty(profile.JobTitle) ?
                    profile.JobTitle : "Unknown";

            if (state == fbConnect || SignInIndicator.is_fb_signed_in)
            {
                var tokenResponse = await GetTokenResponse(turnContext, _facebookConnectionName, null, cancellationToken);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.
                    SignInIndicator.is_fb_signed_in = true;
                    SignInIndicatorGoogle.is_google_signed_in = false;
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
                FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(tokenResponse.Token);
                data.FacebookId = profile.Id;
                data.isFacebookSignedIn = true;
                data.FacebookToken = tokenResponse.Token;
                currentList[index] = data;
                userMappingData.AddOrUpdate("me", currentList, (key, newvalue) => currentList);
                facebookData.isFacebookSignedIn = true;
                facebookData.facebookName = fbProfile.Name;
                facebookData.facebookProfileUrl = fbProfile.ProfilePicture;

                if (data.isGoogleSignedIn)
                {
                    var client2 = new HttpClient();
                    client2.DefaultRequestHeaders.Accept.Clear();
                    client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + data.GoogleToken);

                    var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                    var jboject = JsonConvert.DeserializeObject(json);
                    var googleProfile = JObject.FromObject(jboject);
                    var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                    List<UserData> items = ((JArray)state1).Select(x => new UserData
                    {
                        DisplayName = (string)x["displayName"]
                    }).ToList();

                    var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                    List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                    {
                        Url = (string)x["url"]
                    }).ToList();

                    var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                    List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                    {
                        Value = (string)x["value"]
                    }).ToList();

                    var displayName = items[0].DisplayName;
                    var photoUrl = items2[0].Url;
                    var emailAddress = items3[0].Value;

                    googleData.googleName = displayName;
                    googleData.googleProfileUrl = photoUrl;
                    googleData.googleEmail = emailAddress;
                    googleData.isGoogleSignedIn = true;
                }
                else
                {
                    googleData.isGoogleSignedIn = false;
                }

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetMEResponseCard(profile, photo, title, facebookData, googleData),
                            Height = 250,
                            Width = 400,
                            Title = "Show profile card",
                        },
                    },
                };
            }

            if (state == googleConnect || SignInIndicatorGoogle.is_google_signed_in)
            {
                var googleTokenResponse = await GetTokenResponse(turnContext, _googleConnectionName, null, cancellationToken);
                if (googleTokenResponse == null || string.IsNullOrEmpty(googleTokenResponse.Token))
                {
                    SignInIndicator.is_fb_signed_in = false;
                    SignInIndicatorGoogle.is_google_signed_in = true;
                    var googleSignInLink = await GetSignInLinkAsync(turnContext, _googleConnectionName, cancellationToken).ConfigureAwait(false);

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
                                            Value = googleSignInLink,
                                            Title = "Google auth",
                                        },
                                    },
                            },
                        },
                    };
                }

                var client2 = new HttpClient();
                client2.DefaultRequestHeaders.Accept.Clear();
                client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + googleTokenResponse.Token);

                var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                var jboject = JsonConvert.DeserializeObject(json);
                var googleProfile = JObject.FromObject(jboject);
                var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                List<UserData> items = ((JArray)state1).Select(x => new UserData
                {
                    DisplayName = (string)x["displayName"]
                }).ToList();

                var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                {
                    Url = (string)x["url"]
                }).ToList();

                var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                {
                    Value = (string)x["value"]
                }).ToList();

                var displayName = items[0].DisplayName;
                var photoUrl = items2[0].Url;
                var emailAddress = items3[0].Value;
                googleData.isGoogleSignedIn = true;
                googleData.googleEmail = emailAddress;
                googleData.googleName = displayName;
                googleData.googleProfileUrl = photoUrl;
                data.GoogleId = emailAddress;
                data.isGoogleSignedIn = true;
                data.GoogleToken = googleTokenResponse.Token;
                currentList[index] = data;
                userMappingData.AddOrUpdate("me", currentList, (key, newvalue) => currentList);

                if (data.isFacebookSignedIn)
                {
                    FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(data.FacebookToken);
                    facebookData.isFacebookSignedIn = true;
                    facebookData.facebookName = fbProfile.Name;
                    facebookData.facebookProfileUrl = fbProfile.ProfilePicture;
                }
                else
                {
                    facebookData.isFacebookSignedIn = false;
                }

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetMEResponseCard(profile, photo, title, facebookData, googleData),
                            Height = 250,
                            Width = 400,
                            Title = "Show profile card",
                        },
                    },
                };

            }

            if(state == "dicconnectFromFacebook")
            {
                SignInIndicator.is_fb_signed_in = false;
                SignInIndicatorGoogle.is_google_signed_in = false;
                data.FacebookToken = null;
                data.FacebookId = null;
                data.isFacebookSignedIn = false;
                facebookData.isFacebookSignedIn = false;
                currentList[index] = data;
                userMappingData.AddOrUpdate("me", currentList, (key, newvalue) => currentList);

                if (data.isGoogleSignedIn)
                {
                    var client2 = new HttpClient();
                    client2.DefaultRequestHeaders.Accept.Clear();
                    client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + data.GoogleToken);

                    var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                    var jboject = JsonConvert.DeserializeObject(json);
                    var googleProfile = JObject.FromObject(jboject);
                    var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                    List<UserData> items = ((JArray)state1).Select(x => new UserData
                    {
                        DisplayName = (string)x["displayName"]
                    }).ToList();

                    var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                    List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                    {
                        Url = (string)x["url"]
                    }).ToList();

                    var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                    List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                    {
                        Value = (string)x["value"]
                    }).ToList();

                    var displayName = items[0].DisplayName;
                    var photoUrl = items2[0].Url;
                    var emailAddress = items3[0].Value;
                    googleData.isGoogleSignedIn = true;
                    googleData.googleName = displayName;
                    googleData.googleEmail = emailAddress;
                    googleData.googleProfileUrl = photoUrl;
                }

                else
                {
                    googleData.isGoogleSignedIn = false;
                }

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetMEResponseCard(profile, photo, title, facebookData, googleData),
                            Height = 250,
                            Width = 400,
                            Title = "Show profile card",
                        },
                    },
                };
            }

            if (state == "disConnectFromGoogle")
            {
                SignInIndicator.is_fb_signed_in = false;
                SignInIndicatorGoogle.is_google_signed_in = false;
                data.GoogleToken = null;
                data.GoogleId = null;
                data.isGoogleSignedIn = false;
                googleData.isGoogleSignedIn = false;
                currentList[index] = data;
                userMappingData.AddOrUpdate("me", currentList, (key, newvalue) => currentList);

                if (data.isFacebookSignedIn)
                {
                    FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(data.FacebookToken);
                    facebookData.isFacebookSignedIn = true;
                    facebookData.facebookName = fbProfile.Name;
                    facebookData.facebookProfileUrl = fbProfile.ProfilePicture;
                }

                else
                {
                    facebookData.isFacebookSignedIn = false;
                }

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetMEResponseCard(profile, photo, title, facebookData, googleData),
                            Height = 250,
                            Width = 400,
                            Title = "Show profile card",
                        },
                    },
                };
            }

            else
            {
                return null;
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
            var previewCard = new ThumbnailCard();
            var attachment = new MessagingExtensionAttachment();
            var attachments = new List<MessagingExtensionAttachment>();

            var state = action.State;
            var tokenResponse = await GetTokenResponse(turnContext, _botConnectionName, state, cancellationToken);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
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
            else
            {
                List<UserMapData> currentList = new List<UserMapData>();
                List<UserMapData> userDetailsList = new List<UserMapData>();
                userMappingData.TryGetValue("search", out currentList);
                var client = new SimpleGraphClient(tokenResponse.Token);
                var profile = await client.GetMeAsync();
                var photo = await client.GetPhotoAsync();
                var title = !string.IsNullOrEmpty(profile.JobTitle) ? profile.JobTitle : "Unknown";
                if (currentList == null)
                {
                    var userDetails = new UserMapData()
                    {
                        AadId = turnContext.Activity.From.AadObjectId,
                        isAadSignedIn = true,
                        AadToken = tokenResponse.Token,
                        isFacebookSignedIn = false,
                        isGoogleSignedIn = false
                    };

                    userDetailsList.Add(userDetails);
                    userMappingData.AddOrUpdate("search", userDetailsList, (key, newvalue) => currentList);
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
                                        Value = $"{_applicationBaseUrl}/config?is_fb_signed_in=false&is_google_signed_in=false",
                                        Title = "Select login option",
                                    },
                                },
                            },
                        },
                    };
                }

                else
                {
                    var data = currentList.Find(e => e.AadId == turnContext.Activity.From.AadObjectId);
                    if (data == null)
                    {
                        var userDetails = new UserMapData()
                        {
                            AadId = turnContext.Activity.From.AadObjectId,
                            isAadSignedIn = true,
                            AadToken = tokenResponse.Token,
                            isFacebookSignedIn = false,
                            isGoogleSignedIn = false

                        };

                        currentList.Add(userDetails);
                        userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);

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
                                            Value = $"{_applicationBaseUrl}/config?is_fb_signed_in=false&is_google_signed_in=false",
                                            Title = "Select login option",
                                        },
                                    },
                                },
                            },
                        };
                    }
                    else
                    {
                        data.AadToken = tokenResponse.Token;
                        var index = currentList.FindIndex(e => e.AadId == turnContext.Activity.From.AadObjectId);
                        currentList[index] = data;
                        userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);

                        if ((!data.isFacebookSignedIn && state == null) || (!data.isGoogleSignedIn && state == null))
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
                                               Value = $"{_applicationBaseUrl}/config?is_fb_signed_in={data.isFacebookSignedIn}&is_google_signed_in={data.isGoogleSignedIn}",
                                               Title = "Select login option",
                                           },
                                        },
                                    },
                                },
                            };
                        }

                        else if (state == "ConnectWithFacebook" || SignInIndicator.is_fb_signed_in_search)
                        {
                            var facebooktokenResponse = await GetTokenResponse(turnContext, _facebookConnectionName, state, cancellationToken);

                            if (facebooktokenResponse == null || string.IsNullOrEmpty(facebooktokenResponse.Token))
                            {
                                SignInIndicator.is_fb_signed_in_search = true;
                                SignInIndicatorGoogle.is_google_signed_in_search = false;
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

                            else
                            {
                                SignInIndicator.is_fb_signed_in_search = false;
                                FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(facebooktokenResponse.Token);
                                data.FacebookToken = facebooktokenResponse.Token;
                                data.FacebookId = fbProfile.Id;
                                data.isFacebookSignedIn = true;
                                currentList[index] = data;
                                userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);
                                var ssoCard = new ThumbnailCard
                                {
                                    Title = $"Hello! {profile.DisplayName}",
                                    Text = $"Job title: {title}",
                                    Subtitle = $"Email: {profile.UserPrincipalName}",
                                    Images = new List<CardImage> { new CardImage { Url = photo } }
                                };
                                var ssoAttachment = new MessagingExtensionAttachment
                                {
                                    ContentType = ThumbnailCard.ContentType,
                                    Content = ssoCard,
                                    Preview = ssoCard.ToAttachment()
                                };

                                var faceBookCard = new ThumbnailCard
                                {
                                    Title = $"Hello! {fbProfile.Name}",
                                    Images = new List<CardImage> { new CardImage { Url = fbProfile.ProfilePicture.data.url } }
                                };
                                var faceBookCardAttachment = new MessagingExtensionAttachment
                                {
                                    ContentType = ThumbnailCard.ContentType,
                                    Content = faceBookCard,
                                    Preview = faceBookCard.ToAttachment()
                                };

                                if (data.isGoogleSignedIn)
                                {
                                    var client2 = new HttpClient();
                                    client2.DefaultRequestHeaders.Accept.Clear();
                                    client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + data.GoogleToken);

                                    var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                                    var jboject = JsonConvert.DeserializeObject(json);
                                    var googleProfile = JObject.FromObject(jboject);
                                    var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                                    List<UserData> items = ((JArray)state1).Select(x => new UserData
                                    {
                                        DisplayName = (string)x["displayName"]
                                    }).ToList();

                                    var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                                    List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                                    {
                                        Url = (string)x["url"]
                                    }).ToList();

                                    var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                                    List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                                    {
                                        Value = (string)x["value"]
                                    }).ToList();

                                    var displayName = items[0].DisplayName;
                                    var photoUrl = items2[0].Url;
                                    var emailAddress = items3[0].Value;

                                    var googleCard = new ThumbnailCard
                                    {
                                        Title = $"Hello! {displayName}",
                                        Images = new List<CardImage> { new CardImage { Url = photoUrl } }
                                    };
                                    var googleCardAttachment = new MessagingExtensionAttachment
                                    {
                                        ContentType = ThumbnailCard.ContentType,
                                        Content = googleCard,
                                        Preview = googleCard.ToAttachment()
                                    };

                                    return new MessagingExtensionResponse
                                    {
                                        ComposeExtension = new MessagingExtensionResult
                                        {
                                            Type = "result",
                                            AttachmentLayout = "list",
                                            Attachments = new List<MessagingExtensionAttachment>() { ssoAttachment, faceBookCardAttachment, googleCardAttachment }
                                        },
                                    };
                                }

                                return new MessagingExtensionResponse
                                {
                                    ComposeExtension = new MessagingExtensionResult
                                    {
                                        Type = "result",
                                        AttachmentLayout = "list",
                                        Attachments = new List<MessagingExtensionAttachment>() { ssoAttachment, faceBookCardAttachment }
                                    },
                                };

                            }
                        }

                        else if (state == "ConnectWithGoogle" || SignInIndicatorGoogle.is_google_signed_in_search)
                        {
                            var googleTokenResponse = await GetTokenResponse(turnContext, _googleConnectionName, action.State, cancellationToken);
                            if (googleTokenResponse == null || string.IsNullOrEmpty(googleTokenResponse.Token))
                            {
                                SignInIndicator.is_fb_signed_in_search = false;
                                SignInIndicatorGoogle.is_google_signed_in_search = true;
                                var googleLink = await GetSignInLinkAsync(turnContext, _googleConnectionName, cancellationToken).ConfigureAwait(false);

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
                                                    Value = googleLink,
                                                    Title = "Google auth",
                                                },
                                            },
                                        },
                                    },
                                };
                            }

                            else
                            {
                                SignInIndicatorGoogle.is_google_signed_in_search = false;
                                var client2 = new HttpClient();
                                client2.DefaultRequestHeaders.Accept.Clear();
                                client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + googleTokenResponse.Token);

                                var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                                var jboject = JsonConvert.DeserializeObject(json);
                                var googleProfile = JObject.FromObject(jboject);

                                var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                                List<UserData> items = ((JArray)state1).Select(x => new UserData
                                {
                                    DisplayName = (string)x["displayName"]
                                }).ToList();

                                var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                                List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                                {
                                    Url = (string)x["url"]
                                }).ToList();

                                var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                                List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                                {
                                    Value = (string)x["value"]
                                }).ToList();

                                var displayName = items[0].DisplayName;
                                var photoUrl = items2[0].Url;
                                var emailAddress = items3[0].Value;

                                data.GoogleToken = googleTokenResponse.Token;
                                data.GoogleId = emailAddress;
                                data.isGoogleSignedIn = true;
                                currentList[index] = data;
                                userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);
                                var ssoCard = new ThumbnailCard
                                {
                                    Title = $"Hello! {profile.DisplayName}",
                                    Text = $"Job title: {title}",
                                    Subtitle = $"Email: {profile.UserPrincipalName}",
                                    Images = new List<CardImage> { new CardImage { Url = photo } }
                                };
                                var ssoAttachment = new MessagingExtensionAttachment
                                {
                                    ContentType = ThumbnailCard.ContentType,
                                    Content = ssoCard,
                                    Preview = ssoCard.ToAttachment()
                                };

                                var googleCard = new ThumbnailCard
                                {
                                    Title = $"Hello! {displayName}",
                                    Images = new List<CardImage> { new CardImage { Url = photoUrl } }
                                };
                                var googleCardAttachment = new MessagingExtensionAttachment
                                {
                                    ContentType = ThumbnailCard.ContentType,
                                    Content = googleCard,
                                    Preview = googleCard.ToAttachment()
                                };


                                if (data.isFacebookSignedIn)
                                {
                                    FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(data.FacebookToken);

                                    var faceBookCard = new ThumbnailCard
                                    {
                                        Title = $"Hello! {fbProfile.Name}",
                                        Images = new List<CardImage> { new CardImage { Url = fbProfile.ProfilePicture.data.url } }
                                    };
                                    var faceBookCardAttachment = new MessagingExtensionAttachment
                                    {
                                        ContentType = ThumbnailCard.ContentType,
                                        Content = faceBookCard,
                                        Preview = faceBookCard.ToAttachment()
                                    };



                                    return new MessagingExtensionResponse
                                    {
                                        ComposeExtension = new MessagingExtensionResult
                                        {
                                            Type = "result",
                                            AttachmentLayout = "list",
                                            Attachments = new List<MessagingExtensionAttachment>() { ssoAttachment, faceBookCardAttachment, googleCardAttachment }
                                        },
                                    };
                                }

                                return new MessagingExtensionResponse
                                {
                                    ComposeExtension = new MessagingExtensionResult
                                    {
                                        Type = "result",
                                        AttachmentLayout = "list",
                                        Attachments = new List<MessagingExtensionAttachment>() { ssoAttachment, googleCardAttachment }
                                    },
                                };
                            }
                        }

                        else if (state == "DisconnectFromGoogle")
                        {
                            data.GoogleId = null;
                            data.GoogleToken = null;
                            data.isGoogleSignedIn = false;
                            currentList[index] = data;
                            userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);

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
                                               Value = $"{_applicationBaseUrl}/config?is_fb_signed_in={data.isFacebookSignedIn}&is_google_signed_in={data.isGoogleSignedIn}",
                                               Title = "Select login option",
                                           },
                                        },
                                    },
                                },
                            };
                        }

                        else if (state == "DisconnectFromFacebook")
                        {
                            data.FacebookId = null;
                            data.FacebookToken = null;
                            data.isFacebookSignedIn = false;
                            currentList[index] = data;
                            userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);

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
                                               Value = $"{_applicationBaseUrl}/config?is_fb_signed_in={data.isFacebookSignedIn}&is_google_signed_in={data.isGoogleSignedIn}",
                                               Title = "Select login option",
                                           },
                                        },
                                    },
                                },
                            };
                        }

                        else
                        {
                            FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(data.FacebookToken);
                            var client2 = new HttpClient();
                            client2.DefaultRequestHeaders.Accept.Clear();
                            client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + data.GoogleToken);

                            var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                            var jboject = JsonConvert.DeserializeObject(json);
                            var googleProfile = JObject.FromObject(jboject);
                            var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                            List<UserData> items = ((JArray)state1).Select(x => new UserData
                            {
                                DisplayName = (string)x["displayName"]
                            }).ToList();

                            var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                            List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                            {
                                Url = (string)x["url"]
                            }).ToList();

                            var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                            List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                            {
                                Value = (string)x["value"]
                            }).ToList();

                            var displayName = items[0].DisplayName;
                            var photoUrl = items2[0].Url;
                            var emailAddress = items3[0].Value;

                            var ssoCard = new ThumbnailCard
                            {
                                Title = $"Hello! {profile.DisplayName}",
                                Text = $"Job title: {title}",
                                Subtitle = $"Email: {profile.UserPrincipalName}",
                                Images = new List<CardImage> { new CardImage { Url = photo } }
                            };
                            var ssoAttachment = new MessagingExtensionAttachment
                            {
                                ContentType = ThumbnailCard.ContentType,
                                Content = ssoCard,
                                Preview = ssoCard.ToAttachment()
                            };

                            var googleCard = new ThumbnailCard
                            {
                                Title = $"Hello! {displayName}",
                                Images = new List<CardImage> { new CardImage { Url = photoUrl } }
                            };
                            var googleCardAttachment = new MessagingExtensionAttachment
                            {
                                ContentType = ThumbnailCard.ContentType,
                                Content = googleCard,
                                Preview = googleCard.ToAttachment()
                            };

                            var faceBookCard = new ThumbnailCard
                            {
                                Title = $"Hello! {fbProfile.Name}",
                                Images = new List<CardImage> { new CardImage { Url = fbProfile.ProfilePicture.data.url } }
                            };

                            var faceBookCardAttachment = new MessagingExtensionAttachment
                            {
                                ContentType = ThumbnailCard.ContentType,
                                Content = faceBookCard,
                                Preview = faceBookCard.ToAttachment()
                            };

                            return new MessagingExtensionResponse
                            {
                                ComposeExtension = new MessagingExtensionResult
                                {
                                    Type = "result",
                                    AttachmentLayout = "list",
                                    Attachments = new List<MessagingExtensionAttachment>() { ssoAttachment, faceBookCardAttachment, googleCardAttachment }
                                },
                            };
                        }
                    }
                }

            }
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            List<UserMapData> currentList = new List<UserMapData>();
            userMappingData.TryGetValue("search", out currentList);

            if (currentList == null)
            {
                return null;
            }

            else
            {
                var data = currentList.Find(e => e.AadId == turnContext.Activity.From.AadObjectId);
                if(data == null)
                {
                    return null;
                }

                else
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
                                       Value = $"{_applicationBaseUrl}/config?is_fb_signed_in={data.isFacebookSignedIn}&is_google_signed_in={data.isGoogleSignedIn}",
                                       Title = "Select login option",
                                   },
                                },
                            },
                        },
                    };
                }
            }
        }

        protected override async Task OnTeamsMessagingExtensionConfigurationSettingAsync(ITurnContext<IInvokeActivity> turnContext, JObject settings, CancellationToken cancellationToken)
        {
            List<UserMapData> currentList = new List<UserMapData>();
            userMappingData.TryGetValue("search", out currentList);
            string userConfigSettings;

            if (settings["state"] != null)
            {
                userConfigSettings = settings["state"].ToString();
            }

            else
            {
                userConfigSettings = "";
            }

            var data = currentList.Find(e => e.AadId == turnContext.Activity.From.AadObjectId);
            var index = currentList.FindIndex(e => e.AadId == turnContext.Activity.From.AadObjectId);

            if(userConfigSettings == "ConnectWithFacebook")
            {
                SignInIndicator.is_fb_signed_in_search = true;
                data.isFacebookSignedIn = true;
                currentList[index] = data;
                userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);
            }

            else if(userConfigSettings == "ConnectWithGoogle")
            {
                SignInIndicatorGoogle.is_google_signed_in_search = true;
                data.isGoogleSignedIn = true;
                currentList[index] = data;
                userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);
            }

            else if (userConfigSettings == "DisconnectFromFacebook")
            {
                SignInIndicator.is_fb_signed_in_search = false;
                data.isFacebookSignedIn = false;
                data.FacebookId = null;
                data.FacebookToken = null;
                currentList[index] = data;
                userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);
            }

            else
            {
                SignInIndicatorGoogle.is_google_signed_in_search = false;
                data.isGoogleSignedIn = false;
                data.GoogleId = null;
                data.GoogleToken = null;
                currentList[index] = data;
                userMappingData.AddOrUpdate("search", currentList, (key, newvalue) => currentList);
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
            var state = action.State; // Check the state value
            var tokenResponse = await GetTokenResponse(turnContext, _botConnectionName, null, cancellationToken);

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
            else
            {
                var client = new SimpleGraphClient(tokenResponse.Token);

                var profile = await client.GetMeAsync();
                var photo = await client.GetPhotoAsync();
                var title = !string.IsNullOrEmpty(profile.JobTitle) ?
                        profile.JobTitle : "Unknown";
                var googleData = new GoogleData();
                var facebookData = new FacebookData();
                List<UserMapData> currentList = new List<UserMapData>();
                List<UserMapData> userDetailsList = new List<UserMapData>();
                userMappingData.TryGetValue("me", out currentList);
                if (currentList == null)
                {
                    var userDetails = new UserMapData()
                    {
                        AadId = turnContext.Activity.From.AadObjectId,
                        isAadSignedIn = true,
                        AadToken = tokenResponse.Token
                    };

                    googleData.isGoogleSignedIn = false;
                    facebookData.isFacebookSignedIn = false;
                    userDetailsList.Add(userDetails);
                    userMappingData.AddOrUpdate("me", userDetailsList, (key, newvalue) => currentList);

                    return new MessagingExtensionActionResponse
                    {
                        Task = new TaskModuleContinueResponse
                        {
                            Value = new TaskModuleTaskInfo
                            {
                                Card = GetMEResponseCard(profile, photo, title, facebookData, googleData),
                                Height = 250,
                                Width = 400,
                                Title = "Show profile card",
                            },
                        },
                    };
                }

                else
                {
                    var data = currentList.Find(e => e.AadId == turnContext.Activity.From.AadObjectId);
                    if (data == null)
                    {
                        var userDetails = new UserMapData()
                        {
                            AadId = turnContext.Activity.From.AadObjectId,
                            isAadSignedIn = true,
                            AadToken = tokenResponse.Token
                        };

                        googleData.isGoogleSignedIn = false;
                        facebookData.isFacebookSignedIn = false;
                        currentList.Add(userDetails);
                        userMappingData.AddOrUpdate("me", currentList, (key, newvalue) => currentList);

                        return new MessagingExtensionActionResponse
                        {
                            Task = new TaskModuleContinueResponse
                            {
                                Value = new TaskModuleTaskInfo
                                {
                                    Card = GetMEResponseCard(profile, photo, title, facebookData, googleData),
                                    Height = 250,
                                    Width = 400,
                                    Title = "Show profile card",
                                },
                            },
                        };
                    }
                    else
                    {
                        data.AadToken = tokenResponse.Token;
                        var index = currentList.FindIndex(e => e.AadId == turnContext.Activity.From.AadObjectId);
                        currentList[index] = data;
                        userMappingData.AddOrUpdate("me", currentList, (key, newvalue) => currentList);

                        if (data.isFacebookSignedIn)
                        {
                            FacebookProfile fbProfile = await FacebookHelper.GetFacebookProfileName(data.FacebookToken);
                            facebookData.isFacebookSignedIn = true;
                            facebookData.facebookName = fbProfile.Name;
                            facebookData.facebookProfileUrl = fbProfile.ProfilePicture;
                        }
                        else
                        {
                            facebookData.isFacebookSignedIn = false;
                        }

                        if (data.isGoogleSignedIn)
                        {
                            var client2 = new HttpClient();
                            client2.DefaultRequestHeaders.Accept.Clear();
                            client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + data.GoogleToken);

                            var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                            var jboject = JsonConvert.DeserializeObject(json);
                            var googleProfile = JObject.FromObject(jboject);
                            var state1 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                            List<UserData> items = ((JArray)state1).Select(x => new UserData
                            {
                                DisplayName = (string)x["displayName"]
                            }).ToList();

                            var state2 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                            List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                            {
                                Url = (string)x["url"]
                            }).ToList();

                            var state3 = (JArray)googleProfile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                            List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                            {
                                Value = (string)x["value"]
                            }).ToList();

                            var displayName = items[0].DisplayName;
                            var photoUrl = items2[0].Url;
                            var emailAddress = items3[0].Value;
                            googleData.isGoogleSignedIn = true;
                            googleData.googleName = displayName;
                            googleData.googleEmail = emailAddress;
                            googleData.googleProfileUrl = photoUrl;
                        }
                        else
                        {
                            googleData.isGoogleSignedIn = false;
                        }

                        return new MessagingExtensionActionResponse
                        {
                            Task = new TaskModuleContinueResponse
                            {
                                Value = new TaskModuleTaskInfo
                                {
                                    Card = GetMEResponseCard(profile, photo, title, facebookData, googleData),
                                    Height = 250,
                                    Width = 400,
                                    Title = "Show profile card",
                                },
                            },
                        };
                    }
                }
            }
        }

        private AdaptiveCard GetProfileCard(User profile, string userPhoto, string jobTitle, FacebookData facebookProfile, GoogleData googleProfile)
        {

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"User SSO details are",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri(userPhoto),
                Size = AdaptiveImageSize.Medium,
                Style = AdaptiveImageStyle.Person
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Hello! {profile.DisplayName}",
                Weight = AdaptiveTextWeight.Bolder,
                IsSubtle = true,
                Wrap = true
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Job title : {jobTitle}",
                Weight = AdaptiveTextWeight.Bolder,
                IsSubtle = true,
                Wrap = true
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Email : {profile.UserPrincipalName}",
                Weight = AdaptiveTextWeight.Bolder,
                IsSubtle = true,
                Wrap = true
            });

            if (facebookProfile.isFacebookSignedIn)
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Facebook profile details are",
                    Weight = AdaptiveTextWeight.Bolder,
                    IsSubtle = true,
                    Separator = true,
                    Wrap = true
                });

                card.Body.Add(new AdaptiveImage()
                {
                    Url = new Uri(facebookProfile.facebookProfileUrl.data.url),
                    Size = AdaptiveImageSize.Medium,
                    Style = AdaptiveImageStyle.Person
                });

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Hello! {facebookProfile.facebookName}",
                    Weight = AdaptiveTextWeight.Bolder,
                    IsSubtle = true,
                    Wrap = true
                });
            }


            if (googleProfile.isGoogleSignedIn)
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Google profile details are",
                    Weight = AdaptiveTextWeight.Bolder,
                    IsSubtle = true,
                    Separator = true,
                    Wrap = true
                });

                card.Body.Add(new AdaptiveImage()
                {
                    Url = new Uri(googleProfile.googleProfileUrl),
                    Size = AdaptiveImageSize.Medium,
                    Style = AdaptiveImageStyle.Person
                });

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Hello! {googleProfile.googleName}",
                    Weight = AdaptiveTextWeight.Bolder,
                    IsSubtle = true,
                    Wrap = true
                });
            }

            if (facebookProfile.isFacebookSignedIn && googleProfile.isGoogleSignedIn)
            {
                var url = $"{_applicationBaseUrl}/config?is_fb_signed_in={facebookProfile.isFacebookSignedIn}&is_google_signed_in={googleProfile.isGoogleSignedIn}";
                card.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Title = "Disconnect",
                    Url = new Uri(url)
                });
            }

            return card;
        }

        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, string connectionName, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(connectionName, turnContext.Activity as Activity, null, cancellationToken).ConfigureAwait(false);

            return resource.SignInLink;
        }

        private static Microsoft.Bot.Schema.Attachment GetMEResponseCard(User profile, string userPhoto, string jobTitle, FacebookData facebookProfile, GoogleData googleProfile)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"User SSO details are",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri(userPhoto),
                Size = AdaptiveImageSize.Medium,
                Style = AdaptiveImageStyle.Person
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Hello! {profile.DisplayName}",
                Weight = AdaptiveTextWeight.Bolder,
                IsSubtle = true,
                Wrap = true
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Job title : {jobTitle}",
                Weight = AdaptiveTextWeight.Bolder,
                IsSubtle = true,
                Wrap = true
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Email : {profile.UserPrincipalName}",
                Weight = AdaptiveTextWeight.Bolder,
                IsSubtle = true,
                Wrap = true
            });

            if (facebookProfile.isFacebookSignedIn)
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Facebook profile details are",
                    Weight = AdaptiveTextWeight.Bolder,
                    IsSubtle = true,
                    Separator = true,
                    Wrap = true
                });

                card.Body.Add(new AdaptiveImage()
                {
                    Url = new Uri(facebookProfile.facebookProfileUrl.data.url),
                    Size = AdaptiveImageSize.Medium,
                    Style = AdaptiveImageStyle.Person
                });

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Hello! {facebookProfile.facebookName}",
                    Weight = AdaptiveTextWeight.Bolder,
                    IsSubtle = true,
                    Wrap = true
                });
            }


            if (googleProfile.isGoogleSignedIn)
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Google profile details are",
                    Weight = AdaptiveTextWeight.Bolder,
                    IsSubtle = true,
                    Separator = true,
                    Wrap = true
                });

                card.Body.Add(new AdaptiveImage()
                {
                    Url = new Uri(googleProfile.googleProfileUrl),
                    Size = AdaptiveImageSize.Medium,
                    Style = AdaptiveImageStyle.Person
                });

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Hello! {googleProfile.googleName}",
                    Weight = AdaptiveTextWeight.Bolder,
                    IsSubtle = true,
                    Wrap = true
                });
            }

            if (!facebookProfile.isFacebookSignedIn)
            {
                card.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = "Connect with facebook",
                    Data = new AdaptiveCardAction
                    {
                        Id = "connectWithFacebook"
                    },
                });
            }

            if (facebookProfile.isFacebookSignedIn)
            {
                card.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = "Disconnect from facebook",
                    Data = new AdaptiveCardAction
                    {
                        Id = "dicconnectFromFacebook"
                    },
                });
            }

            if (!googleProfile.isGoogleSignedIn)
            {
                card.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = "Connect with google",
                    Data = new AdaptiveCardAction
                    {
                        Id = "connectWithGoogle"
                    },
                });
            }

            if (googleProfile.isGoogleSignedIn)
            {
                card.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = "Disconnect from google",
                    Data = new AdaptiveCardAction
                    {
                        Id = "disConnectFromGoogle"
                    },
                });
            }

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }


        // Get token response.
        private async Task<TokenResponse> GetTokenResponse(ITurnContext<IInvokeActivity> turnContext, string connectionName, string state, CancellationToken cancellationToken)
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
    }
}