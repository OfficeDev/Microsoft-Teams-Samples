// <copyright file="MainDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger _logger;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _logger = logger;

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                    EndOnInvalidMessage = true
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                MentionTagAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PromptStepAsync() called.");
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> MentionTagAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                try
                {
                    stepContext.Context.Activity.RemoveRecipientMention();
                    if (stepContext.Context.Activity.Text.Trim().ToLower() == "mentiontag")
                    {
                        // Pull in the data from the Microsoft Graph.
                        var client = new SimpleGraphClient(tokenResponse.Token);
                        var teamID = await TeamsInfo.GetTeamDetailsAsync(stepContext.Context, stepContext.Context.Activity.TeamsGetTeamInfo().Id, cancellationToken);
                        var result = await client.GetTag(teamID.AadGroupId);
                        var adaptiveCardTemplate = Path.Combine(".", "Resources", "UserMentionCardTemplate.json");
                        var templateJSON = File.ReadAllText(adaptiveCardTemplate);
                        AdaptiveCardTemplate template = new AdaptiveCardTemplate(templateJSON);
                        var memberData = new
                        {
                            tagId = result.CurrentPage[0].Id,
                            tagName = result.CurrentPage[0].DisplayName
                        };
                        string cardJSON = template.Expand(memberData);
                        var adaptiveCardAttachment = new Attachment
                        {
                            ContentType = "application/vnd.microsoft.card.adaptive",
                            Content = JsonConvert.DeserializeObject(cardJSON),
                        };
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync("Please use **MentionTag** command to demonstrate a tag in this team");
                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while processing your request.", ex.Message);
                }
            }
            else
            {
                _logger.LogInformation("Response token is null or empty.");
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}