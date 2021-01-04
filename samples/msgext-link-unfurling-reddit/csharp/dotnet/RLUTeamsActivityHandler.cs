// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// The RLUTeamsActivityHandler is responsible for reacting to incoming events from Teams sent from BotFramework.
    /// </summary>
    public sealed class RLUTeamsActivityHandler : TeamsActivityHandler
    {
        private readonly IOptions<RedditOptions> options;

        // The Typed HttpClient for Reddit.
        // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#how-to-use-typed-clients-with-httpclientfactory"
        private readonly RedditHttpClient redditHttpClient;

        private readonly ILogger<RLUTeamsActivityHandler> logger;

        private readonly IStringLocalizer<RLUTeamsActivityHandler> localizer;

        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RLUTeamsActivityHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="redditHttpClient">The Reddit HTTP client.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client. </param>
        /// <param name="options">The options.</param>
        public RLUTeamsActivityHandler(
            ILogger<RLUTeamsActivityHandler> logger,
            RedditHttpClient redditHttpClient,
            IStringLocalizer<RLUTeamsActivityHandler> localizer,
            TelemetryClient telemetryClient,
            IOptions<RedditOptions> options)
        {
            this.logger = logger;
            this.redditHttpClient = redditHttpClient;
            this.localizer = localizer;
            this.telemetryClient = telemetryClient;
            this.options = options;
        }

        /// <summary>
        /// Unfurl / Get a preview for the a link pasted inside the Teams' compose box.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The matched url.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task resolving to either a login card or the adaptive card of the Reddit post.</returns>
        /// <remarks>
        /// For more information on link unfurling in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/link-unfurling?tabs=dotnet .
        /// </remarks>
        protected override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(
             ITurnContext<IInvokeActivity> turnContext,
             AppBasedLinkQuery query,
             CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            query = query ?? throw new ArgumentNullException(nameof(query));

            this.RecordEvent(nameof(this.OnTeamsAppBasedLinkQueryAsync), turnContext);

            return this.GetRedditPostAsync(query.Url);
        }

        /// <summary>
        /// Handle when the user is searching in the messaging extension query.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The messaging extension query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that resolves to the list of cards that matched the query.</returns>
        /// <remarks>
        /// This application only supports direct links to reddit as the query string, in many applications
        /// this should be used to search for matching items.
        ///
        /// For more information on search based messaging extensions see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/search-commands/respond-to-search?tabs=dotnet .
        /// </remarks>
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            query = query ?? throw new ArgumentNullException(nameof(query));
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

            this.RecordEvent(nameof(this.OnTeamsMessagingExtensionQueryAsync), turnContext);

            var queryString = query.Parameters.FirstOrDefault()?.Value as string ?? string.Empty;
            return this.SearchRedditPostsAsync(queryString);
        }

        private void RecordEvent(string eventName, ITurnContext turnContext)
        {
            this.telemetryClient.TrackEvent(eventName, new Dictionary<string, string>
            {
                { "userId", turnContext.Activity.From.AadObjectId },
                { "tenantId", turnContext.Activity.Conversation.TenantId },
            });
        }

        private async Task<MessagingExtensionResponse> SearchRedditPostsAsync(string query)
        {
            try
            {
                // Execute the domain logic to get the reddit link
                IEnumerable<RedditLinkModel> redditLinks = await this.redditHttpClient.SearchLinksAsync(query);

                var attachments = redditLinks
                    .Select(redditLink =>
                    {
                        var preview = new MessagingExtensionAttachment(
                            contentType: HeroCard.ContentType,
                            contentUrl: null,
                            content: this.RenderLinkHeroCard(redditLink));
                        return new MessagingExtensionAttachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = this.RenderLinkAdaptiveCard(redditLink),
                            Preview = preview,
                        };
                    })
                    .ToList();

                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = AttachmentLayoutTypes.List,
                        Attachments = attachments,
                    },
                };
            }
