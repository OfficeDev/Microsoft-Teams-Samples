using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is At Mention Dialog Class. Main purpose of this dialog class is to Atmention (@mention)
    /// the user who has started the conversation.
    /// we can pass ChannelAccount object as first parameter of AddMentionToText method to @mention any user.
    /// e.g. ChannelAccount userInformation = new ChannelAccount("29:1TPHVQrnqOI3_FZbeY32VvlBwo1trPhN96SiKYP3av-QCKYGlLBApj-w9fgoI9SCUz4bEmzo9tVlSQdHzgoSzeQ", "Ashish");
    /// </summary>
    public class AtMentionDialog : ComponentDialog
    {
        public AtMentionDialog() : base(nameof(AtMentionDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginFormflowAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginFormflowAsync(
WaterfallStepContext stepContext,
CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            var message = stepContext.Context.Activity;
            Activity replyActivity = message as Activity;
            replyActivity.GetMentions();
            await stepContext.Context.SendActivityAsync(replyActivity);

            //Set the Last Dialog in Conversation Data
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogAtMentionDialog);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}