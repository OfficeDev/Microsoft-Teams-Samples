using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
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
    [Authorize]
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

            // Only notify webhooks registered by the current authenticated connector context.
            var ownerObjectId = User.GetObjectId();
            var ownerTenantId = User.GetTenantId();
            foreach (var sub in SubscriptionRepository.Subscriptions
                .Where(s => s.OwnerObjectId == ownerObjectId && s.OwnerTenantId == ownerTenantId))
            {
                await TaskHelper.PostTaskNotification(sub.WebHookUri, item, "Created", appSettings.Value.BaseUrl, appSettings.Value.AllowedWebhookHostSuffixes);
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
            var task = TaskRepository.Tasks.First(t => t.Guid == id);
            task.Title = request.Title;

            string json = TaskHelper.GetConnectorCardJson(task, "Updated", appSettings.Value.BaseUrl);

            // Only notify webhooks registered by the current authenticated connector context.
            var ownerObjectId = User.GetObjectId();
            var ownerTenantId = User.GetTenantId();
            foreach (var sub in SubscriptionRepository.Subscriptions
                .Where(s => s.EventType == EventType.Update
                    && s.OwnerObjectId == ownerObjectId
                    && s.OwnerTenantId == ownerTenantId))
            {
                await TaskHelper.PostTaskNotification(sub.WebHookUri, task, "Updated", appSettings.Value.BaseUrl, appSettings.Value.AllowedWebhookHostSuffixes);
            }

            // Write the response after notifications so setting headers/status is valid.
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.Headers["CARD-ACTION-STATUS"] = "The task is updated.";
            Response.Headers["CARD-UPDATE-IN-BODY"] = "true";
            Response.StatusCode = StatusCodes.Status200OK;
            await Response.WriteAsync(json);
        }
    }
}