// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.SimpleEchoBot;
using Microsoft.Teams.Samples.TaskModule.Web.Helper;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Teams.Samples.TaskModule.Web.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else if (activity.Type == ActivityTypes.Invoke)
            {
                return HandleInvokeMessages(activity);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private HttpResponseMessage HandleInvokeMessages(Activity activity)
        {
            var activityValue = activity.Value.ToString();
            if (activity.Name == "task/fetch")
            {
                Models.BotFrameworkCardValue<string> action;
                try
                {
                    action  = JsonConvert.DeserializeObject<Models.TaskModuleActionData<string>>(activityValue).Data;
                }
                catch (Exception)
                {
                    action = JsonConvert.DeserializeObject<Models.BotFrameworkCardValue<string>>(activityValue);
                }

                Models.TaskInfo taskInfo = GetTaskInfo(action.Data);
                Models.TaskEnvelope taskEnvelope = new Models.TaskEnvelope
                {
                    Task = new Models.Task()
                    {
                        Type = Models.TaskType.Continue,
                        TaskInfo = taskInfo
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, taskEnvelope);

            }
            else if (activity.Name == "task/submit")
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = activity.CreateReply("Received = " + activity.Value.ToString());
                connector.Conversations.ReplyToActivity(reply);
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        // Helper function for building the TaskInfo object based on the incoming request
        private static Models.TaskInfo GetTaskInfo(string actionInfo)
        {
            Models.TaskInfo taskInfo = new Models.TaskInfo();
            switch (actionInfo)
            {
                case TaskModuleIds.YouTube:
                    taskInfo.Url = taskInfo.FallbackUrl = ApplicationSettings.BaseUrl + "/" + TaskModuleIds.YouTube;
                    SetTaskInfo(taskInfo, TaskModuleUIConstants.YouTube);
                    break;
                case TaskModuleIds.PowerApp:
                    taskInfo.Url = taskInfo.FallbackUrl = ApplicationSettings.BaseUrl + "/" + TaskModuleIds.PowerApp;
                    SetTaskInfo(taskInfo, TaskModuleUIConstants.PowerApp);
                    break;
                case TaskModuleIds.CustomForm:
                    taskInfo.Url = taskInfo.FallbackUrl = ApplicationSettings.BaseUrl + "/" + TaskModuleIds.CustomForm;
                    SetTaskInfo(taskInfo, TaskModuleUIConstants.CustomForm);
                    break;
                case TaskModuleIds.AdaptiveCard:
                    taskInfo.Card = AdaptiveCardHelper.GetAdaptiveCard();
                    SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
                    break;
                default:
                    break;
            }
            return taskInfo;
        }

        private static void SetTaskInfo(Models.TaskInfo taskInfo, UIConstants uIConstants)
        {
            taskInfo.Height = uIConstants.Height;
            taskInfo.Width = uIConstants.Width;
            taskInfo.Title = uIConstants.Title.ToString();
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.InstallationUpdate)
            {
                // Handle add/remove from contact lists
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }

            return null;
        }
    }
}
