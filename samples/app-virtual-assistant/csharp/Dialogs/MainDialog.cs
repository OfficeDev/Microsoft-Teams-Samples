// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Extensions;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Skills.Dialogs;
using Microsoft.Bot.Solutions.Skills.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Teams.Apps.VirtualAssistant.Extension;
using Microsoft.Teams.Apps.VirtualAssistant.Models;
using Microsoft.Teams.Apps.VirtualAssistant.Services;

namespace Microsoft.Teams.Apps.VirtualAssistant.Dialogs
{
    // Dialog providing activity routing and message/event processing.
    public class MainDialog : ComponentDialog
    {
        private BotServices _services;
        private BotSettings _settings;
        private TeamsSwitchSkillDialog _switchSkillDialog;
        private SkillsConfiguration _skillsConfig;
        private LocaleTemplateEngineManager _templateEngine;
        private IStatePropertyAccessor<List<Activity>> _previousResponseAccessor;

        public MainDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(MainDialog))
        {
            _services = serviceProvider.GetService<BotServices>();
            _settings = serviceProvider.GetService<BotSettings>();
            _templateEngine = serviceProvider.GetService<LocaleTemplateEngineManager>();
            _skillsConfig = serviceProvider.GetService<SkillsConfiguration>();
            TelemetryClient = telemetryClient;

            var conversationState = serviceProvider.GetService<ConversationState>();
            _previousResponseAccessor = conversationState.CreateProperty<List<Activity>>(StateProperties.PreviousBotResponse);

            var steps = new WaterfallStep[]
            {
                RouteStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(MainDialog), steps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            InitialDialogId = nameof(MainDialog);

            // Register dialogs
            _switchSkillDialog = serviceProvider.GetService<TeamsSwitchSkillDialog>();
            AddDialog(_switchSkillDialog);

            // Register a QnAMakerDialog for each registered knowledgebase and ensure localised responses are provided.
            var localizedServices = _services.GetCognitiveModels();
            foreach (var knowledgebase in localizedServices.QnAConfiguration)
            {
                var qnaDialog = new QnAMakerDialog(
                    knowledgeBaseId: knowledgebase.Value.KnowledgeBaseId,
                    endpointKey: knowledgebase.Value.EndpointKey,
                    hostName: knowledgebase.Value.Host,
                    noAnswer: _templateEngine.GenerateActivityForLocale("UnsupportedMessage"),
                    activeLearningCardTitle: _templateEngine.GenerateActivityForLocale("QnaMakerAdaptiveLearningCardTitle").Text,
                    cardNoMatchText: _templateEngine.GenerateActivityForLocale("QnaMakerNoMatchText").Text)
                {
                    Id = knowledgebase.Key
                };
                AddDialog(qnaDialog);
            }

            // Register skill dialogs
            var skillDialogs = serviceProvider.GetServices<TeamsSkillDialog>();
            foreach (var dialog in skillDialogs)
            {
                AddDialog(dialog);
            }
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                if (innerDc.Context.Activity.Conversation?.IsGroup == true)
                {
                    // Remove bot atmentions for teams/groupchat scope
                    innerDc.Context.Activity.RemoveRecipientMention();

                    // If bot is invoked without any text, reply with FirstPromptMessage
                    if (string.IsNullOrWhiteSpace(innerDc.Context.Activity.Text))
                    {
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("FirstPromptMessage"));
                        return EndOfTurn;
                    }
                }

                // Get cognitive models for the current locale.
                var localizedServices = _services.GetCognitiveModels();

                // Run LUIS recognition and store result in turn state.
                var dispatchResult = await localizedServices.DispatchService.RecognizeAsync<DispatchLuis>(innerDc.Context, cancellationToken);
                innerDc.Context.TurnState.Add(StateProperties.DispatchResult, dispatchResult);

                if (dispatchResult.TopIntent().intent == DispatchLuis.Intent.l_General)
                {
                    // Run LUIS recognition on General model and store result in turn state.
                    var generalResult = await localizedServices.LuisServices["General"].RecognizeAsync<GeneralLuis>(innerDc.Context, cancellationToken);
                    innerDc.Context.TurnState.Add(StateProperties.GeneralResult, generalResult);
                }

                // Check for any interruptions
                var interrupted = await InterruptDialogAsync(innerDc, cancellationToken);

                if (interrupted)
                {
                    // If dialog was interrupted, return EndOfTurn
                    return EndOfTurn;
                }
            }

