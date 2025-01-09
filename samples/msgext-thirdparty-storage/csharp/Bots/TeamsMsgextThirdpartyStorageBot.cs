// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsMsgextThirdpartyStorageBot : TeamsActivityHandler
    {
        public readonly string baseUrl;

        public TeamsMsgextThirdpartyStorageBot(IConfiguration configuration) : base()
        {
            this.baseUrl = configuration["BaseUrl"];
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            switch (action.CommandId)
            {
                case "createWithPreview":
                    return WebViewResponse(turnContext, action);
            }
            return await Task.FromResult(new MessagingExtensionActionResponse());
        }


        private MessagingExtensionActionResponse WebViewResponse(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            // Parse the uploaded data from action.Data
            var uploadedData = JArray.Parse(action.Data.ToString());

            // Create card elements
            var cardBody = new List<AdaptiveElement>{};

            foreach (var file in uploadedData)
            {
                string name = file["name"].ToString();
                string type = file["type"].ToString();
                string fileTypeIconUrl;

                // Determine the icon based on file type
                if (type.Contains("spreadsheet"))
                    fileTypeIconUrl = baseUrl + "/icons/ExcelIcon.png"; // Replace with the correct URL
                else if (type.Contains("pdf"))
                    fileTypeIconUrl = baseUrl + "/icons/PdfIcon.png"; // Replace with the correct URL
                else if (type.Contains("wordprocessing"))
                    fileTypeIconUrl = baseUrl + "/icons/WordIcons.png"; // Replace with the correct URL
                else if (type.Contains("image"))
                    fileTypeIconUrl = baseUrl + "/icons/ImageIcon.png"; // Replace with the correct URL
                else
                    fileTypeIconUrl = baseUrl + "/icons/ImageIcon.png"; // Replace with the correct URL

                // Add each file detail to the card
                cardBody.Add(new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Width = "auto",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveImage
                                {
                                    Url = new Uri(fileTypeIconUrl),
                                    Size = AdaptiveImageSize.Small
                                }
                            }
                        },
                        new AdaptiveColumn
                        {
                            Width = "stretch",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = name,
                                    Wrap = true
                                }
                            }
                        }
                    }
                });
            }

            // Create Adaptive Card
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4))
            {
                Body = cardBody
            };

            // Return MessagingExtensionActionResponse
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
                            ContentType = AdaptiveCard.ContentType,
                            Content = adaptiveCard
                        }
                    }
                }
            };
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
         ITurnContext<IInvokeActivity> turnContext,
         MessagingExtensionAction action,
         CancellationToken cancellationToken)
        {
            return EmpDetails(turnContext, action);
        }


        private MessagingExtensionActionResponse EmpDetails(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var response = new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Height = 350,
                        Width = 700,
                        Title = "Task Module WebView",
                        Url = baseUrl + "/CustomForm",
                    },
                },
            };
            return response;
        }

    }
}
