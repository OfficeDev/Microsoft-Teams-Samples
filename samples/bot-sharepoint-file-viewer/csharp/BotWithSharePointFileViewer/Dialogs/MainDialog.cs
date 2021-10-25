// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using BotWithSharePointFileViewer.helper;
using BotWithSharePointFileViewer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace BotWithSharePointFileViewer.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        public readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public MainDialog(IConfiguration configuration, IWebHostEnvironment env)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _configuration = configuration;
            _env = env;

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please login",
                    Title = "Login",
                    Timeout = 300000, // User has 5 minutes to login
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

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                if (stepContext.Context.Activity.Text == "viewfile")
                {
                    var fileNameList = await GetSharePointFileHelper.GetSharePointFile(stepContext.Context, tokenResponse, _configuration["SharepointSiteName"], _configuration["SharepointTenantName"] + ":");
                    var sharePointTenantName = _configuration["SharepointTenantName"];
                    var sharepointSiteName = _configuration["SharepointSiteName"];
                    var fileUrl = "";
                    var actions = new List<AdaptiveAction>();

                    foreach (var file in fileNameList)
                    {
                        var extension = file.Split('.')[1];
                        fileUrl = $"https://teams.microsoft.com/_#/{extension}/viewer/teams/https:~2F~2F{sharePointTenantName}~2Fsites~2F{sharepointSiteName}~2FShared%20Documents~2F{file}";
                        actions.Add(new AdaptiveOpenUrlAction
                        {
                            Title = file.Split('.')[0],
                            Url = new Uri(fileUrl),

                        });
 
                    }
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForFileViewerOption(actions)), cancellationToken);

                    return await stepContext.EndDialogAsync();
                }
                else if (stepContext.Context.Activity.Text == "upload")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForUploadFileOption()), cancellationToken);

                    return await stepContext.EndDialogAsync();
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please type 'getchat' command in the groupchat where the bot is added to fetch messages."), cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }

        /// <summary>
        /// Sample Adaptive card for file viewer.
        /// </summary>
        private Microsoft.Bot.Schema.Attachment GetAdaptiveCardForFileViewerOption(List<AdaptiveAction> actions)
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

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Sample Adaptive card for file viewer.
        /// </summary>
        private Microsoft.Bot.Schema.Attachment GetAdaptiveCardForUploadFileOption()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Click on button to upload file in sharepoint site",
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

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}