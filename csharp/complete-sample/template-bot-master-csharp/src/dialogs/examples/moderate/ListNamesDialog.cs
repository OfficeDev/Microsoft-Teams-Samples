using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Fetch Roster Dialog Class. Main purpose of this dialog class is to Call the Roster Api and Post the 
    /// members information (Name and Id) in Teams. This Dialog is using Thumbnail Card to show the member information in teams.
    /// </summary>
    [Serializable]
    public class ListNamesDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await context.PostAsync(Strings.RosterWelcomeMsgTitle);
            var connectorClient = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            var response = await connectorClient.Conversations.GetConversationMembersAsync(context.Activity.Conversation.Id);
            var message = context.MakeMessage();

            foreach (var obj in response.ToList())
            {
                message.Attachments.Add(GetUserInformationCard(obj.Name, Convert.ToString(obj.Properties["objectId"])));
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogFetchRosterDialog);

            await context.PostAsync(message);

            context.Done<object>(null);
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