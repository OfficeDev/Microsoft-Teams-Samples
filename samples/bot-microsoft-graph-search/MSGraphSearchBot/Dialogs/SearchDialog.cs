using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using MSGraphSearchSample.Bots.Dialogs;
using MSGraphSearchSample.Helpers;
using MSGraphSearchSample.Interfaces;
using MSGraphSearchSample.Models;
using MSGraphSearchSample.Models.Search;
using MSGraphSearchSample.Services;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace MSGraphSearchSample.Dialogs
{
    public class SearchDialog : CancelAndHelpDialog
    {
        private const string PromptMsgText = "OK, what are you looking for?";
        private const string NotFountMsgText = "I'm sorry, I couldn't find any results.";
        private const string WhatElseText = "What else can I do for you?";
        private string queryString = string.Empty;
        private EntityType entityType = EntityType.ListItem;
        protected readonly AppConfigOptions appconfig;
        protected readonly IGraphService graphService;
        protected readonly IFileService fileService;
        protected readonly IAdaptiveCardService adaptiveCardService;
        protected readonly ILogger logger;

        public SearchDialog(IOptions<AppConfigOptions> options, IGraphService _graphService, ILogger<SearchDialog> _logger, IFileService _fileService, IAdaptiveCardService _adaptiveCardService)
            : base(nameof(SearchDialog))
        {
            appconfig = options.Value;
            graphService = _graphService;
            logger = _logger;
            fileService = _fileService;
            adaptiveCardService = _adaptiveCardService;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new TextPrompt("AdaptiveCardPrompt1"));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                searchAccountAsync,
                displayResults,
                MoreHelpAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            entityType = (EntityType) stepContext.Options;
            var promptMessage = MessageFactory.Text(PromptMsgText, PromptMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> searchAccountAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            queryString = stepContext.Result.ToString();
            var searchResult = await graphService.Search(entityType, queryString);
            return await stepContext.NextAsync(searchResult, cancellationToken);
        }

        private async Task<DialogTurnResult> displayResults(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var results = (SearchResults)stepContext.Result;
            var cardAttachment = new Attachment();

            if (results != null && results.Total > 0)
            {
                var cardElements = adaptiveCardService.GetSearchResultsContainers(results);
                cardAttachment = AttachmentHelper.BuildAttachment(cardElements);
            }
            else
            {
                var card = fileService.GetCard("NoResultsCard");
                cardAttachment = AttachmentHelper.GetAttachment(card);
            }
            var response = MessageFactory.Attachment(cardAttachment, ssml: "Account Details");
            await stepContext.Context.SendActivityAsync(response, cancellationToken);

            return await stepContext.NextAsync("done", cancellationToken);
        }

        
        private async Task<DialogTurnResult> MoreHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = MessageFactory.Text(WhatElseText, WhatElseText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var nextAction = stepContext.Result.ToString();
            return await stepContext.EndDialogAsync(nextAction, cancellationToken);
        }

    }
}
