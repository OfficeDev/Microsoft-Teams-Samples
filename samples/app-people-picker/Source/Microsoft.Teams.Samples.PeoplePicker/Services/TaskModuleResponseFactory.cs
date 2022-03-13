// <copyright file="TaskModuleResponseFactory.cs" company="Microsoft Corp.">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker.Services
{
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Implementation rendering Task Modules.
    /// </summary>
    public class TaskModuleResponseFactory : ITaskModuleResponseFactory
    {
        /// <inheritdoc/>
        public TaskModuleContinueResponse CreateTaskModuleContinueResponse(Attachment card)
        {
            var taskInfo = new TaskModuleTaskInfo();
            taskInfo.Width = "medium";
            taskInfo.Height = "large";
            taskInfo.Card = card;
            return new TaskModuleContinueResponse
            {
                Value = taskInfo,
            };
        }

        /// <inheritdoc/>
        public TaskModuleResponse CreateTaskModuleMessageResponse(string message)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = message,
                },
            };
        }

        /// <inheritdoc/>
        public TaskModuleResponse CreateTaskModuleCardResponse(Attachment card)
        {
            var taskInfo = new TaskModuleTaskInfo();
            taskInfo.Width = "medium";
            taskInfo.Height = "large";
            taskInfo.Card = card;
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = taskInfo,
                },
            };
        }
    }
}
