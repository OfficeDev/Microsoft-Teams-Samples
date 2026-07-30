// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Solutions;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Teams.Apps.VirtualAssistant.Extension;
using Microsoft.Teams.Apps.VirtualAssistant.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Teams.Apps.VirtualAssistant.Bots
{
    public class DefaultActivityHandler<T> : TeamsActivityHandler
        where T : Dialog
    {
        private readonly Dialog _dialog;
        private readonly BotState _conversationState;
        private IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private LocaleTemplateEngineManager _templateEngine;
        private readonly SkillHttpClient _skillHttpClient;
        private readonly SkillsConfiguration _skillsConfig;
        private readonly IBotTelemetryClient _telemetryClient;
        private readonly string _appId;
        private readonly string _composeExtensionCommandIdSeparator;

        public DefaultActivityHandler(IServiceProvider serviceProvider, T dialog, string appId)
        {
            _dialog = dialog;
            _conversationState = serviceProvider.GetService<ConversationState>();
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _templateEngine = serviceProvider.GetService<LocaleTemplateEngineManager>();
            _skillHttpClient = serviceProvider.GetService<SkillHttpClient>();
            _skillsConfig = serviceProvider.GetService<SkillsConfiguration>();
            _telemetryClient = serviceProvider.GetService<IBotTelemetryClient>();
            this._appId = appId;
            _composeExtensionCommandIdSeparator = ":";
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            return _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        protected override Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var ev = turnContext.Activity.AsEventActivity();
            var value = ev.Value?.ToString();

            switch (ev.Name)
            {
                case TokenEvents.TokenResponseEventName:
                    {
                        // Forward the token response activity to the dialog waiting on the stack.
                        await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
                        break;
                    }

                default:
                    {
                        await turnContext.SendActivityAsync(new Activity(type: ActivityTypes.Trace, text: $"Unknown Event '{ev.Name ?? "undefined"}' was received but not processed."));
                        break;
                    }
            }
        }

        protected override async Task OnEndOfConversationActivityAsync(ITurnContext<IEndOfConversationActivity> turnContext, CancellationToken cancellationToken)
        {
            await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        // Invoked when a "task/fetch" event is received to invoke task module.
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            try
            {
                string skillId = (turnContext.Activity as Activity).GetSkillId();
                var skill = _skillsConfig.Skills.Where(s => s.Value.AppId == skillId).FirstOrDefault().Value;

                // Forward request to correct skill
                var invokeResponse = await _skillHttpClient.PostActivityAsync(this._appId, skill, _skillsConfig.SkillHostEndpoint, turnContext.Activity as Activity, cancellationToken);

                return invokeResponse.GetTaskModuleRespose();
            }
            catch (Exception exception)
            {
                await turnContext.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorMessage"));
                _telemetryClient.TrackException(exception);

                return null;
            }
        }

        // Invoked when a 'task/submit' invoke activity is received for task module submit actions.
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            try
            {
                string skillId = (turnContext.Activity as Activity).GetSkillId();
                var skill = _skillsConfig.Skills.Where(s => s.Value.AppId == skillId).FirstOrDefault().Value;

                // Forward request to correct skill
                var invokeResponse = await _skillHttpClient.PostActivityAsync(this._appId, skill, _skillsConfig.SkillHostEndpoint, turnContext.Activity as Activity, cancellationToken).ConfigureAwait(false);

                return invokeResponse.GetTaskModuleRespose();
            }
            catch (Exception exception)
            {
                await turnContext.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorMessage"));
                _telemetryClient.TrackException(exception);

                return null;
            }
        }

        // Invoked when a 'composeExtension/query' invoke activity is received for compose extension query command.
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var skillId = ExtractSkillIdFromComposeExtensionQueryCommand(turnContext, query);
            var skill = _skillsConfig.Skills.Where(s => s.Value.AppId == skillId).FirstOrDefault().Value;
            var invokeResponse = await _skillHttpClient.PostActivityAsync(this._appId, skill, _skillsConfig.SkillHostEndpoint, turnContext.Activity as Activity, cancellationToken).ConfigureAwait(false);

            return invokeResponse.GetMessagingExtensionResponse();
        }

        // Invoked when a 'composeExtension/selectItem' invoke activity is received for compose extension query command.
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            var data = JsonConvert.DeserializeObject<SkillCardActionData>(query.ToString());
            var skill = _skillsConfig.Skills.Where(s => s.Value.AppId == data.SkillId).FirstOrDefault().Value;
            var invokeResponse = await _skillHttpClient.PostActivityAsync(this._appId, skill, _skillsConfig.SkillHostEndpoint, turnContext.Activity as Activity, cancellationToken).ConfigureAwait(false);

            return invokeResponse.GetMessagingExtensionResponse();
        }

        // Invoked when a 'composeExtension/submitAction' invoke activity is received for compose extension action command.
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            return await ForwardMessagingExtensionActionCommandActivityToSkill(turnContext, action, cancellationToken);
        }

        // Invoked when a 'composeExtension/submitAction' invoke activity is received for compose extension edit preview action command.
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            return await ForwardMessagingExtensionActionCommandActivityToSkill(turnContext, action, cancellationToken);
        }

        // Invoked when a 'composeExtension/submitAction' invoke activity is received for compose extension send preview action command.
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            return await ForwardMessagingExtensionActionCommandActivityToSkill(turnContext, action, cancellationToken);
        }

        // Invoked when a 'composeExtension/fetchTask' invoke activity is received for compose extension action command.
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            return await ForwardMessagingExtensionActionCommandActivityToSkill(turnContext, action, cancellationToken);
        }

        // Forwards invoke activity to right skill for compose extension action commands.
        private async Task<MessagingExtensionActionResponse> ForwardMessagingExtensionActionCommandActivityToSkill(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var skillId = ExtractSkillIdFromComposeExtensionActionCommand(turnContext, action);
            var skill = _skillsConfig.Skills.Where(s => s.Value.AppId == skillId).FirstOrDefault().Value;
            var invokeResponse = await _skillHttpClient.PostActivityAsync(this._appId, skill, _skillsConfig.SkillHostEndpoint, turnContext.Activity as Activity, cancellationToken).ConfigureAwait(false);

            return invokeResponse.GetMessagingExtensionActionResponse();
        }

        // Extracts skill Id from messaging extension query command and updates activity value
        private string ExtractSkillIdFromComposeExtensionQueryCommand(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query)
        {
            var commandArray = query.CommandId.Split(_composeExtensionCommandIdSeparator);
            var skillId = commandArray.Last();

            // Update activity value by removing skill id before forwarding to the skill.
            var activityValue = JsonConvert.DeserializeObject<MessagingExtensionQuery>(turnContext.Activity.Value.ToString());
            activityValue.CommandId = string.Join(_composeExtensionCommandIdSeparator, commandArray.Take(commandArray.Length - 1));
            turnContext.Activity.Value = activityValue;

            return skillId;
        }

        // Extracts skill Id from messaging extension command and updates activity value
        private string ExtractSkillIdFromComposeExtensionActionCommand(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var commandArray = action.CommandId.Split(_composeExtensionCommandIdSeparator);
            var skillId = commandArray.Last();

            // Update activity value by removing skill id before forwarding to the skill.
            var activityValue = JsonConvert.DeserializeObject<MessagingExtensionAction>(turnContext.Activity.Value.ToString());
            activityValue.CommandId = string.Join(_composeExtensionCommandIdSeparator, commandArray.Take(commandArray.Length - 1));
            turnContext.Activity.Value = activityValue;

            return skillId;
        }
    }
}