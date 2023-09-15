// <copyright file="MainDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
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

        private async Task<ResourceResponse> TagMentionAdaptivecard(WaterfallStepContext stepContext, CancellationToken cancellationToken, string tagName, string tagId)
        {
            var adaptiveCardTemplate = Path.Combine(".", "Resources", "UserMentionCardTemplate.json");
            var templateJSON = File.ReadAllText(adaptiveCardTemplate);
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(templateJSON);
            var memberData = new
            {
                tagId = tagId,
                tagName = tagName
            };
            string cardJSON = template.Expand(memberData);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };
            return await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
        }

        private async Task<DialogTurnResult> MentionTagAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            bool tagExists = false;
            if (tokenResponse?.Token != null)
            {
                try
                {
                    stepContext.Context.Activity.RemoveRecipientMention();
                    if (stepContext.Context.Activity.Text.Trim().ToLower().Contains("<at>"))
                    {
                        var tagName = stepContext.Context.Activity.Text.Replace("<at>", string.Empty).Replace("</at>", string.Empty).Trim();
                        var tagID = stepContext.Context.Activity.Entities[1].Properties["mentioned"]["id"].ToString();
                        await TagMentionAdaptivecard(stepContext, cancellationToken, tagName, tagID);

                    }
                    else if (stepContext.Context.Activity.Text.Trim().ToLower() != "")
                    {
                        SimpleGraphClient client = null;
                        TeamDetails teamDetails = null;
                        try
                        {
                            // Pull in the data from the Microsoft Graph.
                            client = new SimpleGraphClient(tokenResponse.Token);
                            teamDetails = await TeamsInfo.GetTeamDetailsAsync(stepContext.Context, stepContext.Context.Activity.TeamsGetTeamInfo().Id, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await stepContext.Context.SendActivityAsync("You don't have Graph API permissions to fetch tag's information. Please use this command to mention a tag: \"`@<Bot-name>  @<your-tag>`\" to experience tag mention using bot.");
                        }
                        var result = await client.GetTag(teamDetails.AadGroupId);
                        foreach (var tagDetails in result.CurrentPage)
                        {
                            if (tagDetails.DisplayName == stepContext.Context.Activity.Text.Trim().ToLower())
                            {
                                tagExists = true;
                                await TagMentionAdaptivecard(stepContext, cancellationToken, tagDetails.DisplayName, tagDetails.Id);
                                break;
                            }
                        }
                        if (!tagExists)
                        {
                            await stepContext.Context.SendActivityAsync("Provided tag name is not available in this team. Please try with another tag name or create a new tag.");
                        }
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync("Please provide a tag name while mentioning the bot as \"`@<Bot-name> <your-tag-name>`\" or mention a tag as \"`@<Bot-name> @<your-tag>`\"");
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