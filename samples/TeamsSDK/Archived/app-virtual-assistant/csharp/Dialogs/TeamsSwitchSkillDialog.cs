// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Skills.Dialogs;

namespace Microsoft.Teams.Apps.VirtualAssistant.Dialogs
{
    public class TeamsSwitchSkillDialog : ComponentDialog
    {
        private static string _confirmPromptId = "ConfirmSkillSwitch";
        private IStatePropertyAccessor<string> _skillIdAccessor;
        private IStatePropertyAccessor<Activity> _lastActivityAccessor;

        public TeamsSwitchSkillDialog(ConversationState conversationState)
            : base(nameof(TeamsSwitchSkillDialog))
        {
            _skillIdAccessor = conversationState.CreateProperty<string>(Properties.SkillId);
            _lastActivityAccessor = conversationState.CreateProperty<Activity>(Properties.LastActivity);

            var intentSwitch = new WaterfallStep[]
            {
                PromptToSwitchAsync,
                EndAsync,
            };

            AddDialog(new WaterfallDialog(nameof(TeamsSwitchSkillDialog), intentSwitch));
            AddDialog(new ConfirmPrompt(_confirmPromptId));
        }

        // Runs when this dialog ends. Handles result of prompt to switch skills or resume waiting dialog.
        protected override async Task<DialogTurnResult> EndComponentAsync(DialogContext outerDc, object result, CancellationToken cancellationToken)
        {
            var skillId = await _skillIdAccessor.GetAsync(outerDc.Context, () => null).ConfigureAwait(false);
            var lastActivity = await _lastActivityAccessor.GetAsync(outerDc.Context, () => null).ConfigureAwait(false);
            outerDc.Context.Activity.Text = lastActivity.Text;
            outerDc.Context.Activity.Entities = lastActivity.Entities;
            outerDc.Context.Activity.Id = lastActivity.Id;

            // Ends this dialog.
            await outerDc.EndDialogAsync().ConfigureAwait(false);

            if ((bool)result)
            {
                // If user decided to switch, replace current skill dialog with new skill dialog.
                var skillDialogArgs = new SkillDialogArgs { SkillId = skillId, Value = lastActivity.Value, ActivityType = lastActivity.Type };

                // Start the skill dialog.
                return await outerDc.ReplaceDialogAsync(skillId, skillDialogArgs).ConfigureAwait(false);
            }
            else
            {
                // Otherwise, continue the waiting skill dialog with the user's previous utterance.
                return await outerDc.ContinueDialogAsync().ConfigureAwait(false);
            }
        }

        // Prompts user to switch to a new skill.
        private async Task<DialogTurnResult> PromptToSwitchAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = stepContext.Options as SwitchSkillDialogOptions ?? throw new ArgumentException($"You must provide options of type {typeof(SwitchSkillDialogOptions).FullName}.");
            await _skillIdAccessor.SetAsync(stepContext.Context, options.Skill.Id).ConfigureAwait(false);
            await _lastActivityAccessor.SetAsync(stepContext.Context, stepContext.Context.Activity).ConfigureAwait(false);

            return await stepContext.PromptAsync(_confirmPromptId, options).ConfigureAwait(false);
        }

        // Ends this dialog, returning the prompt result.
        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool result = (bool)stepContext.Result;
            return await stepContext.EndDialogAsync(result: result).ConfigureAwait(false);
        }

        private class Properties
        {
            public const string SkillId = "skillSwitchValue";
            public const string LastActivity = "skillSwitchActivity";
        }
    }
}