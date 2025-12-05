using System.Collections.Concurrent;
using BotDailyTaskReminder.Models;
using Microsoft.AspNetCore.Mvc;

namespace BotDailyTaskReminder.Controllers
{
    /// <summary>
    /// Controller to handle task reminder API calls.
    /// This endpoint is called by the Quartz scheduler.
    /// The actual reminder sending is handled through event handlers in the main bot controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;
        private readonly ConcurrentDictionary<string, string> _conversationReferences;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskController"/> class.
        /// </summary>
        /// <param name="taskDetails">The task details storage.</param>
        /// <param name="conversationReferences">The conversation references storage.</param>
        public TaskController(
            ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails,
            ConcurrentDictionary<string, string> conversationReferences)
        {
            _taskDetails = taskDetails;
            _conversationReferences = conversationReferences;
        }

        /// <summary>
        /// This endpoint is called by the Quartz scheduler to trigger task reminder checks.
        /// The actual proactive messaging needs to be implemented using the Teams SDK's proactive messaging capabilities.
        /// </summary>
        [HttpGet]
        public System.Threading.Tasks.Task<IActionResult> GetTaskReminder()
        {
            try
            {
                // Log the trigger
                Console.WriteLine($"[TASK_REMINDER] Reminder check triggered at {DateTime.UtcNow}");
                
                // Check if there are tasks to process
                var hasTasks = _taskDetails.TryGetValue("taskDetails", out var tasks);
                var conversationCount = _conversationReferences.Count;
                
                if (hasTasks && tasks != null)
                {
                    var currentDateTime = DateTime.Now;
                    Console.WriteLine($"[TASK_REMINDER] Checking {tasks.Count} tasks against current time: {currentDateTime}");
                    
                    // Check which tasks should trigger
                    foreach (var task in tasks)
                    {
                        if (task.DateTime.Hour == currentDateTime.Hour &&
                            task.DateTime.Minute == currentDateTime.Minute)
                        {
                            foreach (var day in task.SelectedDays)
                            {
                                if ((int)day == (int)currentDateTime.DayOfWeek ||
                                    ((int)day == 7 && (int)currentDateTime.DayOfWeek == 0))
                                {
                                    Console.WriteLine($"[TASK_REMINDER] Task '{task.Title}' should be sent now!");
                                }
                            }
                        }
                    }
                }
                
                return System.Threading.Tasks.Task.FromResult<IActionResult>(Ok(new { 
                    status = "success",
                    message = "Task reminder check completed",
                    timestamp = DateTime.UtcNow,
                    taskCount = hasTasks && tasks != null ? tasks.Count : 0,
                    conversationCount = conversationCount
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TASK_REMINDER] Error: {ex.Message}");
                return System.Threading.Tasks.Task.FromResult<IActionResult>(StatusCode(500, new { 
                    error = "Failed to process task reminders", 
                    details = ex.Message 
                }));
            }
        }
    }
}

