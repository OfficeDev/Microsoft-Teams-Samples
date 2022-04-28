
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using MSGraphSearchSample.Constants.Bot;
using MSGraphSearchSample.Constants.Search;
using MSGraphSearchSample.Dialogs;
using MSGraphSearchSample.Helpers;
using MSGraphSearchSample.Interfaces;
using MSGraphSearchSample.Models;
using MSGraphSearchSample.Models.Bot;
using System.Threading;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Bots.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        protected readonly AppConfigOptions _appConfig;
        protected readonly IFileService _fileService;
        protected readonly IGraphService _graphService;
        protected readonly IAdaptiveCardService _adaptiveCardService;
        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(SearchDialog searchDialog,ILogger<MainDialog> logger, IOptions<AppConfigOptions> options, IFileService fileService, IGraphService graphService, IAdaptiveCardService adaptiveCardService)
            : base(nameof(MainDialog))
        {
            Logger = logger;
            _appConfig = options.Value;
            _fileService = fileService;
            _graphService = graphService;
            _adaptiveCardService = adaptiveCardService;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt("AdaptiveCardPrompt"));
            AddDialog(searchDialog);
            AddDialog(new TokenExchangeOAuthPrompt(
                nameof(TokenExchangeOAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = _appConfig.ConnectionName,
                    Text = "We need to ask for additional permissions. You should only need to do this once.",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync,
                ActStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // SSO sign in prompt (if not already signed in)
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
            return await stepContext.BeginDialogAsync(nameof(TokenExchangeOAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse?.Token != null)
            {
                // Pull in the data from the Microsoft Graph.
                _graphService.SetAccessToken(tokenResponse.Token);
                var nextActionText = !string.IsNullOrEmpty(stepContext.Context.Activity.Text) ? stepContext.Context.Activity.Text : "Welcome";
                return await stepContext.NextAsync(nextActionText, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var action = stepContext.Result != null ? stepContext.Result.ToString().Trim().ToLower() : stepContext.Context.Activity.Text.Trim().ToLower();
            var entityType = getEntityType(action);
            switch (entityType)
            {
                // Start a new instance of your dialog
                case EntityType.Event:
                case EntityType.DriveItem:
                case EntityType.ListItem:
                case EntityType.Message:
                    return await stepContext.BeginDialogAsync(nameof(SearchDialog), entityType, cancellationToken);

                default:
                    return await GetWelcomeCard(stepContext, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptText = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptText, cancellationToken);
        }

        private async Task<DialogTurnResult> GetWelcomeCard(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get user info from Graph API (delegated permission)
            var userInfo = await _graphService.GetCurrentUserInfo();
            // Get welcome adaptive card
            var card = _fileService.GetCard("WelcomeCard");
            var adaptiveCard = _adaptiveCardService.BindData(card, userInfo);
            var adaptiveCardAttachment = AttachmentHelper.GetAttachment(adaptiveCard);

            return await stepContext.PromptAsync("AdaptiveCardPrompt",
            new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(adaptiveCardAttachment),
            }, cancellationToken);
        }

        private EntityType getEntityType(string type)
        {
            switch (type)
            {
                case EntityTypes.Events:
                case CommandIds.Events:
                    return EntityType.Event;                    
                case EntityTypes.Files:
                case CommandIds.Files:
                    return EntityType.DriveItem;
                case EntityTypes.ListItems:
                case CommandIds.ListItems:
                    return EntityType.ListItem;
                case EntityTypes.Messages:
                case CommandIds.Messages:
                    return EntityType.Message;
                default:
                    return EntityType.UnknownFutureValue;
            }
        }
    }
}
