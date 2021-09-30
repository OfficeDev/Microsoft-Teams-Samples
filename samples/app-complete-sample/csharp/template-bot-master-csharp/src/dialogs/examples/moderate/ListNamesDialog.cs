using Bot.Builder.Community.Dialogs.FormFlow;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Fetch Roster Dialog Class. Main purpose of this dialog class is to Call the Roster Api and Post the 
    /// members information (Name and Id) in Teams. This Dialog is using Thumbnail Card to show the member information in teams.
    /// </summary>
    public class ListNamesDialog : ComponentDialog
    {
        public ListNamesDialog() : base(nameof(ListNamesDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginListNamesDialogAsync
            }));
        }

        private async Task<DialogTurnResult> BeginListNamesDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await stepContext.Context.SendActivityAsync(Strings.RosterWelcomeMsgTitle);

            var connectorClient = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl), ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);

            var response = await connectorClient.Conversations.GetConversationMembersAsync(stepContext.Context.Activity.Conversation.Id);
            string output = JsonConvert.SerializeObject(response);

            var message = stepContext.MakeMessage();
            message.Text = output;

            foreach (var obj in response.ToList())
            {
                message.Attachments.Add(GetUserInformationCard(obj.Name, Convert.ToString(obj.Properties["objectId"])));
            }
            await stepContext.Context.SendActivityAsync(message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        /// <summary>
        /// Create Card Template with Channel member information
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        private static Attachment GetUserInformationCard(string userName, string objectId)
        {
            string chatUrl = "https://teams.microsoft.com/l/chat/0/0?users=8:orgid:" + objectId;
            var heroCard = new ThumbnailCard
            {
                Title = userName,
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, Strings.CaptionChatButton, value: chatUrl) }
            };

            return heroCard.ToAttachment();
        }
    }
}