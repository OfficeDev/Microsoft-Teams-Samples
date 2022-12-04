// <copyright file="RLUTeamsActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using System;
    using System.Collections.Generic;
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
    using Newtonsoft.Json.Linq;

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
        /// Get the login or preview response for the given link.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The matched url.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task resolving to either a login card or </returns>
        /// <remarks>
        /// For more information on Link Unfurling see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/link-unfurling?tabs=dotnet
        ///
        /// This method also implements messaging extension authentication to get the reddit API token for the user.
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/add-authentication
        /// </remarks>
        protected override async Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(
             ITurnContext<IInvokeActivity> turnContext,
             AppBasedLinkQuery query,
             CancellationToken cancellationToken = default)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            query = query ?? throw new ArgumentNullException(nameof(query));

            IUserTokenProvider tokenProvider = turnContext.Adapter as IUserTokenProvider;

            // Get the magic code out of the request for when the login flow is completed.
            // https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-authentication?view=azure-bot-service-4.0#securing-the-sign-in-url
            string magicCode = (turnContext.Activity?.Value as JObject)?.Value<string>("state");

            // Get the token from the Azure Bot Framework Token Service to handle token storage
            // https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-authentication?view=azure-bot-service-4.0#about-the-bot-framework-token-service
            var tokenResponse = await tokenProvider
                ?.GetUserTokenAsync(
                    turnContext: turnContext,
                    connectionName: this.options.Value.BotFrameworkConnectionName,
                    magicCode: magicCode,
                    cancellationToken: cancellationToken);

            try
            {
                // Execute the domain logic to get the reddit link
                RedditLinkModel redditLink = await this.redditHttpClient.GetLinkAsync(tokenResponse?.Token, query.Url);

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
            catch (RedditUnauthorizedException)
            {
                this.logger.LogInformation("Attempt to fetch post resulted in unauthorized, triggering log-in flow");

                // "log out" the user, so log-in gets a new token.
                await tokenProvider.SignOutUserAsync(
                    turnContext: turnContext,
                    connectionName: this.options.Value.BotFrameworkConnectionName,
                    cancellationToken: cancellationToken);

                return await this
                    .GetAuthenticationMessagingExtensionResponseAsync(turnContext, cancellationToken)
                    .ConfigureAwait(false);
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                this.logger.LogError(ex, "Failed to get reddit post");
                return null;
            }
        }

        /// <summary>
        /// Open the Messaging Extension Configuration Page
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The matched url.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task resolving to the settings / logout page </returns>
        /// <remarks>
        /// For more information on Link Unfurling see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/link-unfurling?tabs=dotnet
        /// </remarks>
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            var messagingExtensionReponse = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "config",
                    SuggestedActions = new MessagingExtensionSuggestedAction
                    {
                        Actions = new List<CardAction>
                        {
                            new CardAction
                            {
                                Type = ActionTypes.OpenUrl,
                                Value = this.options.Value.SettingsPageUrl,
                            },
                        },
                    },
                },
            };
            return Task.FromResult(messagingExtensionReponse);
        }

        /// <summary>
        /// Summary
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task resolving to either a login card or </returns>
        /// <remarks>
        /// For more information see the sample on GitHub
        /// https://github.com/microsoft/BotBuilder-Samples/blob/master/samples/csharp_dotnetcore/52.teams-messaging-extensions-search-auth-config/Bots/TeamsMessagingExtensionsSearchAuthConfigBot.cs#L43
        /// </remarks>
        protected override async Task OnTeamsMessagingExtensionConfigurationSettingAsync(
            ITurnContext<IInvokeActivity> turnContext,
            JObject settings,
            CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            settings = settings ?? new JObject();

            var state = settings["state"]?.ToString()?.ToUpperInvariant();

            switch (state)
            {
                case "SIGNOUT":
                    this.logger.LogInformation("User initiated sign-out");

                    var tokenProvider = turnContext.Adapter as IUserTokenProvider;

                    await tokenProvider.SignOutUserAsync(
                        turnContext: turnContext,
                        connectionName: this.options.Value.BotFrameworkConnectionName,
                        cancellationToken: cancellationToken);
                    return;
                default:
                    this.logger.LogWarning("Unknown setting state: {0}", state);
                    return;
            }
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
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            query = query ?? throw new ArgumentNullException(nameof(query));

            IUserTokenProvider tokenProvider = turnContext.Adapter as IUserTokenProvider;
            string magicCode = (turnContext.Activity?.Value as JObject)?.Value<string>("state");

            var tokenResponse = await tokenProvider
                ?.GetUserTokenAsync(
                    turnContext: turnContext,
                    connectionName: this.options.Value.BotFrameworkConnectionName,
                    magicCode: magicCode,
                    cancellationToken: cancellationToken);
            var queryString = query.Parameters.FirstOrDefault()?.Value as string ?? string.Empty;
            try
            {
                 // Execute the domain logic to get the reddit link
                IEnumerable<RedditLinkModel> redditLinks = await this.redditHttpClient.SearchLinksAsync(tokenResponse?.Token, queryString);

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
            catch (RedditUnauthorizedException)
            {
                this.logger.LogInformation("Attempt to fetch post resulted in unauthorized, triggering log-in flow");

                // "log out" the user, so log-in gets a new token.
                await tokenProvider.SignOutUserAsync(
                    turnContext: turnContext,
                    connectionName: this.options.Value.BotFrameworkConnectionName,
                    cancellationToken: cancellationToken);

                return await this
                    .GetAuthenticationMessagingExtensionResponseAsync(turnContext, cancellationToken)
                    .ConfigureAwait(false);
            }
#pragma warning disable CA1031
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
                Title = post?.Title,
                Text = post?.Link,
                Images = new List<CardImage>
                {
                    new CardImage(post?.Thumbnail),
                },
            };
        }

        private AdaptiveCard RenderLinkAdaptiveCard(RedditLinkModel post)
        {
            var titleBlock = new AdaptiveTextBlock
            {
                Text = post.Title,
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
                        Text = $"↑ {post.Score}",
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
                        Text = $"🗨️ {post.NumComments}",
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
                        Text = $"/r/{post.Subreddit}",
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
                    Text = post.SelfText ?? this.localizer.GetString("Preview not available"),
                    Wrap = true,
                    Separator = true,
                };
            }

            return new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    titleBlock,
                    infoColumns,
                    preview,
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
        }

        private async Task<MessagingExtensionResponse> GetAuthenticationMessagingExtensionResponseAsync(
            ITurnContext<IInvokeActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // Before requesting the token link, make sure the request is still live.
            cancellationToken.ThrowIfCancellationRequested();

            IUserTokenProvider tokenProvider = turnContext.Adapter as IUserTokenProvider;
            string signInLink = await tokenProvider
                ?.GetOauthSignInLinkAsync(
                    turnContext: turnContext,
                    connectionName: this.options.Value.BotFrameworkConnectionName,
                    cancellationToken: cancellationToken);

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "auth",
                    SuggestedActions = new MessagingExtensionSuggestedAction
                    {
                        Actions = new List<CardAction>
                        {
                            new CardAction
                            {
                                Type = ActionTypes.OpenUrl,
                                Value = signInLink,
                                Title = this.localizer.GetString("Sign in"),
                            },
                        },
                    },
                },
            };
        }
    }
}