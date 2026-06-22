// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsMessagingExtensionsSearchBot : TeamsActivityHandler
    {
        private readonly string _baseUrl;
        private readonly IHttpClientFactory _httpClientFactory;

        public TeamsMessagingExtensionsSearchBot(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _baseUrl = configuration["BaseUrl"];
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var command = query.CommandId;
            var searchQuery = query?.Parameters?[0]?.Value as string ?? string.Empty;

            if (command == "wikipediaSearch")
            {
                string wikipediaUrl = $"https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&srsearch={searchQuery}&utf8=1";

                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    HttpResponseMessage response = await httpClient.GetAsync(wikipediaUrl, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    using JsonDocument document = JsonDocument.Parse(responseBody);
                    JsonElement queryElement = document.RootElement.GetProperty("query");
                    JsonElement searchElement = queryElement.GetProperty("search");

                    var attachments = new List<MessagingExtensionAttachment>();

                    foreach (JsonElement result in searchElement.EnumerateArray().Take(8))
                    {
                        string title = result.GetProperty("title").GetString();
                        string snippet = result.GetProperty("snippet").GetString();

                        var previewCard = new ThumbnailCard
                        {
                            Title = title,
                            Tap = new CardAction { Type = "openUrl", Value = $"https://en.wikipedia.org/wiki/{Uri.EscapeDataString(title)}" }
                        };

                        attachments.Add(new MessagingExtensionAttachment
                        {
                            ContentType = HeroCard.ContentType,
                            Content = new HeroCard { Title = title, Subtitle = snippet },
                            Preview = previewCard.ToAttachment()
                        });
                    }

                    return new MessagingExtensionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "result",
                            AttachmentLayout = "list",
                            Attachments = attachments
                        }
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching Wikipedia data: " + ex.Message);
                    return null;
                }
            }
            else
            {
                switch (searchQuery)
                {
                    case "adaptive card":
                        return GetAdaptiveCard();
                    case "connector card":
                        return GetConnectorCard();
                    case "result grid":
                        return GetResultGrid();
                }

                var packages = await FindPackages(searchQuery);

                // We take every row of the results and wrap them in cards wrapped in MessagingExtensionAttachment objects.
                // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.
                var attachments = packages.Select(package =>
                {
                    var previewCard = new ThumbnailCard { Title = package.Item1, Tap = new CardAction { Type = "invoke", Value = package } };
                    if (!string.IsNullOrEmpty(package.Item5))
                    {
                        previewCard.Images = new List<CardImage> { new CardImage(package.Item5, "Icon") };
                    }

                    return new MessagingExtensionAttachment
                    {
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard { Title = package.Item1 },
                        Preview = previewCard.ToAttachment()
                    };
                }).ToList();

                // The list of MessagingExtensionAttachments must be wrapped in a MessagingExtensionResult wrapped in a MessagingExtensionResponse.
                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = "list",
                        Attachments = attachments
                    }
                };
            }
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            // The Preview card's Tap should have a Value property assigned, this will be returned to the bot in this event.
            var (packageId, version, description, projectUrl, iconUrl) = query.ToObject<(string, string, string, string, string)>();

            var card = new ThumbnailCard
            {
                Title = $"{packageId}, {version}",
                Subtitle = description,
                Buttons = new List<CardAction>
                {
                    new CardAction { Type = ActionTypes.OpenUrl, Title = "Nuget Package", Value = $"https://www.nuget.org/packages/{packageId}" },
                    new CardAction { Type = ActionTypes.OpenUrl, Title = "Project", Value = projectUrl },
                },
            };

            if (!string.IsNullOrEmpty(iconUrl))
            {
                card.Images = new List<CardImage> { new CardImage(iconUrl, "Icon") };
            }

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment>
                    {
                        new MessagingExtensionAttachment
                        {
                            ContentType = ThumbnailCard.ContentType,
                            Content = card,
                        }
                    }
                }
            });
        }

        private async Task<IEnumerable<(string, string, string, string, string)>> FindPackages(string text)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var obj = JObject.Parse(await httpClient.GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{text}&prerelease=true"));
            return obj["data"].Select(item => (item["id"].ToString(), item["version"].ToString(), item["description"].ToString(), item["projectUrl"]?.ToString(), item["iconUrl"]?.ToString()));
        }

        private MessagingExtensionResponse GetAdaptiveCard()
        {
            string filepath = Path.Combine(".", "Resources", "RestaurantCard.json");
            var previewcard = new ThumbnailCard
            {
                Title = "Adaptive Card",
                Text = "Please select to get Adaptive card"
            };
            var adaptiveList = FetchAdaptive(filepath);
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment>
                    {
                        new MessagingExtensionAttachment
                        {
                            ContentType = "application/vnd.microsoft.card.adaptive",
                            Content = adaptiveList.Content,
                            Preview = previewcard.ToAttachment()
                        }
                    }
                }
            };
        }

        private MessagingExtensionResponse GetConnectorCard()
        {
            string filepath = Path.Combine(".", "Resources", "connectorCard.json");
            var previewcard = new ThumbnailCard
            {
                Title = "O365 Connector Card",
                Text = "Please select to get Connector card"
            };
            var connector = FetchConnector(filepath);
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment>
                    {
                        new MessagingExtensionAttachment
                        {
                            ContentType = O365ConnectorCard.ContentType,
                            Content = connector.Content,
                            Preview = previewcard.ToAttachment()
                        }
                    }
                }
            };
        }

        private Attachment FetchAdaptive(string filepath)
        {
            var adaptiveCardJson = File.ReadAllText(filepath);
            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson)
            };
        }

        private Attachment FetchConnector(string filepath)
        {
            var connectorCardJson = File.ReadAllText(filepath);
            return new MessagingExtensionAttachment
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = JsonConvert.DeserializeObject(connectorCardJson),
            };
        }

        private MessagingExtensionResponse GetResultGrid()
        {
            var imageFiles = Directory.EnumerateFiles("wwwroot", "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".jpg"));

            var attachments = new List<MessagingExtensionAttachment>();

            foreach (string img in imageFiles)
            {
                var image = img.Split(Path.DirectorySeparatorChar);
                var thumbnailCard = new ThumbnailCard
                {
                    Images = new List<CardImage> { new CardImage(_baseUrl + "/" + image[1]) }
                };
                attachments.Add(new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = thumbnailCard,
                });
            }

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "grid",
                    Attachments = attachments
                }
            };
        }
    }
}
