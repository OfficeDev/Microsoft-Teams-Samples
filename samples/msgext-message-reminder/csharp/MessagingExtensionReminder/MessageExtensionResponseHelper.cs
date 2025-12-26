// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

using Microsoft.Teams.Api.Cards;
using Microsoft.Teams.Cards;

namespace MessagingExtensionReminder
{
    public static class MessageExtensionResponseHelper
    {
        // Helper methods for creating responses
        public static Microsoft.Teams.Api.MessageExtensions.Response CreateSearchResults(string query, Microsoft.Teams.Common.Logging.ILogger log)
        {
            var attachments = new List<Microsoft.Teams.Api.MessageExtensions.Attachment>();

            // Create simple search results
            for (int i = 1; i <= 5; i++)
            {
                var card = new AdaptiveCard(new List<CardElement>
                {
                    new TextBlock($"Search Result {i}")
                    {
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Large
                    },
                    new TextBlock($"Query: '{query}' - Result description for item {i}")
                    {
                        Wrap = true,
                        IsSubtle = true
                    }
                });

                var previewCard = new ThumbnailCard()
                {
                    Title = $"Result {i}",
                    Text = $"This is a preview of result {i} for query '{query}'."
                };

                var attachment = new Microsoft.Teams.Api.MessageExtensions.Attachment
                {
                    ContentType = Microsoft.Teams.Api.ContentType.AdaptiveCard,
                    Content = card,
                    Preview = new Microsoft.Teams.Api.MessageExtensions.Attachment
                    {
                        ContentType = Microsoft.Teams.Api.ContentType.ThumbnailCard,
                        Content = previewCard
                    }
                };

                attachments.Add(attachment);
            }

            return new Microsoft.Teams.Api.MessageExtensions.Response
            {
                ComposeExtension = new Microsoft.Teams.Api.MessageExtensions.Result
                {
                    Type = Microsoft.Teams.Api.MessageExtensions.ResultType.Result,
                    AttachmentLayout = Microsoft.Teams.Api.Attachment.Layout.List,
                    Attachments = attachments
                }
            };
        }

        public static Microsoft.Teams.Api.MessageExtensions.Response HandleCreateCard(JsonElement? data, Microsoft.Teams.Common.Logging.ILogger log)
        {
            var title = GetJsonValue(data, "title") ?? "Default Title";
            var description = GetJsonValue(data, "description") ?? "Default Description";

            log.Info($"[CREATE_CARD] Title: {title}, Description: {description}");

            var card = new Microsoft.Teams.Cards.AdaptiveCard(new List<CardElement>
            {
                new TextBlock("Custom Card Created")
                {
                    Weight = TextWeight.Bolder,
                    Size = TextSize.Large,
                    Color = TextColor.Good
                },
                new TextBlock(title)
                {
                    Weight = TextWeight.Bolder,
                    Size = TextSize.Medium
                },
                new TextBlock(description)
                {
                    Wrap = true,
                    IsSubtle = true
                }
            });

            var attachment = new Microsoft.Teams.Api.MessageExtensions.Attachment
            {
                ContentType = Microsoft.Teams.Api.ContentType.AdaptiveCard,
                Content = card,
            };

            return new Microsoft.Teams.Api.MessageExtensions.Response
            {
                ComposeExtension = new Microsoft.Teams.Api.MessageExtensions.Result
                {
                    Type = Microsoft.Teams.Api.MessageExtensions.ResultType.Result,
                    AttachmentLayout = Microsoft.Teams.Api.Attachment.Layout.List,
                    Attachments = new List<Microsoft.Teams.Api.MessageExtensions.Attachment> { attachment }
                }
            };
        }

        public static Microsoft.Teams.Api.MessageExtensions.Response HandleGetMessageDetails(Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.SubmitActionActivity activity, Microsoft.Teams.Common.Logging.ILogger log)
        {
            var messageText = activity.Value?.MessagePayload?.Body?.Content ?? "No message content";
            var messageId = activity.Value?.MessagePayload?.Id ?? "Unknown";

            log.Info($"[GET_MESSAGE_DETAILS] Message ID: {messageId}");

            var card = new Microsoft.Teams.Cards.AdaptiveCard(new List<CardElement>
            {
                new TextBlock("Message Details")
                {
                    Weight = TextWeight.Bolder,
                    Size = TextSize.Large,
                    Color = TextColor.Accent
                },
                new TextBlock($"Message ID: {messageId}")
                {
                    Wrap = true
                },
                new TextBlock($"Content: {messageText}")
                {
                    Wrap = true
                }
            });

            var attachment = new Microsoft.Teams.Api.MessageExtensions.Attachment
            {
                ContentType = new Microsoft.Teams.Api.ContentType("application/vnd.microsoft.card.adaptive"),
                Content = card
            };

            return new Microsoft.Teams.Api.MessageExtensions.Response
            {
                ComposeExtension = new Microsoft.Teams.Api.MessageExtensions.Result
                {
                    Type = Microsoft.Teams.Api.MessageExtensions.ResultType.Result,
                    AttachmentLayout = Microsoft.Teams.Api.Attachment.Layout.List,
                    Attachments = new List<Microsoft.Teams.Api.MessageExtensions.Attachment> { attachment }
                }
            };
        }