            // Set up response caching for "repeat" functionality.
            innerDc.Context.OnSendActivities(StoreOutgoingActivities);
            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                if (innerDc.Context.Activity.Conversation?.IsGroup == true)
                {
                    // Remove bot atmentions for teams/groupchat scope
                    innerDc.Context.Activity.RemoveRecipientMention();

                    // If bot is invoked without any text, reply with FirstPromptMessage
                    if (string.IsNullOrWhiteSpace(innerDc.Context.Activity.Text))
                    {
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("FirstPromptMessage"));
                        return EndOfTurn;
                    }
                }

                // Get cognitive models for the current locale.
                var localizedServices = _services.GetCognitiveModels();

                // Run LUIS recognition and store result in turn state.
                var dispatchResult = await localizedServices.DispatchService.RecognizeAsync<DispatchLuis>(innerDc.Context, cancellationToken);
                innerDc.Context.TurnState.Add(StateProperties.DispatchResult, dispatchResult);

                if (dispatchResult.TopIntent().intent == DispatchLuis.Intent.l_General)
                {
                    // Run LUIS recognition on General model and store result in turn state.
                    var generalResult = await localizedServices.LuisServices["General"].RecognizeAsync<GeneralLuis>(innerDc.Context, cancellationToken);
                    innerDc.Context.TurnState.Add(StateProperties.GeneralResult, generalResult);
                }

                // Check for any interruptions
                var interrupted = await InterruptDialogAsync(innerDc, cancellationToken);

                if (interrupted)
                {
                    // If dialog was interrupted, return EndOfTurn
                    return EndOfTurn;
                }
            }

            // Set up response caching for "repeat" functionality.
            innerDc.Context.OnSendActivities(StoreOutgoingActivities);
            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task SwitchSkillPrompt(DialogContext innerDc, EnhancedBotFrameworkSkill identifiedSkill)
        {
            var prompt = _templateEngine.GenerateActivityForLocale("SkillSwitchPrompt", new { Skill = identifiedSkill.Name });
            await innerDc.BeginDialogAsync(_switchSkillDialog.Id, new SwitchSkillDialogOptions(prompt, identifiedSkill));
        }

        private async Task<bool> InterruptDialogAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            var interrupted = false;
            var activity = innerDc.Context.Activity;
            var dialog = innerDc.ActiveDialog?.Id != null ? innerDc.FindDialog(innerDc.ActiveDialog?.Id) : null;

            if (activity.Type == ActivityTypes.Message && !string.IsNullOrEmpty(activity.Text))
            {
                // Check if the active dialog is a skill for conditional interruption.
                var isSkill = dialog is TeamsSkillDialog;
                EnhancedBotFrameworkSkill identifiedSkill;
                var skillId = innerDc.Context.Activity.GetSkillId();
                // Get Dispatch LUIS result from turn state.
                var dispatchResult = innerDc.Context.TurnState.Get<DispatchLuis>(StateProperties.DispatchResult);
                (var dispatchIntent, var dispatchScore) = dispatchResult.TopIntent();

                // Check if skill id is present in activity payload
                if (!string.IsNullOrWhiteSpace(skillId))
                {
                    identifiedSkill = _skillsConfig.Skills.Where(s => s.Value.AppId == skillId).FirstOrDefault().Value;

                    // Check if skill identified through activity payload is not the current active skill
                    if (isSkill && !dialog.Id.Equals(identifiedSkill.Id))
                    {
                        await SwitchSkillPrompt(innerDc, identifiedSkill);
                        interrupted = true;
                    }
                }
                else
                {
                    // Check if we need to switch skills.
                    if (isSkill && IsSkillIntent(dispatchIntent) && dispatchIntent.ToString() != dialog.Id && dispatchScore > 0.9)
                    {
                        if (_skillsConfig.Skills.TryGetValue(dispatchIntent.ToString(), out identifiedSkill))
                        {
                            await SwitchSkillPrompt(innerDc, identifiedSkill);
                            interrupted = true;
                        }
                        else
                        {
                            throw new ArgumentException($"{dispatchIntent.ToString()} is not in the skills configuration");
                        }
                    }
                }

                if (dispatchIntent == DispatchLuis.Intent.l_General)
                {
                    // Get connected LUIS result from turn state.
                    var generalResult = innerDc.Context.TurnState.Get<GeneralLuis>(StateProperties.GeneralResult);
                    (var generalIntent, var generalScore) = generalResult.TopIntent();

                    if (generalScore > 0.5)
                    {
                        switch (generalIntent)
                        {
                            case GeneralLuis.Intent.Cancel:
                                {
                                    if (!isSkill)
                                    {
                                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CancelledMessage"));
                                        await innerDc.CancelAllDialogsAsync();
                                        interrupted = true;    
                                    }

                                    break;
                                }

                            case GeneralLuis.Intent.Escalate:
                                {
                                    await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("EscalateMessage"));
                                    await innerDc.RepromptDialogAsync();
                                    interrupted = true;
                                    break;
                                }

                            case GeneralLuis.Intent.Help:
                                {
                                    if (!isSkill)
                                    {
                                        // If current dialog is a skill, allow it to handle its own help intent.
                                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("HelpCard"));
                                        await innerDc.RepromptDialogAsync();
                                        interrupted = true;
                                    }

                                    break;
                                }

                            case GeneralLuis.Intent.Logout:
                                {
                                    if (!isSkill)
                                    {
                                        // Log user out of all accounts.
                                        await LogUserOut(innerDc);

                                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("LogoutMessage"));
                                        await innerDc.CancelAllDialogsAsync();
                                        interrupted = true;
                                    }
                                        
                                    break;
                                }

                            case GeneralLuis.Intent.Repeat:
                                {
                                    // Sends the activities since the last user message again.
                                    var previousResponse = await _previousResponseAccessor.GetAsync(innerDc.Context, () => new List<Activity>());

                                    foreach (var response in previousResponse)
                                    {
                                        // Reset id of original activity so it can be processed by the channel.
                                        response.Id = string.Empty;
                                        await innerDc.Context.SendActivityAsync(response);
                                    }

                                    interrupted = true;
                                    break;
                                }

                            case GeneralLuis.Intent.StartOver:
                                {
                                    await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("StartOverMessage"));

                                    // Cancel all dialogs on the stack.
                                    await innerDc.CancelAllDialogsAsync();
                                    interrupted = true;
                                    break;
                                }

                            case GeneralLuis.Intent.Stop:
                                {
                                    // Use this intent to send an event to your device that can turn off the microphone in speech scenarios.
                                    break;
                                }
                        }
                    }
                }
            }

            return interrupted;
        }

        private async Task<DialogTurnResult> RouteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get dispatch result from turn state.
            var dispatchResult = stepContext.Context.TurnState.Get<DispatchLuis>(StateProperties.DispatchResult);
            (var dispatchIntent, var dispatchScore) = dispatchResult.TopIntent();

            if (IsSkillIntent(dispatchIntent))
            {
                var dispatchIntentSkill = dispatchIntent.ToString();
                var skillDialogArgs = new SkillDialogArgs { SkillId = dispatchIntentSkill };

                // Start the skill dialog.
                return await stepContext.BeginDialogAsync(dispatchIntentSkill, skillDialogArgs);
            }
            else if (dispatchIntent == DispatchLuis.Intent.q_Faq)
            {
                stepContext.SuppressCompletionMessage(true);

                return await stepContext.BeginDialogAsync("Faq");
            }
            else if (dispatchIntent == DispatchLuis.Intent.q_Chitchat)
            {
                stepContext.SuppressCompletionMessage(true);

                return await stepContext.BeginDialogAsync("Chitchat");
            }
            else
            {
                stepContext.SuppressCompletionMessage(true);
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UnsupportedMessage"));

                return await stepContext.EndDialogAsync();
            }
        }

        private async Task LogUserOut(DialogContext dc)
        {
            IUserTokenProvider tokenProvider;
            var supported = dc.Context.Adapter is IUserTokenProvider;
            if (supported)
            {
                tokenProvider = (IUserTokenProvider)dc.Context.Adapter;

                // Sign out user
                var tokens = await tokenProvider.GetTokenStatusAsync(dc.Context, dc.Context.Activity.From.Id);
                foreach (var token in tokens)
                {
                    await tokenProvider.SignOutUserAsync(dc.Context, token.ConnectionName);
                }

                // Cancel all active dialogs
                await dc.CancelAllDialogsAsync();
            }
            else
            {
                throw new InvalidOperationException("OAuthPrompt.SignOutUser(): not supported by the current adapter");
            }
        }

        private async Task<ResourceResponse[]> StoreOutgoingActivities(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            var messageActivities = activities
                .Where(a => a.Type == ActivityTypes.Message)
                .ToList();

            // If the bot is sending message activities to the user (as opposed to trace activities)
            if (messageActivities.Any())
            {
                var botResponse = await _previousResponseAccessor.GetAsync(turnContext, () => new List<Activity>());

                // Get only the activities sent in response to last user message
                botResponse = botResponse
                    .Concat(messageActivities)
                    .Where(a => a.ReplyToId == turnContext.Activity.Id)
                    .ToList();

                await _previousResponseAccessor.SetAsync(turnContext, botResponse);
            }

            return await next();
        }

        private bool IsSkillIntent(DispatchLuis.Intent dispatchIntent)
        {
            if (dispatchIntent.ToString().Equals(DispatchLuis.Intent.l_General.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
                dispatchIntent.ToString().Equals(DispatchLuis.Intent.q_Faq.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
                dispatchIntent.ToString().Equals(DispatchLuis.Intent.q_Chitchat.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
                dispatchIntent.ToString().Equals(DispatchLuis.Intent.None.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}