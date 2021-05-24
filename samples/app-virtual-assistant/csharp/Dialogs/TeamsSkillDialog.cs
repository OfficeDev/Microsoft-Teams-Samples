// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Skills.Models;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Teams.Apps.VirtualAssistant.Dialogs
{
    /// <summary>
    /// A sample dialog that can wrap remote calls to a skill.
    /// </summary>
    /// <remarks>
    /// The options parameter in <see cref="BeginDialogAsync"/> must be a <see cref="SkillDialogArgs"/> instance
    /// with the initial parameters for the dialog.
    /// </remarks>
    public class TeamsSkillDialog : Dialog
    {
        private readonly string _botId;
        private readonly ConversationState _conversationState;
        private readonly SkillHttpClient _skillClient;
        private readonly EnhancedBotFrameworkSkill _skill;
        private readonly Uri _skillHostEndpoint;

        public TeamsSkillDialog(ConversationState conversationState, SkillHttpClient skillClient, EnhancedBotFrameworkSkill skill, IConfiguration configuration, Uri skillHostEndpoint)
            : base(skill?.Id)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _botId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            if (string.IsNullOrWhiteSpace(_botId))
            {
                throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is not in configuration");
            }

            _skillHostEndpoint = skillHostEndpoint;
            _skillClient = skillClient ?? throw new ArgumentNullException(nameof(skillClient));
            _skill = skill ?? throw new ArgumentNullException(nameof(skill));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (!(options is SkillDialogArgs dialogArgs))
            {
                throw new ArgumentNullException(nameof(options), $"Unable to cast {nameof(options)} to {nameof(SkillDialogArgs)}");
            }

            var skillId = dialogArgs.SkillId;

            await dc.Context.TraceActivityAsync($"{GetType().Name}.BeginDialogAsync()", label: $"Using activity of type: {dialogArgs.ActivityType}", cancellationToken: cancellationToken).ConfigureAwait(false);

            Activity skillActivity;
            switch (dialogArgs.ActivityType)
            {
                case ActivityTypes.Event:
                    var eventActivity = Activity.CreateEventActivity();
                    eventActivity.Name = dialogArgs.Name;
                    eventActivity.ApplyConversationReference(dc.Context.Activity.GetConversationReference(), true);
                    skillActivity = (Activity)eventActivity;
                    break;

                case ActivityTypes.Message:
                    var messageActivity = Activity.CreateMessageActivity();
                    messageActivity.Text = dc.Context.Activity.Text;
                    messageActivity.Entities = dc.Context.Activity.Entities;
                    skillActivity = (Activity)messageActivity;
                    skillActivity.Id = dc.Context.Activity.Id;
                    break;

                default:
                    throw new ArgumentException($"Invalid activity type in {dialogArgs.ActivityType} in {nameof(SkillDialogArgs)}");
            }

            ApplyParentActivityProperties(dc.Context, skillActivity, dialogArgs);
            return await this.SendToSkillAsync(dc, skillActivity, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"ActivityType: {dc.Context.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"Got EndOfConversation", cancellationToken: cancellationToken).ConfigureAwait(false);
                return await dc.EndDialogAsync(dc.Context.Activity.Value, cancellationToken).ConfigureAwait(false);
            }

            // Just forward to the remote skill
            return await SendToSkillAsync(dc, dc.Context.Activity, cancellationToken).ConfigureAwait(false);
        }

        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EndOfTurn);
        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default)
        {
            if (reason == DialogReason.CancelCalled || reason == DialogReason.ReplaceCalled)
            {
                await turnContext.TraceActivityAsync($"{GetType().Name}.EndDialogAsync()", label: $"ActivityType: {turnContext.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

                var activity = (Activity)Activity.CreateEndOfConversationActivity();
                ApplyParentActivityProperties(turnContext, activity, null);

                await SendToSkillAsync(null, (Activity)activity, cancellationToken).ConfigureAwait(false);
            }

            await base.EndDialogAsync(turnContext, instance, reason, cancellationToken).ConfigureAwait(false);
        }

        private static void ApplyParentActivityProperties(ITurnContext turnContext, Activity skillActivity, SkillDialogArgs dialogArgs)
        {
            // Apply conversation reference and common properties from incoming activity before sending.
            skillActivity.ApplyConversationReference(turnContext.Activity.GetConversationReference(), true);
            skillActivity.ChannelData = turnContext.Activity.ChannelData;
            skillActivity.Properties = turnContext.Activity.Properties;

            if (dialogArgs != null)
            {
                skillActivity.Value = dialogArgs?.Value;
            }
        }

        private async Task<DialogTurnResult> SendToSkillAsync(DialogContext dc, Activity activity, CancellationToken cancellationToken)
        {
            // add checking on skill configuration before sending over to skill
            // because we can't perform the check in constructor which will fail startup of the app
            if (string.IsNullOrWhiteSpace(_skill.AppId))
            {
                throw new ArgumentNullException(nameof(_skill.AppId));
            }

            if (string.IsNullOrWhiteSpace(_skill.SkillEndpoint.ToString()))
            {
                throw new ArgumentNullException(nameof(_skill.SkillEndpoint));
            }

            if (string.IsNullOrWhiteSpace(_skillHostEndpoint.ToString()))
            {
                throw new ArgumentNullException(nameof(_skillHostEndpoint));
            }

            if (dc != null)
            {
                // Always save state before forwarding
                // (the dialog stack won't get updated with the skillDialog and things won't work if you don't)
                await _conversationState.SaveChangesAsync(dc.Context, true, cancellationToken).ConfigureAwait(false);
            }

            var response = await _skillClient.PostActivityAsync(_botId, _skill, _skillHostEndpoint, activity, cancellationToken).ConfigureAwait(false);
            if (!(response.Status >= 200 && response.Status <= 299))
            {
                throw new HttpRequestException($"Error invoking the skill id: \"{_skill.Id}\" at \"{_skill.SkillEndpoint}\" (status is {response.Status}). \r\n {response.Body}");
            }

            return EndOfTurn;
        }
    }
}