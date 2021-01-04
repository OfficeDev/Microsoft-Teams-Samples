using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Update Text Dialog Class. Main purpose of this class is to Update the Text in Bot
    /// </summary>
    [Serializable]
    public class UpdateTextMsgDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string cachedMessage = string.Empty;

            if (context.UserData.TryGetValue(Strings.SetUpMsgKey, out cachedMessage))
            {
                IMessageActivity reply = context.MakeMessage();
                reply.Text = Strings.UpdateMessagePrompt;

                ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
                ResourceResponse resp = await client.Conversations.UpdateActivityAsync(context.Activity.Conversation.Id, cachedMessage, (Activity)reply);

                await context.PostAsync(Strings.UpdateMessageConfirmation);
            }
            else
            {
                await context.PostAsync(Strings.ErrorTextMessageUpdate);
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogUpdateMessasge);

            context.Done<object>(null);
        }
    }
}