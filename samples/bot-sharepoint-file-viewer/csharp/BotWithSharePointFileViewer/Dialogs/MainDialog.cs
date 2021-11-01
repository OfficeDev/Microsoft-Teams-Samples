// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using BotWithSharePointFileViewer.helper;
using BotWithSharePointFileViewer.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace BotWithSharePointFileViewer.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        public readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, TokenState> _Token;

        public MainDialog(IConfiguration configuration,ConcurrentDictionary<string, TokenState> token)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _configuration = configuration;
            _Token = token;

            AddDialog(new TokenExchangeOAuthPrompt(
                nameof(TokenExchangeOAuthPrompt),
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
                LoginStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Method to invoke oauth flow.
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(TokenExchangeOAuthPrompt), null, cancellationToken);
        }

        // Invoked after success of prompt step async.
        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                if (stepContext.Context.Activity.Text!= null)
                {
                    if (stepContext.Context.Activity.Text.ToLower().Trim() == "viewfile")
                    {
                        var fileNameList = await SharePointFileHelper.GetSharePointFile(tokenResponse, _configuration["SharePointSiteName"], _configuration["SharePointTenantName"] + ":");

                        if (fileNameList.Count == 0)
                        {
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text("No files found. Please type 'uploadfile' to upload file to SharePoint site"), cancellationToken);
                        }
                        else
                        {
                            var sharePointTenantName = _configuration["SharePointTenantName"];
                            var sharePointSiteName = _configuration["SharePointSiteName"];
                            var fileUrl = "";
                            var actions = new List<AdaptiveAction>();

                            foreach (var file in fileNameList)
                            {
                                var extension = file.Split('.')[1];
                                fileUrl = $"https://teams.microsoft.com/_#/{extension}/viewer/teams/https:~2F~2F{sharePointTenantName}~2Fsites~2F{sharePointSiteName}~2FShared%20Documents~2F{file}";
                                actions.Add(new AdaptiveOpenUrlAction
                                {
                                    Title = file.Split('.')[0],
                                    Url = new Uri(fileUrl),

                                });
                            }

                            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForFileViewerOption(actions)), cancellationToken);
                        }

                        return await stepContext.EndDialogAsync();
                    }
                    else if (stepContext.Context.Activity.Text.ToLower().Trim() == "uploadfile")
                    {
                        TokenState token = new TokenState
                        {
                            AccessToken = tokenResponse.Token
                        };

                        _Token.AddOrUpdate("token", token, (key, newValue) => token);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForUploadFileOption()), cancellationToken);

                        return await stepContext.EndDialogAsync();
                    }    
                }                  
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please type 'uploadfile' to upload file to SharePoint site or 'viewfile' to get card for file viewer"), cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        /// <summary>
        /// Sample Adaptive card for file viewer.
        /// </summary>
        private Attachment GetAdaptiveCardForFileViewerOption(List<AdaptiveAction> actions)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Click on button to view file in file viewer",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    }
                },
                Actions = actions
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Sample Adaptive card for upload file.
        /// </summary>
        private Attachment GetAdaptiveCardForUploadFileOption()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Click on button to upload file to SharePoint site",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Upload File",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                            Id="upload"
                        },
                    },
                }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}