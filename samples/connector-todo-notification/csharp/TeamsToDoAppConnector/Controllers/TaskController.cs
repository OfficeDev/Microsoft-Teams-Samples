using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TeamsToDoAppConnector.Models;
using TeamsToDoAppConnector.Models.Configuration;
using TeamsToDoAppConnector.Repository;
using TeamsToDoAppConnector.Utils;

namespace TeamsToDoAppConnector.Controllers
{
    /// <summary>
    /// Represents the controller which handles tasks create, update. 
    /// This class also sends push notification to the channels.
    /// </summary>
    public class TaskController : Controller
    {

        /// <summary>
        /// Stores the AppSettings configuration values.
        /// </summary>
        private readonly IOptions<AppSettings> appSettings;
        public TaskController(IOptions<AppSettings> app)
        {
            appSettings = app;
        }

        [Route("task/index")]
        [HttpGet]
        public ActionResult Index()
        {
            return View(TaskRepository.Tasks);
        }

        [Route("task/create")]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [Route("task/create")]
        [HttpPost]
        public async Task<ActionResult> Create(TaskItem item)
        {
            item.Guid = Guid.NewGuid().ToString();
            TaskRepository.Tasks.Add(item);

            // Loop through subscriptions and notify each channel that task is created.
            foreach (var sub in SubscriptionRepository.Subscriptions)
            {
                await TaskHelper.PostTaskNotification(sub.WebHookUri, item, "Created",appSettings.Value.BaseUrl);
            }

            return RedirectToAction("Detail", new { id = item.Guid });
        }

        [Route("task/detail/{id}")]
        [HttpGet]
        public ActionResult Detail(string id)
        {
            return View(TaskRepository.Tasks.FirstOrDefault(i => i.Guid == id));
        }

        [Route("task/update")]
        [HttpPost]
        public async Task Update([FromBody]Request request, string id)
        {
            //Task<HttpResponseMessage>
            var task = TaskRepository.Tasks.First(t => t.Guid == id);
            task.Title = request.Title;

            string json = TaskHelper.GetConnectorCardJson(task, "Updated", appSettings.Value.BaseUrl);

            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.Headers.Add("CARD-ACTION-STATUS", "The task is uppdate.");
            Response.Headers.Add("CARD-UPDATE-IN-BODY", "true");
            Response.WriteAsync(json);
            Response.StatusCode = StatusCodes.Status200OK;
            
            // Send Task updated notification to all Subscriptions.
            foreach (var sub in SubscriptionRepository.Subscriptions.Where(s => s.EventType == EventType.Update))
            {
                await TaskHelper.PostTaskNotification(sub.WebHookUri, task, "Updated", appSettings.Value.BaseUrl);
            }
        }
    }
}