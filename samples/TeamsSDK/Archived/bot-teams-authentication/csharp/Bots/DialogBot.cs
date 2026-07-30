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
    /// <summary>
    /// This IBot implementation can run any type of Dialog. The use of type parameterization allows multiple different bots
    /// to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    /// each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    /// The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    /// and the requirement is that all BotState objects are saved at the end of a turn.
    /// </summary>
    /// <typeparam name="T">The type of Dialog to be run.</typeparam>
    public class DialogBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly BotState _conversationState;
        protected readonly Dialog _dialog;
        protected readonly ILogger _logger;
        protected readonly BotState _userState;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogBot{T}"/> class.
        /// </summary>
        /// <param name="conversationState">The conversation state.</param>
        /// <param name="userState">The user state.</param>
        /// <param name="dialog">The dialog to be run.</param>
        /// <param name="logger">The logger.</param>
        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _logger = logger;
        }

        /// <summary>
        /// Called at the end of each turn. Saves any state changes that might have occurred during the turn.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Handles the message activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
    }
}
