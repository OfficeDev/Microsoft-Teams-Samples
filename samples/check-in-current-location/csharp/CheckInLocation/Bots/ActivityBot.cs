// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using CheckInLocation.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace CheckInLocation.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
       // private readonly string _connectionName;
        private readonly string _applicationBaseUrl;

        public ActivityBot(IConfiguration configuration)
        {
           // _connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskModule()), cancellationToken);
        }

        /// <summary>
        /// Invoked when bot (like a user) are added to the conversation.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome! With this sample your bot can generate QR code for App and install the app in team by scanning QR code. Please add this bot to team and type hey."), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handle task module is fetch.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name = "taskModuleRequest" >The task module invoke request value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var buttonType = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.Id;

            var taskModuleResponse = new TaskModuleResponse();
            if (buttonType == "checkin")
            {
                taskModuleResponse.Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = _applicationBaseUrl + "/" + "CheckIn",
                        Height = 350,
                        Width = 350,
                        Title = "Check in details",
                    },
                };
            }
            return Task.FromResult(taskModuleResponse);
        }

        /// <summary>
        /// Handle task module is submit.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name = "taskModuleRequest" >The task module invoke request value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var appInfo = taskModuleRequest.Data;
            string user = turnContext.Activity.From.Name;
            string time = turnContext.Activity.LocalTimestamp.ToString();
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForUserLocation(time, user)), cancellationToken);
            return null;
        }

        /// <summary>
        /// Sample Adaptive card for Meeting Start event.
        /// </summary>
        private Attachment GetAdaptiveCardForTaskModule()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Please click on check in",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Check in",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                            Id="checkin"
                        },
                    }
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Sample Adaptive card for Meeting Start event.
        /// </summary>
        private Attachment GetAdaptiveCardForUserLocation(string time,string user)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = $"{user}",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = $"Check in time:{time}",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = $"Check in at:{time}",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    }
                }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}