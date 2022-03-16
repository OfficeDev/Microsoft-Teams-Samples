// <copyright file="BotActivityHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Web.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;
    using Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.ResourceServices;
    using Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Teams Bot Activity Handler.
    /// </summary>
    public class BotActivityHandler : TeamsActivityHandler
    {
        private readonly IResourceProvider resourceProvider;
        private readonly ICardFactory cardFactory;
        private readonly IUrlParser urlParser;
        private readonly IAppSettings appSettings;
        private readonly ILogger<BotActivityHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotActivityHandler"/> class.
        /// </summary>
        /// <param name="resourceProvider">Resource provider.</param>
        /// <param name="cardFactory">Card factory.</param>
        /// <param name="urlParser">Url parser.</param>
        /// <param name="appSettings">App settings.</param>
        /// <param name="logger">Logger.</param>
        public BotActivityHandler(
            IResourceProvider resourceProvider,
            ICardFactory cardFactory,
            IUrlParser urlParser,
            IAppSettings appSettings,
            ILogger<BotActivityHandler> logger)
        {
            this.resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
            this.cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
            this.urlParser = urlParser ?? throw new ArgumentNullException(nameof(urlParser));
            this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// This method is called when user enters a url in compose box from a domain that this app unfurls.
        ///
        /// Application authenticates the user to read resource and insert a card with resource information and option to review it in a meeting.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="query">Query - this contains the url that user entered.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ME response.</returns>
        protected async override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"OnTeamsAppBasedLinkQueryAsync:  {query.Url}");

            // Make sure its a valid url.
            if (!this.urlParser.IsValidResourceUrl(query.Url))
            {
                return null;
            }

            // Make sure the user is signed-in.
            var isUserSignedIn = await turnContext.IsUserSignedInAsync(this.appSettings.GraphConnectionName, query.State, cancellationToken);
            if (!isUserSignedIn)
            {
                this.logger.LogInformation("User is not signed-in.");
                var signInUrl = await turnContext.GetOAuthSignInUrlAsync(this.appSettings.GraphConnectionName, cancellationToken);
                var signInResponse = this.GetSignInResponse(signInUrl);
                return signInResponse;
            }

            // If the user is signed-in:
            // 1. Check if user is authorized to access the resource. (Skipped in this sample).
            // 2. Create a card with resource information/preview image and option to review it in a meeting.
            var resourceId = this.urlParser.GetResourceId(query.Url);
            var resource = await this.resourceProvider.GetResourceAsync(resourceId);
            return this.GetContentCardResponse(resource);
        }

        /// <inheritdoc/>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            // Verify action.
            if (!taskModuleRequest.IsReviewInMeetingAction())
            {
                // App doesn't recognize the action.
                throw new Exception("App doesn't recognize the incoming action.");
            }

            // Read resource information from task module request and return link to review it in a meeting.
            var resource = taskModuleRequest.ReadResource();
            var response = new TaskModuleResponse(this.GetTaskModuleContinueResponse(
                title: resource.Name,
                url: $"{this.appSettings.BaseUrl}/reviewinmeeting/{resource.Id}",
                fallbackUrl: resource.Url));
            return Task.FromResult(response);
        }

        /// <inheritdoc/>
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(
           ITurnContext<IInvokeActivity> turnContext,
           MessagingExtensionQuery query,
           CancellationToken cancellationToken)
        {
            var openSettingAction = new CardAction
            {
                Type = ActionTypes.OpenUrl,
                Value = $"{this.appSettings.BaseUrl}/mesettings",
            };

            // ME Result.
            var result = new MessagingExtensionResult
            {
                Type = "config",
                SuggestedActions = new MessagingExtensionSuggestedAction
                {
                    Actions = new List<CardAction> { openSettingAction },
                },
            };

            // ME response.
            var response = new MessagingExtensionResponse()
            {
                ComposeExtension = result,
            };

            return Task.FromResult(response);
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsMessagingExtensionConfigurationSettingAsync(
            ITurnContext<IInvokeActivity> turnContext,
            JObject settings,
            CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            settings ??= new JObject();

            var state = settings["state"]?.ToString()?.ToUpperInvariant();
            switch (state)
            {
                case "SIGNOUT":
                    this.logger.LogInformation("User initiated sign-out");
                    await turnContext.SignOutUserAsync(this.appSettings.GraphConnectionName, cancellationToken);
                    return;
                default:
                    this.logger.LogWarning("Unknown setting state: {0}", state);
                    return;
            }
        }

        /// <summary>
        /// Handle when the user is searching in the messaging extension query.
        /// Apps should handle user queries and return appropriate results.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The messaging extension query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that resolves to the list of cards that matched the query.</returns>
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            var attachment = new HeroCard("Query not implemented", "App should handle query approperiately.");
            var attachments = new MessagingExtensionAttachment(HeroCard.ContentType, null/*url*/, attachment);
            var response = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult(
                    AttachmentLayoutTypes.List,
                    "result" /*type*/,
                    new[] { attachments }),
            };

            return Task.FromResult(response);
        }

        private MessagingExtensionResponse GetContentCardResponse(Resource resource)
        {
            // Prepare attachments
            var expandedCard = this.cardFactory.GetResourceContentCard(resource);
            var collapsedCard = this.cardFactory.GetResourcePreviewCard(resource);
            var attachments = new MessagingExtensionAttachment(expandedCard.ContentType, null/*contentUrl*/, expandedCard.Content)
            {
                Preview = collapsedCard,
            };

            // ME result.
            var result = new MessagingExtensionResult(
                AttachmentLayoutTypes.List,
                "result" /*type*/,
                new[] { attachments });
            return new MessagingExtensionResponse(result);
        }

        /// <summary>
        /// Prepares a message extension response with sign-in action as compose extension.
        /// </summary>
        /// <param name="signInUrl">Sign-in url.</param>
        /// <returns>Messaging extension response.</returns>
        private MessagingExtensionResponse GetSignInResponse(string signInUrl)
        {
            // Sign-in action.
            var signInAction = new CardAction
            {
                Type = ActionTypes.OpenUrl,
                Value = signInUrl,
                Title = "Sign in",
            };

            // ME Result.
            var result = new MessagingExtensionResult
            {
                Type = "auth",
                SuggestedActions = new MessagingExtensionSuggestedAction
                {
                    Actions = new List<CardAction> { signInAction },
                },
            };

            // ME response.
            var response = new MessagingExtensionResponse()
            {
                ComposeExtension = result,
            };

            return response;
        }

        private TaskModuleContinueResponse GetTaskModuleContinueResponse(string title, string url, string fallbackUrl)
        {
            return new TaskModuleContinueResponse()
            {
                Value = new TaskModuleTaskInfo()
                {
                    Title = title,
                    Url = url,
                    FallbackUrl = url,
                    Height = "small",
                    Width = "small",
                },
            };
        }
    }
}
