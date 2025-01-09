// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// A bot that handles Teams messaging extensions for third-party storage integration.
    /// </summary>
    public class TeamsMsgextThirdpartyStorageBot : TeamsActivityHandler
    {
        private readonly string baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMsgextThirdpartyStorageBot"/> class.
        /// </summary>
        /// <param name="configuration">Configuration object to access app settings.</param>
        public TeamsMsgextThirdpartyStorageBot(IConfiguration configuration)
        {
            this.baseUrl = configuration["BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration), "BaseUrl is not configured.");
        }

        /// <summary>
        /// Handles the submit action for messaging extensions.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the messaging extension response.</returns>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            try
            {
                switch (action.CommandId)
                {
                    case "createWithPreview":
                        return CreateWebViewResponse(turnContext, action);
                    default:
                        throw new NotSupportedException($"Command '{action.CommandId}' is not supported.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in OnTeamsMessagingExtensionSubmitActionAsync: {ex.Message}");
                return new MessagingExtensionActionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "message",
                        Text = "An error occurred while processing your request. Please try again later."
                    }
                };
            }
        }

        /// <summary>
        /// Generates a messaging extension action response containing an Adaptive Card with file details.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <returns>A MessagingExtensionActionResponse with an Adaptive Card.</returns>
        private MessagingExtensionActionResponse CreateWebViewResponse(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            try
            {
                // Parse the uploaded data from action.Data
                var uploadedFiles = JArray.Parse(action.Data.ToString());

                // Dictionary for file type icons
                var fileTypeIcons = new Dictionary<string, string>
                {
                    { "spreadsheet", $"{this.baseUrl}/icons/ExcelIcon.png" },
                    { "pdf", $"{this.baseUrl}/icons/PdfIcon.png" },
                    { "wordprocessing", $"{this.baseUrl}/icons/WordIcons.png" },
                    { "image", $"{this.baseUrl}/icons/ImageIcon.png" },
                };

                var cardBody = new List<AdaptiveElement>();

                foreach (var file in uploadedFiles)
                {
                    string name = file["name"]?.ToString() ?? "Unknown";
                    string type = file["type"]?.ToString() ?? string.Empty;
                    string fileTypeIconUrl = fileTypeIcons.FirstOrDefault(kvp => type.Contains(kvp.Key)).Value
                                             ?? $"{this.baseUrl}/icons/DefaultIcon.png";

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
                var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4)) { Body = cardBody };

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
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in CreateWebViewResponse: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Handles the fetch task for messaging extensions.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the messaging extension response.</returns>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            try
            {
                return CreateMediaNameDetailsTaskResponse(turnContext, action);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in OnTeamsMessagingExtensionFetchTaskAsync: {ex.Message}");
                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Title = "Error",
                            Height = 200,
                            Width = 400,
                            Url = null
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Generates a task module response for the employee details form.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <returns>A MessagingExtensionActionResponse with a task module.</returns>
        private MessagingExtensionActionResponse CreateMediaNameDetailsTaskResponse(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            try
            {
                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Height = 530,
                            Width = 700,
                            Title = "Task Module WebView",
                            Url = $"{this.baseUrl}/CustomForm",
                        },
                    },
                };
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in CreateMediaNameDetailsTaskResponse: {ex.Message}");
                throw;
            }
        }
    }
}
