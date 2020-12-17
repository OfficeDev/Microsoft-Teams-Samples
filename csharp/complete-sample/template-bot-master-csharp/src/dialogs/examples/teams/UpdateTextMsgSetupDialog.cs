using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Update Setup Text Dialog Class. Main purpose of this class is to Setup the text message that will be update later in Bot example.
    /// </summary>
    [Serializable]
    public class UpdateTextMsgSetupDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IMessageActivity reply = context.MakeMessage();
            reply.Text = Strings.SetupMessagePrompt;

            ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            ResourceResponse resp = await client.Conversations.ReplyToActivityAsync((Activity)reply);

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogSetupMessasge);

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.SetUpMsgKey, resp.Id.ToString());

            context.Done<object>(null);
        }
    }
}