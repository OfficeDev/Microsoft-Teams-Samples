// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using DetailsTab.Controllers;
using DetailsTab.Models;
using System.Net.Http;

namespace DetailsTab.Bots
{
    public class DetailsTabBot : TeamsActivityHandler
    {
        private readonly IConfiguration _configuration;
        public DetailsTabBot(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            HomeController.serviceUrl = turnContext.Activity.ServiceUrl;
            HomeController.conversationId = turnContext.Activity.Conversation.Id;
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            JObject value = JObject.Parse(turnContext.Activity.Value.ToString());
            string Id = value.GetValue("Choice")?.ToString();
            string answer = value.GetValue("Feedback")?.ToString();
            if(Id == null || answer == null)
            {
                return;
            }
            string userName = turnContext.Activity.From.Name;
            TaskInfo info = HomeController.TaskList.taskInfoList.Find(x => x.id == Id);
            if (info.PersonAnswered == null)
            {
                info.PersonAnswered = new Dictionary<string, List<string>> { { answer, new List<string> { userName } } };
            }
            else
            {
                if (info.PersonAnswered.ContainsKey(answer))
                {
                    info.PersonAnswered[answer].Add(userName);
                }
                else
                {
                    info.PersonAnswered.Add(answer, new List<string> { userName });
                }
            }
            int option1Answerd = info.PersonAnswered.ContainsKey(info.option1) ? info.PersonAnswered[info.option1].Count : 0;
            int option2Answerd = info.PersonAnswered.ContainsKey(info.option2) ? info.PersonAnswered[info.option2].Count : 0;
            int total = option1Answerd + option2Answerd;

            int percentOption1 = total == 0 ? 0 :(option1Answerd * 100) / total;
            int percentOption2 = total == 0 ? 0 : 100 - percentOption1;
            Attachment att = HomeController.AgendaAdaptiveList(info, "Result.json", percentOption1, percentOption2);
            try
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(att), cancellationToken);
            }
            catch (Exception e)
            {

            }
        }
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            JObject value = JObject.Parse(turnContext.Activity.Value.ToString());
            JObject data = JObject.Parse(value.GetValue("data").ToString());
            string Id = data.GetValue("Id").ToString();
            
            var model = new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = _configuration["BaseUrl"] + "/Result?id=" + Id,
                        FallbackUrl = _configuration["BaseUrl"] + "/Result?id=" + Id,
                        Height = 300,
                        Width = 350,
                        Title = "Details",
                    },
                },
            };
            return model;
        }
    }
}