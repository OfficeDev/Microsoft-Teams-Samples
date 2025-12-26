// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using MessagingExtensionReminder.Models;
using Microsoft.AspNetCore.Mvc;

namespace MessagingExtensionReminder.Controllers
{
    /// Controller to handle task reminder API calls.
    [Route("api/task")]
    [ApiController]
    public class TaskReminderController : ControllerBase
    {
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;
        private readonly ConcurrentDictionary<string, Microsoft.Teams.Api.ConversationReference> _conversationReferences;
        public TaskReminderController(
            ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails,
            ConcurrentDictionary<string, Microsoft.Teams.Api.ConversationReference> conversationReferences)
        {
            _taskDetails = taskDetails;
            _conversationReferences = conversationReferences;
        }

        /// This endpoint is called by the Quartz scheduler to trigger task reminder checks.
        [HttpGet]
        public Task<IActionResult> GetTaskReminder()
        {
            try
            {
                var hasTasks = _taskDetails.TryGetValue("taskDetails", out var tasks);
                var conversationCount = _conversationReferences.Count;
                return Task.FromResult<IActionResult>(Ok(new
                {
                    status = "success",
                    message = "Task reminder check completed",
                    timestamp = DateTime.UtcNow,
                    taskCount = hasTasks && tasks != null ? tasks.Count : 0,
                    conversationCount = conversationCount
                }));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IActionResult>(StatusCode(500, new
                {
                    error = "Failed to process task reminders",
                    details = ex.Message
                }));
            }
        }
    }
}
