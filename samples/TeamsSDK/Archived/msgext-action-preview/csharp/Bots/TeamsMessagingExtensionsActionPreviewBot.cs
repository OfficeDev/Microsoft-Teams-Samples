// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsMessagingExtensionsActionPreviewBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {
                var obj = (JObject)turnContext.Activity.Value;
                var answer = obj["Answer"]?.ToString();
                var choices = obj["Choices"]?.ToString();
                await turnContext.SendActivityAsync(MessageFactory.Text($"{turnContext.Activity.From.Name} answered '{answer}' and chose '{choices}'."), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Hello from the TeamsMessagingExtensionsActionPreviewBot."), cancellationToken);
            }
        }

        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            var adaptiveCardEditor = AdaptiveCardHelper.CreateAdaptiveCardEditor();

            return Task.FromResult(new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = new Attachment
                        {
                            Content = adaptiveCardEditor,
                            ContentType = AdaptiveCard.ContentType,
                        },
                        Height = 450,
                        Width = 500,
                        Title = "Task Module Fetch Example",
                    },
                },
            });
        }

        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            var exampleData = JsonConvert.DeserializeObject<ExampleData>(action.Data.ToString());
            var adaptiveCard = AdaptiveCardHelper.CreateAdaptiveCard(exampleData);

            return Task.FromResult(new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "botMessagePreview",
                    ActivityPreview = MessageFactory.Attachment(new Attachment
                    {
                        Content = adaptiveCard,
                        ContentType = AdaptiveCard.ContentType,
                    }) as Activity,
                },
            });
        }

        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            var activityPreview = action.BotActivityPreview[0];
            var attachmentContent = activityPreview.Attachments[0].Content;
            var previewedCard = JsonConvert.DeserializeObject<AdaptiveCard>(attachmentContent.ToString(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var exampleData = AdaptiveCardHelper.CreateExampleData(previewedCard);

            var adaptiveCardEditor = AdaptiveCardHelper.CreateAdaptiveCardEditor(exampleData);

            return Task.FromResult(new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = new Attachment
                        {
                            Content = adaptiveCardEditor,
                            ContentType = AdaptiveCard.ContentType,
                        },
                        Height = 450,
                        Width = 500,
                        Title = "Task Module Fetch Example",
                    },
                },
            });
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            var activityPreview = action.BotActivityPreview[0];
            var attachmentContent = activityPreview.Attachments[0].Content;
            var previewedCard = JsonConvert.DeserializeObject<AdaptiveCard>(attachmentContent.ToString(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var exampleData = AdaptiveCardHelper.CreateExampleData(previewedCard);

            var adaptiveCard = AdaptiveCardHelper.CreateAdaptiveCard(exampleData);
            var message = MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = adaptiveCard });

            if (exampleData.UserAttributionSelect == "true")
            {
                message.ChannelData = new
                {
                    OnBehalfOf = new[]
                    {
                        new
                        {
                            ItemId = 0,
                            MentionType = "person",
                            Mri = turnContext.Activity.From.Id,
                            DisplayName = turnContext.Activity.From.Name
                        }
                    }
                };
            }

            await turnContext.SendActivityAsync(message, cancellationToken);

            return null;
        }

        protected override async Task OnTeamsMessagingExtensionCardButtonClickedAsync(
            ITurnContext<IInvokeActivity> turnContext,
            JObject obj,
            CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionCardButtonClickedAsync Value: " + JsonConvert.SerializeObject(turnContext.Activity.Value));
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
