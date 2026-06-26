using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using AppCompleteSample;
using AppCompleteSample.src.dialogs;
using AppCompleteSample.Utility;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is DeepLink Dialog Class. Main purpose of this class is to show Deep link from Bot to Tab example
    /// </summary>
    public class DeepLinkStaticTabDialog : ComponentDialog
    {
        private const string TabEntityID = "statictab";
        private const string TabConfigEntityID = "configTab";
        private string BotId { get; set; }
        private bool IsChannelUser { get; set; } = false;
        private string ChannelId { get; set; }
        private string TabUrl { get; set; }
        private string ButtonCaption { get; set; }
        private string DeepLinkCardTitle { get; set; }

        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;

        private readonly IOptions<AzureSettings> azureSettings;
        public DeepLinkStaticTabDialog(IStatePropertyAccessor<RootDialogState> conversationState, IOptions<AzureSettings> azureSettings) : base(nameof(DeepLinkStaticTabDialog))
        {
            this._conversationState = conversationState;
            this.azureSettings = azureSettings;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginDeepLinkStaticTabDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginDeepLinkStaticTabDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }
            
            BotId = this.azureSettings.Value.MicrosoftAppId;

            GetChannelID(stepContext);

            var message = CreateDeepLinkMessage(stepContext);

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogDeepLinkStaticTabDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            await stepContext.Context.SendActivityAsync(message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private IMessageActivity CreateDeepLinkMessage(WaterfallStepContext context)
        {
            var message = context.Context.Activity;
            if (message.Attachments != null)
            {
                message.Attachments = null;
            }

            if (message.Entities.Count >= 1)
            {
                message.Entities.Remove(message.Entities[0]);
            }
            var attachment = CreateDeepLinkCard();
            message.Attachments = new List<Attachment>() { attachment };
            return message;
        }

        private Attachment CreateDeepLinkCard()
        {
            if (IsChannelUser)
            {
                TabUrl = GetConfigTabDeepLinkURL(ChannelId);
                ButtonCaption = Strings.DeepLinkCardConfigButtonCaption;
                DeepLinkCardTitle = Strings.DeepLinkCardConfigTitle;
            }
            else
            {
                TabUrl = GetStaticTabDeepLinkURL();
                ButtonCaption = Strings.DeepLinkCard1To1ButtonCaption;
                DeepLinkCardTitle = Strings.DeepLinkCard1To1Title;
            }

            return new HeroCard
            {
                Title = DeepLinkCardTitle,
                Buttons = new List<CardAction>
                {
                   new CardAction(ActionTypes.OpenUrl, ButtonCaption, value: TabUrl),
                }
            }.ToAttachment();
        }
        private string GetStaticTabDeepLinkURL()
        {
            //Example -  BaseURL + 28:BotId + TabEntityId (set in the manifest) + ?conversationType=chat
            return "https://teams.microsoft.com/l/entity/28:" + BotId + "/" + TabEntityID + "?conversationType=chat";
        }

        private string GetConfigTabDeepLinkURL(string channelId)
        {
            //Example -  BaseURL + BotId + TabConfigEntityId (e.g. entityId: "configTab" : it should be same which we have set at the time of Tab Creation like below) + ?context= + {"channelId":"19:47051e5643ed49b58665e1250b6db460@thread.skype"} (should be encoded)
            //microsoftTeams.settings.setSettings({ suggestedDisplayName: "Bot Info", contentUrl: createTabUrl(), entityId: "configTab" });

            channelId = channelId.Replace("19:", "19%3a")
                                     .Replace("@thread.skype", "%40thread.skype");

            return "https://teams.microsoft.com/l/entity/" + BotId + "/" + TabConfigEntityID + "?context=%7B%22channelId%22%3A%22" + channelId + "%22%7D";
        }

        private void GetChannelID(WaterfallStepContext context)
        {

            IsChannelUser = false;

            if (context.Context.Activity.ChannelData != null)
            {
                ChannelId = context.Context.Activity.ChannelId;

                if (!string.IsNullOrEmpty(ChannelId))
                {
                    IsChannelUser = true;
                }
                else
                {
                    IsChannelUser = false;
                }
            }
        }
    }
}