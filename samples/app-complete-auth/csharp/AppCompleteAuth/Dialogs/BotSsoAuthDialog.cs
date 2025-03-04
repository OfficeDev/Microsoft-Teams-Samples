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
    /// <summary>
    /// BotSsoAuthDialog class to handle Single Sign-On (SSO) authentication.
    /// </summary>
    public class BotSsoAuthDialog : LogoutDialog
    {
        private readonly ConcurrentDictionary<string, Token> _token;

        public BotSsoAuthDialog(string configuration, ConcurrentDictionary<string, Token> token)
            : base(nameof(BotSsoAuthDialog), configuration)
        {
            _token = token;
            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please login",
                    Title = "Sign In",
                    Timeout = 1000 * 60 * 1, // User has 1 minute to login
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

        /// <summary>
        /// Prompts the user to login.
        /// </summary>
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        /// <summary>
        /// Handles the login step.
        /// </summary>
        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);
                var token = new Token
                {
                    AccessToken = tokenResponse.Token
                };

                _token.AddOrUpdate("token", token, (key, newValue) => token);
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

        /// <summary>
        /// Displays the user information card.
        /// </summary>
        private async Task<DialogTurnResult> UserInfoCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userName = stepContext.Result as string;
            _token.TryGetValue("token", out var token);
            var client = new SimpleGraphClient(token.AccessToken);
            var me = await client.GetMeAsync();
            var title = !string.IsNullOrEmpty(me.JobTitle) ? me.JobTitle : "Unknown";
            var photo = await client.GetPhotoAsync();
            var reply = MessageFactory.Attachment(GetProfileCard(me, photo, title, userName));
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.EndDialogAsync();
        }

        /// <summary>
        /// Creates a user profile card.
        /// </summary>
        private Microsoft.Bot.Schema.Attachment GetProfileCard(User profile, string photo, string title, string userName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = "User Information",
                            Size = AdaptiveTextSize.Default
                        },
                        new AdaptiveColumnSet
                        {
                            Columns = new List<AdaptiveColumn>
                            {
                                new AdaptiveColumn
                                {
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveImage
                                        {
                                            Url = new Uri(photo),
                                            Size = AdaptiveImageSize.Medium,
                                            Style = AdaptiveImageStyle.Person
                                        }
                                    },
                                    Width = "auto"
                                },
                                new AdaptiveColumn
                                {
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Text = $"Hello! {profile.DisplayName}",
                                            Weight = AdaptiveTextWeight.Bolder,
                                            IsSubtle = true
                                        }
                                    },
                                    Width = "stretch"
                                }
                            }
                        },
                        new AdaptiveFactSet
                        {
                            Separator = true,
                            Facts = new List<AdaptiveFact>
                            {
                                new AdaptiveFact { Title = "Job title:", Value = title },
                                new AdaptiveFact { Title = "Email:", Value = profile.UserPrincipalName },
                                new AdaptiveFact { Title = "User name:", Value = userName }
                            }
                        }
                    }
            };

            return new Microsoft.Bot.Schema.Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }
    }
}