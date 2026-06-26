// <copyright file="WorkItemController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using ReleaseManagement.Helpers;
    using ReleaseManagement.Models;
    using ReleaseManagement.Models.Configuration;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class WorkItemController : ControllerBase
    {
        private readonly GraphHelper graphHelper;
        private readonly DevOpsHelper devOpsHelper;
        private readonly ConcurrentDictionary<string, ReleaseManagementTask> taskDetails;

        public WorkItemController(IOptions<AzureSettings> azureSettings, ConcurrentDictionary<string, ReleaseManagementTask> taskDetails)
        {
            this.graphHelper = new(azureSettings);
            this.devOpsHelper = new(azureSettings);
            this.taskDetails = taskDetails;
        }

        [HttpPost]
        public async Task<IActionResult> OnWebHookTriggerAsync(WorkItem workItem)
        {
            // Maps incoming workitem payload to release management model.
            var releaseManagementTask = await devOpsHelper.MapToReleaseManagementTask(workItem);

            taskDetails.AddOrUpdate(Constant.TaskDetails, releaseManagementTask, (_, _) => releaseManagementTask);

            if (releaseManagementTask.GroupChatMembers.Any())
            {
                var groupChatId = await graphHelper.CreateGroupChatAsync(releaseManagementTask.GroupChatMembers, releaseManagementTask.TaskTitle);
                await graphHelper.AppinstallationforGroupAsync(groupChatId);
                return Ok();
            }

            return BadRequest();
        }
    }
}
