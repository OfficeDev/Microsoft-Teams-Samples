using AdaptiveCards;
using AppCompleteAuth.helper;
using AppCompleteAuth.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppCompleteAuth.Dialogs
{
    public class FacebookAuthDialog : LogoutDialog
    {
        private readonly ConcurrentDictionary<string, Token> _Token;
        public FacebookAuthDialog(string configuration, ConcurrentDictionary<string, Token> token) : base(nameof(FacebookAuthDialog), configuration)
        {
            _Token = token;
            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Login to facebook",
                    Title = "Log In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync,
                UserInfoCardAsync
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
            // Getting the token from the previous step.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                Token token = new Token
                {
                    AccessToken = tokenResponse.Token
                };

                _Token.AddOrUpdate("facebookToken", token, (key, newValue) => token);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);
                return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What is your preferred user name?"),
                },
                cancellationToken);
                
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> UserInfoCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userName = stepContext.Result as string;
            // Pull in the data from the Microsoft Graph.
            var token = new Token();
            _Token.TryGetValue("facebookToken", out token);
            // Getting basic facebook profile details.
            FacebookProfile profile = await FacebookHelper.GetFacebookProfileName(token.AccessToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetProfileCard(profile, userName)));

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        // Create facebook profile card.
        private Attachment GetProfileCard(FacebookProfile profile, string userName)
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
                            },
                            new AdaptiveTextBlock()
                            {
                                Text =  $"User name : {userName}",
                                Weight = AdaptiveTextWeight.Bolder,
                                IsSubtle = true
                            }
                        },
                        Width ="stretch"
                    }
                }
            });

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}