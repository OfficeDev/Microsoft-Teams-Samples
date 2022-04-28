// <copyright file="WorkItemController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using ReleaseManagement.Models;
    using ReleaseManagement.Helpers;
    using ReleaseManagement.Models.Configuration;
    using Microsoft.Extensions.Options;
    using System.Linq;
    using System.Collections.Concurrent;

    [Route("api/[controller]")]
    [ApiController]
    public class WorkItemController : ControllerBase
    {
        private readonly GraphHelper _helper;
        private readonly IOptions<AzureSettings> azureSettings;
        private readonly ConcurrentDictionary<string, ReleaseManagementTask> taskDetails;

        public WorkItemController(IOptions<AzureSettings> azureSettings, ConcurrentDictionary<string, ReleaseManagementTask> taskDetails)
        {
            this.azureSettings = azureSettings;
            this._helper = new(azureSettings);
            this.taskDetails = taskDetails;
        }

        [HttpPost]
        public async Task<IActionResult> OnWebHookTriggerAsync(WorkItem workItem)
        {
            // Maps incoming workitem payload to release management model.
            var releaseManagementTask = DevOpsHelper.MapToReleaseManagementTask(workItem);

            taskDetails.AddOrUpdate(Constant.TaskDetails, releaseManagementTask, (key, newValue) => releaseManagementTask);

            if (releaseManagementTask.GroupChatMembers.Count() > 1)
            {
                var groupChat = await _helper.CreateGroupChatAsync(releaseManagementTask.GroupChatMembers, releaseManagementTask.TaskTitle);
                await _helper.AppinstallationforGroupAsync(groupChat.Id);
            }
            return this.Ok();
        }
    }
}
