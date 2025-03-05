// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.BotBuilderSamples.Models;

namespace Microsoft.BotBuilderSamples.Bots
{
    /// <summary>
    /// A bot that handles Teams task modules.
    /// </summary>
    public class TeamsTaskModuleBot : TeamsActivityHandler
    {
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsTaskModuleBot"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public TeamsTaskModuleBot(IConfiguration config)
        {
            _baseUrl = config["BaseUrl"].TrimEnd('/') + "/";
        }

        /// <summary>
        /// Handles incoming message activities.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Attachment(new[] { GetTaskModuleHeroCardOptions(), GetTaskModuleAdaptiveCardOptions() });
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Handles task module fetch requests.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="taskModuleRequest">The task module request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task module response.</returns>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJObject = JObject.FromObject(taskModuleRequest.Data);
            var value = asJObject.ToObject<CardTaskFetchValue<string>>()?.Data;

            var taskInfo = new TaskModuleTaskInfo();
            switch (value)
            {
                case TaskModuleIds.YouTube:
                    taskInfo.Url = taskInfo.FallbackUrl = _baseUrl + TaskModuleIds.YouTube;
                    SetTaskInfo(taskInfo, TaskModuleUIConstants.YouTube);
                    break;
                case TaskModuleIds.CustomForm:
                    taskInfo.Url = taskInfo.FallbackUrl = _baseUrl + TaskModuleIds.CustomForm;
                    SetTaskInfo(taskInfo, TaskModuleUIConstants.CustomForm);
                    break;
                case TaskModuleIds.AdaptiveCard:
                    taskInfo.Card = CreateAdaptiveCardAttachment();
                    SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
                    break;
            }

            return Task.FromResult(taskInfo.ToTaskModuleResponse());
        }

        /// <summary>
        /// Handles task module submit requests.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="taskModuleRequest">The task module request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task module response.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsTaskModuleSubmitAsync Value: " + JsonConvert.SerializeObject(taskModuleRequest));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            return TaskModuleResponseFactory.CreateResponse("Thanks!");
        }

        /// <summary>
        /// Sets the task module task info.
        /// </summary>
        /// <param name="taskInfo">The task info.</param>
        /// <param name="uiConstants">The UI constants.</param>
        private static void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uiConstants)
        {
            taskInfo.Height = uiConstants.Height;
            taskInfo.Width = uiConstants.Width;
            taskInfo.Title = uiConstants.Title;
        }

        /// <summary>
        /// Gets the task module hero card options.
        /// </summary>
        /// <returns>An attachment containing the hero card options.</returns>
        private static Attachment GetTaskModuleHeroCardOptions()
        {
            return new HeroCard()
            {
                Title = "Dialogs (referred as task modules in TeamsJS v1.x) Invocation from Hero Card",
                Buttons = new[] { TaskModuleUIConstants.AdaptiveCard, TaskModuleUIConstants.CustomForm, TaskModuleUIConstants.YouTube }
                            .Select(cardType => new TaskModuleAction(cardType.ButtonTitle, new CardTaskFetchValue<string>() { Data = cardType.Id }))
                            .ToList<CardAction>(),
            }.ToAttachment();
        }

        /// <summary>
        /// Gets the task module adaptive card options.
        /// </summary>
        /// <returns>An attachment containing the adaptive card options.</returns>
        private static Attachment GetTaskModuleAdaptiveCardOptions()
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock(){ Text="Dialogs (referred as task modules in TeamsJS v1.x) Invocation from Adaptive Card", Weight=AdaptiveTextWeight.Bolder, Size=AdaptiveTextSize.Large}
                        },
                Actions = new[] { TaskModuleUIConstants.AdaptiveCard, TaskModuleUIConstants.CustomForm, TaskModuleUIConstants.YouTube }
                            .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = cardType.Id } })
                            .ToList<AdaptiveAction>(),
            };

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        /// <summary>
        /// Creates an adaptive card attachment.
        /// </summary>
        /// <returns>An attachment containing the adaptive card.</returns>
        private static Attachment CreateAdaptiveCardAttachment()
        {
            string[] paths = { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
        }
    }
}
