using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Teams.Apps.VirtualAssistant.Extension
{
    public static class InvokeResponseExtensions
    {
        // Converts "InvokeResponse" sent by SkillHttpClinet to "MessagingExtensionActionResponse"
        public static MessagingExtensionActionResponse GetMessagingExtensionActionResponse(this InvokeResponse invokeResponse)
        {
            if (invokeResponse.Body != null)
            {
                var response = JsonConvert.DeserializeObject<MessagingExtensionActionResponse>(invokeResponse.Body.ToString());

                if (response.Task != null)
                {
                    response.Task = GetTask(invokeResponse.Body);
                }

                return response;
            }

            return null;
        }

        // Converts "InvokeResponse" sent by SkillHttpClinet to "MessagingExtensionResponse"
        public static MessagingExtensionResponse GetMessagingExtensionResponse(this InvokeResponse invokeResponse)
        {
            if (invokeResponse.Body != null)
            {
                return JsonConvert.DeserializeObject<MessagingExtensionResponse>(invokeResponse.Body.ToString());
            }

            return null;
        }

        // Converts "InvokeResponse" sent by SkillHttpClinet to "TaskModuleResponse"
        public static TaskModuleResponse GetTaskModuleRespose(this InvokeResponse invokeResponse)
        {
            if (invokeResponse.Body != null)
            {
                return new TaskModuleResponse()
                {
                    Task = GetTask(invokeResponse.Body),
                };
            }

            return null;
        }

        private static TaskModuleResponseBase GetTask(object invokeResponseBody)
        {
            JObject resposeBody = (JObject)JToken.FromObject(invokeResponseBody);
            var task = resposeBody.GetValue("task");
            var taskType = task.SelectToken("type").ToString();

            return taskType switch
            {
                "continue" => new TaskModuleContinueResponse()
                {
                    Type = taskType,
                    Value = task.SelectToken("value").ToObject<TaskModuleTaskInfo>(),
                },
                "message" => new TaskModuleMessageResponse()
                {
                    Type = taskType,
                    Value = task.SelectToken("value").ToString(),
                },
                _ => null,
            };
        }
    }
}
