using AdaptiveCards;
using IdentityLinkingWithSSO.helper;
using IdentityLinkingWithSSO.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
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

namespace IdentityLinkingWithSSO.Dialogs
{
    public class GoogleAuthDialog : LogoutDialog
    {
        private readonly ConcurrentDictionary<string, List<UserMapData>> mappingData;
        public GoogleAuthDialog(string configuration, ConcurrentDictionary<string, List<UserMapData>> data) : base(nameof(GoogleAuthDialog), configuration)
        {
            mappingData = data;
            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Login to google",
                    Title = "Log In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        // Shows the OAuthPrompt to the user to login if not already logged in.
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<UserMapData> currentList = new List<UserMapData>();
            var attachmentList = new List<Microsoft.Bot.Schema.Attachment>();
            Microsoft.Bot.Schema.Attachment userCard;
            Microsoft.Bot.Schema.Attachment facebookCard;
            Microsoft.Bot.Schema.Attachment googleCard;
            // Getting the token from the previous step.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                // Getting basic facebook profile details.
                var client2 = new HttpClient();
                client2.DefaultRequestHeaders.Accept.Clear();
                client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenResponse.Token);

                var json = await client2.GetStringAsync(String.Format("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls")).ConfigureAwait(false);
                var jboject = JsonConvert.DeserializeObject(json);
                var profile = JObject.FromObject(jboject);
                var state = (JArray)profile.ToObject<CardTaskFetchValue<JArray>>()?.Names;
                List<UserData> items = ((JArray)state).Select(x => new UserData
                {
                    DisplayName = (string)x["displayName"]
                }).ToList();

                var state2 = (JArray)profile.ToObject<CardTaskFetchValue<JArray>>()?.Photos;
                List<UserData> items2 = ((JArray)state2).Select(x => new UserData
                {
                    Url = (string)x["url"]
                }).ToList();

                var state3 = (JArray)profile.ToObject<CardTaskFetchValue<JArray>>()?.EmailAddresses;
                List<UserData> items3 = ((JArray)state3).Select(x => new UserData
                {
                    Value = (string)x["value"]
                }).ToList();

                var displayName = items[0].DisplayName;
                var photoUrl = items2[0].Url;
                var emailAddress = items3[0].Value;

                mappingData.TryGetValue("key", out currentList);
                var data = currentList.Find(e => e.AadId == stepContext.Context.Activity.From.AadObjectId);
                data.GoogleToken = tokenResponse.Token;
                data.GoogleId = emailAddress;
                data.isGoogleSignedIn = true;
                var index = currentList.FindIndex(e => e.AadId == stepContext.Context.Activity.From.AadObjectId);
                currentList[index] = data;
                var client = new SimpleGraphClient(data.AadToken);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ?
                            me.JobTitle : "Unknown";
                var photo = await client.GetPhotoAsync();
                userCard = GetProfileCard(me, photo, title);
                if(data.isFacebookSignedIn)
                {
                    FacebookProfile fbData = await FacebookHelper.GetFacebookProfileName(data.FacebookToken);
                    facebookCard = GetProfileCard(fbData);
                }

                else
                {
                    facebookCard = GetThumbnailCardFacebook().ToAttachment();
                }
              //  var faceBookCard = GetProfileCard(profile);
                googleCard = GetGoogleProfile(displayName, photoUrl, emailAddress);
                attachmentList = new List<Microsoft.Bot.Schema.Attachment>();
                attachmentList.Add(userCard);
                attachmentList.Add(facebookCard);
                attachmentList.Add(googleCard);
                var reply = MessageFactory.Attachment(attachmentList);
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        // Get user profile card.
        private Microsoft.Bot.Schema.Attachment GetProfileCard(User profile, string photo, string title)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"User Profile details are",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri(photo),
                Size = AdaptiveImageSize.Medium,
                Style = AdaptiveImageStyle.Person
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Hello! {profile.DisplayName}",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Job title: {profile.JobTitle}",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Email: {profile.UserPrincipalName}",
                Size = AdaptiveTextSize.Default
            });

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        // Create facebook profile card.
        private Microsoft.Bot.Schema.Attachment GetProfileCard(FacebookProfile profile)
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

            card.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "DisConnect",
                Data = new AdaptiveCardAction
                {
                    MsteamsCardAction = new CardAction
                    {
                        Type = "imBack",
                        Value = "DisConnectFromFacebook",
                    }
                },
            });

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        public static ThumbnailCard GetThumbnailCardFacebook()
        {
            var thumbnailCard = new ThumbnailCard
            {
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Connect to facebook", value: "connectToFacebook") }
            };

            return thumbnailCard;
        }

        // Create facebook profile card.
        private Microsoft.Bot.Schema.Attachment GetGoogleProfile(string displayName, string photoUrl, string emailAddress)
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
                                Url = new Uri(photoUrl),
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
                                Text =  $"Hello! {displayName}",
                                Weight = AdaptiveTextWeight.Bolder,
                                IsSubtle = true
                            }
                        },
                        Width ="stretch"
                    }
                }
            });

            card.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "DisConnect",
                Data = new AdaptiveCardAction
                {
                    MsteamsCardAction = new CardAction
                    {
                        Type = "imBack",
                        Value = "DisConnectFromGoogle",
                    }
                },
            });

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}