        public static Microsoft.Teams.Api.MessageExtensions.Response CreateLinkUnfurlResponse(string url, Microsoft.Teams.Common.Logging.ILogger log)
        {
            var card = new Microsoft.Teams.Cards.AdaptiveCard(new List<CardElement>
            {
                new TextBlock("Link Preview")
                {
                    Weight = TextWeight.Bolder,
                    Size = TextSize.Medium
                },
                new TextBlock($"URL: {url}")
                {
                    IsSubtle = true,
                    Wrap = true
                },
                new TextBlock("This is a preview of the linked content generated by the message extension.")
                {
                    Wrap = true,
                    Size = TextSize.Small
                }
            });

            var attachment = new Microsoft.Teams.Api.MessageExtensions.Attachment
            {
                ContentType = new Microsoft.Teams.Api.ContentType("application/vnd.microsoft.card.adaptive"),
                Content = card,
                Preview = new Microsoft.Teams.Api.MessageExtensions.Attachment
                {
                    ContentType = Microsoft.Teams.Api.ContentType.ThumbnailCard,
                    Content = new ThumbnailCard
                    {
                        Title = "Link Preview",
                        Text = url
                    }
                }
            };

            return new Microsoft.Teams.Api.MessageExtensions.Response
            {
                ComposeExtension = new Microsoft.Teams.Api.MessageExtensions.Result
                {
                    Type = Microsoft.Teams.Api.MessageExtensions.ResultType.Result,
                    AttachmentLayout = Microsoft.Teams.Api.Attachment.Layout.List,
                    Attachments = new List<Microsoft.Teams.Api.MessageExtensions.Attachment> { attachment }
                }
            };
        }

        public static Microsoft.Teams.Api.MessageExtensions.Response CreateItemSelectionResponse(object? selectedItem, Microsoft.Teams.Common.Logging.ILogger log)
        {
            var itemJson = JsonSerializer.Serialize(selectedItem);

            var card = new Microsoft.Teams.Cards.AdaptiveCard(
                new List<CardElement>
                {
                    new TextBlock("Item Selected")
                    {
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Large,
                        Color = TextColor.Good
                    },
                    new TextBlock("You selected the following item:")
                    {
                        Wrap = true
                    },
                    new TextBlock(itemJson)
                    {
                        Wrap = true,
                        FontType = FontType.Monospace,
                        Separator = true
                    }
                }
            )
            {
                Schema = "http://adaptivecards.io/schemas/adaptive-card.json"
            };

            var attachment = new Microsoft.Teams.Api.MessageExtensions.Attachment
            {
                ContentType = new Microsoft.Teams.Api.ContentType("application/vnd.microsoft.card.adaptive"),
                Content = card
            };

            return new Microsoft.Teams.Api.MessageExtensions.Response
            {
                ComposeExtension = new Microsoft.Teams.Api.MessageExtensions.Result
                {
                    Type = Microsoft.Teams.Api.MessageExtensions.ResultType.Result,
                    AttachmentLayout = Microsoft.Teams.Api.Attachment.Layout.List,
                    Attachments = new List<Microsoft.Teams.Api.MessageExtensions.Attachment> { attachment }
                }
            };
        }

        public static Microsoft.Teams.Api.MessageExtensions.Response CreateErrorResponse(string message)
        {
            return new Microsoft.Teams.Api.MessageExtensions.Response
            {
                ComposeExtension = new Microsoft.Teams.Api.MessageExtensions.Result
                {
                    Type = Microsoft.Teams.Api.MessageExtensions.ResultType.Message,
                    Text = message
                }
            };
        }

        public static Microsoft.Teams.Api.MessageExtensions.Response CreateErrorActionResponse(string message)
        {
            return new Microsoft.Teams.Api.MessageExtensions.Response
            {
                ComposeExtension = new Microsoft.Teams.Api.MessageExtensions.Result
                {
                    Type = Microsoft.Teams.Api.MessageExtensions.ResultType.Message,
                    Text = message
                }
            };
        }

        private static string? GetJsonValue(JsonElement? data, string key)
        {
            if (data?.ValueKind == JsonValueKind.Object && data.Value.TryGetProperty(key, out var value))
            {
                return value.GetString();
            }
            return null;
        }
    }
}
