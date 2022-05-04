using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Helpers
{
    public static class TaskModuleHelper
    {
        public static TaskModuleTaskInfo CreateTaskInfo(string Title, int Width, int Height, Attachment AdaptiveCard)
        {
            var taskInfo = new TaskModuleTaskInfo();
            taskInfo.Height = Height;
            taskInfo.Width = Width;
            taskInfo.Title = Title;
            taskInfo.Card = AdaptiveCard;
            return taskInfo;
        }
        public static TaskModuleTaskInfo CreateTaskInfo(string Title, int Width, int Height, string PageUrl)
        {
            var taskInfo = new TaskModuleTaskInfo();
            taskInfo.Height = Height;
            taskInfo.Width = Width;
            taskInfo.Title = Title;
            taskInfo.Url = taskInfo.FallbackUrl = PageUrl;
            return taskInfo;
        }
        public static TaskModuleResponse CreateResponse(TaskModuleTaskInfo taskInfo)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = taskInfo
                }
            };
        }

        public static TaskModuleResponse CreateResponse(string message = "")
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Type = "message",
                    Value = message // if empty, dialog will be closed automatically, otherwise it will display a message in the dialog and user has to close it manually
                },
            };
        }

    }
}
