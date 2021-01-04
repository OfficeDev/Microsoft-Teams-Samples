using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Fetch Roster Payload Dialog Class. Main purpose of this dialog class is to Call the Roster Api and Post the 
    /// full JSON Payload in Teams returned by Roster Api.
    /// </summary>
    [Serializable]
    public class FetchRosterDialog : IDialog<object>
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
            string output = JsonConvert.SerializeObject(response);

            var message = context.MakeMessage();
            message.Text = output;

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogFetchPayloadRosterDialog);

            await context.PostAsync(message);

            context.Done<object>(null);
        }
    }
}