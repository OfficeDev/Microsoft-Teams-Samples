using AdaptiveCards;
using AppCompleteAuth.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppCompleteAuth.Dialogs
{
    public class BotSsoAuthDialog : LogoutDialog
    {
        private readonly ConcurrentDictionary<string, Token> _Token;
        public BotSsoAuthDialog(string configuration, ConcurrentDictionary<string, Token> token) : base(nameof(BotSsoAuthDialog), configuration)
        {
            _Token = token;
            AddDialog(new OAuthPrompt(
                 nameof(OAuthPrompt),
                 new OAuthPromptSettings
                 {
                     ConnectionName = ConnectionName,
                     Text = "Please login",
                     Title = "Sign In",
                     Timeout = 1000 * 60 * 1, // User has 5 minutes to login (1000 * 60 * 5)

                    //EndOnInvalidMessage = true
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

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        // Invoked after success of prompt step async.
        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step.
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);
                Token token = new Token
                {
                    AccessToken = tokenResponse.Token
                };

                _Token.AddOrUpdate("token", token, (key, newValue) => token);
                return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What is your preferred user name?"),
                },
                cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> UserInfoCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userName = stepContext.Result as string;
            // Pull in the data from the Microsoft Graph.
            var token = new Token();
            _Token.TryGetValue("token", out token);
            var client = new SimpleGraphClient(token.AccessToken);
            var me = await client.GetMeAsync();
            var title = !string.IsNullOrEmpty(me.JobTitle) ?
                        me.JobTitle : "Unknown";
            var photo = await client.GetPhotoAsync();
            var reply = MessageFactory.Attachment(GetProfileCard(me, photo, title, userName));
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.EndDialogAsync();
        }

        // Get user profile card.
        private Microsoft.Bot.Schema.Attachment GetProfileCard(User profile, string photo, string title, string userName)
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
                    new AdaptiveFact
                    {
                        Title = "User name :",
                        Value = $"{userName}"
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