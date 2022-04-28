using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Graph;
using MSGraphSearchSample.Constants;
using MSGraphSearchSample.Constants.MessagingExtension;
using MSGraphSearchSample.Helpers;
using MSGraphSearchSample.Models;
using MSGraphSearchSample.Models.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace MSGraphSearchSample.Bots
{
    public partial class Bot<T> : TeamsActivityHandler
        where T : Dialog
    {
        // Handle queries
        // More info: https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/search-commands/define-search-command
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            // get query object
            query = query ?? throw new ArgumentNullException(nameof(query));
            // query string value
            // Multi-parameter is only supported for command type set to action. For query we only support 1 parameter currently.
            var queryText = query.Parameters.FirstOrDefault()?.Value as string ?? string.Empty;
            var parameterName = query.Parameters.FirstOrDefault()?.Name;

            // Setup Single Sign-On (obtaining access token)
            var userConfigSettings = await UserConfigProperty.GetAsync(turnContext, () => string.Empty);
            var tokenResponse = await GetTokenResponse(query.State, turnContext, cancellationToken);
            if (tokenResponse != null)
                return tokenResponse;

            // Return some items on initial run (if it's enabled in manifest)
            if (parameterName == "initialRun")
                return await GetPreviewItems();

            var entityType = GetEntityType(query.CommandId);            
            if (entityType == Microsoft.Graph.EntityType.UnknownFutureValue)
                return new MessagingExtensionResponse();

            var results = await _graphService.Search(entityType, queryText, 0);

            if (results != null)
            {
                var data = Mappers.DataMapper.GetMappedData(results.Hits, entityType);
                var attachments = new List<MessagingExtensionAttachment>();
                foreach (var item in data)
                {
                    var title = GetThumbnailCardTitle(item);
                    var text = GetThumbnailCardText(item);
                    var container = GetContainer(item) as List<AdaptiveElement>;
                    var itemAttachment = AttachmentHelper.BuildAttachment(container);

                    var attachment = new MessagingExtensionAttachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = itemAttachment.Content,
                        Preview = new ThumbnailCard { Title = title, Text = text }.ToAttachment()
                    };
                    attachments.Add(attachment);
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
            else
                return new MessagingExtensionResponse();


        }

        // The Preview card's Tap should have a Value property assigned, this will be returned to the bot in this event
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            // IMPORTANT: is not triggered in mobile teams application

            // We take every row of the results and wrap them in cards wrapped in MessagingExtensionAttachment objects.
            // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.

            return null;
        }

        protected async Task<MessagingExtensionResponse> GetPreviewItems()
        {
            var attachments = new List<MessagingExtensionAttachment>();
            // Get the attachment items from your Graph or any other services and add them to the attachments collection

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

        protected EntityType GetEntityType(string commandId)
        {
            switch (commandId)
            {
                case CommandIds.SearchEvents:
                    return EntityType.Event;
                case CommandIds.SearchFiles:
                    return EntityType.DriveItem;
                case CommandIds.SearchListItems:
                    return EntityType.ListItem;
                case CommandIds.SearchMessages:
                    return EntityType.Message;
                default:
                    return EntityType.UnknownFutureValue;
            }
        }

        protected List<AdaptiveElement> GetContainer(dynamic item)
        {
            var container = new List<AdaptiveElement>();
            if (item is Models.Search.Event)
                container = _adaptiveCardService.GetElements(new List<Models.Search.Event>() { item });
            if (item is Models.Search.ListItem)
                container = _adaptiveCardService.GetElements(new List<Models.Search.ListItem>() { item });
            if (item is Models.Search.Message)
                container = _adaptiveCardService.GetElements(new List<Models.Search.Message>() { item });
            if (item is Models.Search.DriveItem)
                container = _adaptiveCardService.GetElements(new List<Models.Search.DriveItem>() { item });

            return container;
        }
        protected string GetThumbnailCardTitle(dynamic item)
        {
            if (item is Models.Search.Event)
                return (item as Models.Search.Event).Subject;
            if (item is Models.Search.ListItem)
                return (item as Models.Search.ListItem).Title;
            if (item is Models.Search.Message)
                return (item as Models.Search.Message).Subject;
            if (item is Models.Search.DriveItem)
                return (item as Models.Search.DriveItem).Name;
            return "Unknown";
        }

        protected string GetThumbnailCardText(dynamic item)
        {
            if (item is Models.Search.Event)
                return (item as Models.Search.Event).Start;
            if (item is Models.Search.ListItem)
                return (item as Models.Search.ListItem).Created;
            if (item is Models.Search.Message)
                return (item as Models.Search.Message).CreatedDatedTime;
            if (item is Models.Search.DriveItem)
                return (item as Models.Search.DriveItem).CreatedDateTime;
            return "Unknown";
        }        
    }
}
