using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using TeamsToDoAppConnector.Models;
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
                await TaskHelper.PostTaskNotification(sub.WebHookUri, item, "Created");
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
        public async Task Update([System.Web.Http.FromBody]Request request, string id)
        {
            var task = TaskRepository.Tasks.First(t => t.Guid == id);
            task.Title = request.Title;

            string json = TaskHelper.GetConnectorCardJson(task, "Updated");

            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.Headers.Add("CARD-ACTION-STATUS", "The task is uppdate.");
            Response.Headers.Add("CARD-UPDATE-IN-BODY", "true");
            Response.Write(json);
            Response.End();

            // Send Task updated notification to all Subscriptions.
            foreach (var sub in SubscriptionRepository.Subscriptions.Where(s => s.EventType == EventType.Update))
            {
                await TaskHelper.PostTaskNotification(sub.WebHookUri, task, "Updated");
            }
        }
    }
}