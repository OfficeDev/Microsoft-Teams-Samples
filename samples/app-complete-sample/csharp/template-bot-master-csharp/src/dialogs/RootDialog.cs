using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Root Dialog, its a triggring point for every Child dialog based on the RexEx Match with user input command
    /// </summary>

    public class RootDialog : ComponentDialog
    {
        private static List<Choice> CommandList = new List<Choice>()
            {
                new Choice(DialogMatches.FetchRosterPayloadMatch) { Synonyms = new List<string> { DialogMatches.FetchRosterPayloadMatch } },
                new Choice(DialogMatches.FetchRosterApiMatch) { Synonyms = new List<string> { DialogMatches.FetchRosterApiMatch } },
                new Choice(DialogMatches.HelloDialogMatch1)  { Synonyms = new List<string> { "hi" } }
            };
        public RootDialog()
            : base(nameof(RootDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptForOptionsAsync,
                ShowChildDialogAsync,
                ResumeAfterAsync,
            }));
            AddDialog(new FetchRosterDialog());
            AddDialog(new ListNamesDialog());
            AddDialog(new HelloDialog());
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        }

        private async Task<DialogTurnResult> PromptForOptionsAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Prompt the user for a response using our choice prompt.
            return await stepContext.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions()
                {
                    Choices = CommandList,
                    Prompt = MessageFactory.Text("hello"),
                    RetryPrompt = MessageFactory.Text(Strings.ErrorMessage)
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> ShowChildDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // string optionSelected = await userReply;
            var optionSelected = (stepContext.Result as FoundChoice).Value;

            switch (optionSelected)
            {
                case DialogMatches.FetchRosterPayloadMatch:
                    //context.Call(new InstallAppDialog(), this.ResumeAfterOptionDialog);
                    //break;
                    return await stepContext.BeginDialogAsync(
                        nameof(FetchRosterDialog),
                        cancellationToken);
                case DialogMatches.FetchRosterApiMatch:
                    //context.Call(new ResetPasswordDialog(), this.ResumeAfterOptionDialog);
                    //break;
                    return await stepContext.BeginDialogAsync(
                        nameof(ListNamesDialog),
                        cancellationToken);
                case DialogMatches.HelloDialogMatch1:
                    //context.Call(new LocalAdminDialog(), this.ResumeAfterOptionDialog);
                    //break;
                    return await stepContext.BeginDialogAsync(
                        nameof(HelloDialog),
                        cancellationToken);
            }

            // We shouldn't get here, but fail gracefully if we do.
            await stepContext.Context.SendActivityAsync(
                "I don't recognize that option.",
                cancellationToken: cancellationToken);
            // Continue through to the next step without starting a child dialog.
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ResumeAfterAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                //var message = await userReply;
                var message = stepContext.Context.Activity;

                var ticketNumber = new Random().Next(0, 20000);
                //await context.PostAsync($"Thank you for using the Helpdesk Bot. Your ticket number is {ticketNumber}.");
                await stepContext.Context.SendActivityAsync(
                    $"Thank you for using the Helpdesk Bot. Your ticket number is {ticketNumber}.",
                    cancellationToken: cancellationToken);

                //context.Done(ticketNumber);
            }
            catch (Exception ex)
            {
                // await context.PostAsync($"Failed with message: {ex.Message}");
                await stepContext.Context.SendActivityAsync(
                    $"Failed with message: {ex.Message}",
                    cancellationToken: cancellationToken);

                // In general resume from task after calling a child dialog is a good place to handle exceptions
                // try catch will capture exceptions from the bot framework awaitable object which is essentially "userReply"
            }

            // Replace on the stack the current instance of the waterfall with a new instance,
            // and start from the top.
            return await stepContext.ReplaceDialogAsync(
                nameof(WaterfallDialog),
                cancellationToken: cancellationToken);
        }
    }
}