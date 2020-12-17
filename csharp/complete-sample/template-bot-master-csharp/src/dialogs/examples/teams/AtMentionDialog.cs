using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading.Tasks;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is At Mention Dialog Class. Main purpose of this dialog class is to Atmention (@mention)
    /// the user who has started the conversation.
    /// we can pass ChannelAccount object as first parameter of AddMentionToText method to @mention any user.
    /// e.g. ChannelAccount userInformation = new ChannelAccount("29:1TPHVQrnqOI3_FZbeY32VvlBwo1trPhN96SiKYP3av-QCKYGlLBApj-w9fgoI9SCUz4bEmzo9tVlSQdHzgoSzeQ", "Ashish");
    /// </summary>

    [Serializable]
    public class AtMentionDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var message = context.MakeMessage();
            Activity replyActivity = message as Activity;
            replyActivity.AddMentionToText(context.Activity.From, MentionTextLocation.PrependText);
            await context.PostAsync(replyActivity);

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogAtMentionDialog);
            context.Done<object>(null);
        }
    }
}