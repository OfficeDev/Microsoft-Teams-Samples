// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly BotState _conversationstate;
        protected readonly Dialog _dialog;
        protected readonly ILogger _logger;
        protected readonly BotState _userState;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            _conversationstate = conversationState;
            _userState = userState;
            _dialog = dialog;
            _logger = logger;
        }
        // Save any state changes that might have occurred during the turn.
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationstate.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        //  Run the Dialog with the new message Activity.
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

           
            await _dialog.RunAsync(turnContext, _conversationstate.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
    }
}