#pragma warning disable CA1031 // This is a top-level handler and should avoid throwing exceptions.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                this.logger.LogError(ex, "Failed to get reddit post");
                return null;
            }
        }

        private async Task<MessagingExtensionResponse> GetRedditPostAsync(string postLink)
        {
            try
            {
                // Execute the domain logic to get the reddit link
                RedditLinkModel redditLink = await this.redditHttpClient.GetLinkAsync(postLink);

                var preview = new MessagingExtensionAttachment(
                    contentType: HeroCard.ContentType,
                    contentUrl: null,
                    content: this.RenderLinkHeroCard(redditLink));

                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = AttachmentLayoutTypes.List,
                        Attachments = new List<MessagingExtensionAttachment>()
                        {
                            new MessagingExtensionAttachment
                            {
                                ContentType = AdaptiveCard.ContentType,
                                Content = this.RenderLinkAdaptiveCard(redditLink),
                                Preview = preview,
                            },
                        },
                    },
                };
            }
#pragma warning disable CA1031 // This is a top-level handler and should avoid throwing exceptions.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                this.logger.LogError(ex, "Failed to get reddit post");
                return null;
            }
        }

        private HeroCard RenderLinkHeroCard(RedditLinkModel post)
        {
            return new HeroCard
            {
                Title = post.Title,
                Text = post.Subreddit,
                Images = new List<CardImage>
                {
                    new CardImage(post.Thumbnail),
                },
            };
        }

        private AdaptiveCard RenderLinkAdaptiveCard(RedditLinkModel post)
        {
            var titleBlock = new AdaptiveTextBlock
            {
                Text = $"[{post.Title}]({post.Link})",
                Size = AdaptiveTextSize.Large,
                Wrap = true,
                MaxLines = 2,
            };

            var upvoteColumn = new AdaptiveColumn
            {
                Width = AdaptiveColumnWidth.Auto,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.localizer.GetString("↑ {0}", post.Score),
                    },
                },
            };

            var commentColumn = new AdaptiveColumn
            {
                Width = AdaptiveColumnWidth.Auto,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.localizer.GetString(
                            "🗨️ [{0}](https://www.reddit.com/r/{1}/comments/{2})",
                            post.NumComments,
                            post.Subreddit,
                            post.Id),
                    },
                },
            };

            var subredditColumn = new AdaptiveColumn
            {
                Width = AdaptiveColumnWidth.Stretch,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = $"[/r/{post.Subreddit}](https://www.reddit.com/r/{post.Subreddit})",
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                        Size = AdaptiveTextSize.Default,
                        Weight = AdaptiveTextWeight.Bolder,
                    },
                },
            };

            var infoColumns = new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                {
                    upvoteColumn,
                    commentColumn,
                    subredditColumn,
                },
            };

            AdaptiveElement preview;
            if (post.Thumbnail != null)
            {
                preview = new AdaptiveImage
                {
                    Url = new Uri(post.Thumbnail),
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                    Separator = true,
                };
            }
            else
            {
                preview = new AdaptiveTextBlock
                {
                    Text = post.SelfText ?? this.localizer.GetString("Preview Not Available"),
                    Wrap = true,
                    Separator = true,
                };
            }

            var bottomLeftColumn = new AdaptiveColumn
            {
                Width = AdaptiveColumnWidth.Auto,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.localizer.GetString("Posted by [/u/{0}](https://www.reddit.com/u/{0})", post.Author),
                        Size = AdaptiveTextSize.Small,
                        Weight = AdaptiveTextWeight.Lighter,
                    },
                },
            };

            var createdText = $"{{{{DATE({post.Created.DateTime.ToString("yyyy-MM-ddThh:mm:ssZ", CultureInfo.InvariantCulture)})}}}}";
            var bottomRightColumn = new AdaptiveColumn
            {
                Width = AdaptiveColumnWidth.Stretch,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = createdText,
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                        Size = AdaptiveTextSize.Small,
                        Weight = AdaptiveTextWeight.Lighter,
                    },
                },
            };

            var bottomColumns = new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                {
                    bottomLeftColumn,
                    bottomRightColumn,
                },
            };

            var card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    titleBlock,
                    infoColumns,
                    preview,
                    bottomColumns,
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = this.localizer.GetString("Open in Reddit"),
                        Url = new Uri(post.Link),
                    },
                },
            };
            return card;
        }
    }
}