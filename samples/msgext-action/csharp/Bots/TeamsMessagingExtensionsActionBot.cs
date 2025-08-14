// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.BotBuilderSamples.Helpers;
using Microsoft.BotBuilderSamples.Models;

namespace Microsoft.BotBuilderSamples.Bots
{
    /// <summary>
    /// TeamsMessagingExtensionsActionBot handles messaging extension actions for Teams.
    /// </summary>
    public class TeamsMessagingExtensionsActionBot : TeamsActivityHandler
    {
        private readonly string baseUrl;

        public TeamsMessagingExtensionsActionBot(IConfiguration configuration) : base()
        {
            this.baseUrl = configuration["BaseUrl"];
        }

        /// <summary>
        /// Handles the submission of messaging extension actions.
        /// </summary>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            return action.CommandId switch
            {
                "createCard" => CreateCardCommand(turnContext, action),
                "shareMessage" => ShareMessageCommand(turnContext, action),
                "webView" => WebViewResponse(turnContext, action),
                "createAdaptiveCard" => CreateAdaptiveCardResponse(turnContext, action),
                "razorView" => RazorViewResponse(turnContext, action),
                "HTML" => ShareHtmlCard(turnContext, action),
                _ => await Task.FromResult(new MessagingExtensionActionResponse())
            };
        }

        private MessagingExtensionActionResponse RazorViewResponse(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var cardData = JsonConvert.DeserializeObject<RazorViewResponse>(action.Data.ToString());
            var card = new HeroCard
            {
                Title = "Requested User: " + turnContext.Activity.From.Name,
                Text = cardData.DisplayData,
            };

            var attachments = new List<MessagingExtensionAttachment>
                {
                    new MessagingExtensionAttachment
                    {
                        Content = card,
                        ContentType = HeroCard.ContentType,
                        Preview = card.ToAttachment(),
                    }
                };

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        private MessagingExtensionActionResponse CreateCardCommand(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var createCardData = ((JObject)action.Data).ToObject<CardResponse>();

            var card = new HeroCard
            {
                Title = createCardData.Title,
                Subtitle = createCardData.Subtitle,
                Text = createCardData.Text,
            };

            var attachments = new List<MessagingExtensionAttachment>
                {
                    new MessagingExtensionAttachment
                    {
                        Content = card,
                        ContentType = HeroCard.ContentType,
                        Preview = card.ToAttachment(),
                    }
                };

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        private MessagingExtensionActionResponse ShareMessageCommand(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var heroCard = new HeroCard
            {
                Title = $"{action.MessagePayload.From?.User?.DisplayName} originally sent this message:",
                Text = action.MessagePayload.Body.Content,
            };

            if (action.MessagePayload.Attachments != null && action.MessagePayload.Attachments.Count > 0)
            {
                heroCard.Subtitle = $"({action.MessagePayload.Attachments.Count} Attachments not included)";
            }

            var includeImage = ((JObject)action.Data)["includeImage"]?.ToString();
            if (string.Equals(includeImage, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                heroCard.Images = new List<CardImage>
                    {
                        new CardImage { Url = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU" },
                    };
            }

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment>
                        {
                            new MessagingExtensionAttachment
                            {
                                Content = heroCard,
                                ContentType = HeroCard.ContentType,
                                Preview = heroCard.ToAttachment(),
                            },
                        },
                },
            };
        }

        private MessagingExtensionActionResponse WebViewResponse(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var cardData = JsonConvert.DeserializeObject<CustomFormResponse>(action.Data.ToString());
            var imgUrl = baseUrl + "/profile-image.png";

            var card = new ThumbnailCard
            {
                Title = "ID: " + cardData.EmpId,
                Subtitle = "Name: " + cardData.EmpName,
                Text = "E-Mail: " + cardData.EmpEmail,
                Images = new List<CardImage> { new CardImage { Url = imgUrl } },
            };

            var attachments = new List<MessagingExtensionAttachment>
                {
                    new MessagingExtensionAttachment
                    {
                        Content = card,
                        ContentType = ThumbnailCard.ContentType,
                        Preview = card.ToAttachment(),
                    }
                };

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        private MessagingExtensionActionResponse CreateAdaptiveCardResponse(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var createCardResponse = ((JObject)action.Data).ToObject<CardResponse>();
            var attachments = CardHelper.CreateAdaptiveCardAttachment(action, createCardResponse);

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        private MessagingExtensionActionResponse ShareHtmlCard(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var createCardResponse = ((JObject)action.Data).ToObject<CardResponse>();
            var attachments = CardHelper.CreateAdaptiveCardAttachmentForHtml(action, createCardResponse);

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        /// <summary>
        /// Handles the fetching of tasks for messaging extensions.
        /// </summary>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            return action.CommandId switch
            {
                "webView" => EmpDetails(turnContext, action),
                "HTML" => TaskModuleHtmlPage(turnContext, action),
                "razorView" => DateDayInfo(turnContext, action),
                _ => await HandleDefaultFetchTaskAsync(turnContext, cancellationToken)
            };
        }

        private async Task<MessagingExtensionActionResponse> HandleDefaultFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            string memberName;
            try
            {
                var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                memberName = member.Name;
            }
            catch (ErrorResponseException ex)
            {
                if (ex.Body.Error.Code == "BotNotInConversationRoster")
                {
                    return new MessagingExtensionActionResponse
                    {
                        Task = new TaskModuleContinueResponse
                        {
                            Value = new TaskModuleTaskInfo
                            {
                                Card = GetAdaptiveCardAttachmentFromFile("justintimeinstallation.json"),
                                Height = 200,
                                Width = 400,
                                Title = "Adaptive Card - App Installation",
                            },
                        },
                    };
                }
                throw;
            }

            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = GetAdaptiveCardAttachmentFromFile("adaptiveCard.json"),
                        Height = 200,
                        Width = 400,
                        Title = $"Welcome {memberName}",
                    },
                },
            };
        }

        private MessagingExtensionActionResponse DateDayInfo(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Height = 175,
                        Width = 300,
                        Title = "Task Module Razor View",
                        Url = baseUrl + "/Home/RazorView",
                    },
                },
            };
        }

        private MessagingExtensionActionResponse TaskModuleHtmlPage(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Height = 200,
                        Width = 400,
                        Title = "Task Module HTML Page",
                        Url = baseUrl + "/Home/HtmlPage",
                    },
                },
            };
        }

        private MessagingExtensionActionResponse EmpDetails(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Height = 300,
                        Width = 450,
                        Title = "Task Module WebView",
                        Url = baseUrl + "/Home/CustomForm",
                    },
                },
            };
        }

        private static Attachment GetAdaptiveCardAttachmentFromFile(string fileName)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", fileName));

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
        }
    }
}
