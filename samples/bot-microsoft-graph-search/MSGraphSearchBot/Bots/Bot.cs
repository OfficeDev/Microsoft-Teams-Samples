using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSGraphSearchSample.Constants;
using MSGraphSearchSample.Constants.AdaptiveCards;
using MSGraphSearchSample.Helpers;
using MSGraphSearchSample.Interfaces;
using MSGraphSearchSample.Models;
using MSGraphSearchSample.Models.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Bots
{
    public partial class Bot<T> : TeamsActivityHandler
        where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger _logger;
        protected readonly AppConfigOptions _appConfig;
        protected readonly IAdaptiveCardService _adaptiveCardService;
        protected readonly IFileService _fileService;
        protected readonly IGraphService _graphService;
        private readonly IStatePropertyAccessor<string> UserConfigProperty;

        public Bot(ConversationState conversationState, UserState userState, T dialog, ILogger<Bot<T>> logger,
            IAdaptiveCardService adaptiveCardService, IFileService fileService, IOptions<AppConfigOptions> options,
            IGraphService graphService)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            UserConfigProperty = userState.CreateProperty<string>("UserConfiguration");
            _logger = logger;
            _fileService = fileService;
            _adaptiveCardService = adaptiveCardService;
            _appConfig = options.Value;
            _graphService = graphService;
        }
        // Handle members joining a conversation, or bot added as personal app
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.                
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var responseText = $"Hi, thank you for adding me! Here's a snippet of what you can now do:";
                    var responseMessage = MessageFactory.Text(responseText, responseText, InputHints.IgnoringInput);
                    await turnContext.SendActivityAsync(responseMessage, cancellationToken);

                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }
        // This method calls one of the other handlers, based on the type of activity received.
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        // Handle message activities
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
        // Verifying SSO SignIn
        protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        protected override async Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            if (invokeValue.Action.Verb == Actions.Next || invokeValue.Action.Verb == Actions.Previous)
            {
                JObject val = JObject.FromObject(invokeValue.Action.Data);
                PagingData data = val.ToObject<CardTaskFetchValue<PagingData>>()?.Data;
                var results = await _graphService.Search(data.EntityType,data.QueryString, data.From);
                var cardAttachment = new Attachment();

                if (results != null && results.Total > 0)
                {
                    results.CurrentPage = data.PageNumber;
                    results.Action = invokeValue.Action.Verb;
                    var cardElements = _adaptiveCardService.GetSearchResultsContainers(results);
                    cardAttachment = AttachmentHelper.BuildAttachment(cardElements);
                }
                else
                {
                    var card = _fileService.GetCard("NoResultsCard");
                    cardAttachment = AttachmentHelper.GetAttachment(card);
                }

                Activity updateActivity = new Activity();
                updateActivity.Type = "message";
                updateActivity.Id = turnContext.Activity.ReplyToId;
                updateActivity.Attachments = new List<Attachment> { cardAttachment };
                await turnContext.UpdateActivityAsync(updateActivity);
            }
            var response = new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = null,
                Value = null
            };
            return response;
        }

    }
}
