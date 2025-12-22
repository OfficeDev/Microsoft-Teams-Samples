using BotDailyTaskReminder.Models;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api.Activities.Invokes;
using Microsoft.Teams.Api.TaskModules;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using Microsoft.Teams.Common;
using System.Collections.Concurrent;
using System.Text.Json;


namespace BotDailyTaskReminder.Controllers
{
    /// </summary>
    [TeamsController]
    public class Controller
    {
        private readonly ConcurrentDictionary<string, Microsoft.Teams.Api.ConversationReference> _conversationReferences;
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;
        private readonly string _applicationBaseUrl;
        private IConfiguration _configuration;

        public Controller(
            ConcurrentDictionary<string, Microsoft.Teams.Api.ConversationReference> conversationReferences,
            ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails,
            IConfiguration configuration)
        {
            _conversationReferences = conversationReferences;
            _taskDetails = taskDetails;
            _configuration = configuration;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        [Message]
        public async System.Threading.Tasks.Task OnMessage(
            [Context] Microsoft.Teams.Api.Activities.MessageActivity activity, 
            [Context] IContext.Client client, 
            [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Message received");
            
            // Store conversation reference for proactive messaging
            StoreConversationReference(activity);

            var text = activity.Text?.ToLower().Trim() ?? string.Empty;

            if (text == "create-reminder")
            {
                // Send adaptive card with button to schedule task
                var card = CreateTaskSchedulerCard();
                await client.Send(card);
            }
            else
            {
                await client.Typing();
                await client.Send($"You said '{activity.Text}'. Use command 'create-reminder' to schedule a recurring task.");
            }
        }

        [Conversation.MembersAdded]
        public async System.Threading.Tasks.Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            var welcomeText = "Hello and welcome! Use the command 'create-reminder' to schedule a recurring task and receive reminders.";
            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient.Id)
                {
                    await context.Send(welcomeText);
                }
            }
        }

        [TaskFetch]
        public Microsoft.Teams.Api.TaskModules.Response OnTaskFetch([Context] Tasks.FetchActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            var data = activity.Value?.Data as JsonElement?;
            if (data == null)
            {
                log.Info("[TASK_FETCH] No data found in the activity value");
                return new Microsoft.Teams.Api.TaskModules.Response(
                    new Microsoft.Teams.Api.TaskModules.MessageTask("No data found in the activity value"));
            }

            var dialogType = data.Value.TryGetProperty("opendialogtype", out var dialogTypeElement) && dialogTypeElement.ValueKind == JsonValueKind.String
                ? dialogTypeElement.GetString()
                : null;

            log.Info($"[TASK_FETCH] Dialog type: {dialogType}");

            return dialogType switch
            {
                "webpage_dialog" => CreateWebpageDialog(_configuration, log),
                _ => new Microsoft.Teams.Api.TaskModules.Response(
                    new Microsoft.Teams.Api.TaskModules.MessageTask("Unknown dialog type"))
            };
        }

        [TaskSubmit]
        public async System.Threading.Tasks.Task<Microsoft.Teams.Api.TaskModules.Response> OnTaskSubmit(
            [Context] Tasks.SubmitActivity activity, 
            [Context] IContext.Client client, 
            [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info($"[TASK_SUBMIT] Activity.Value: {System.Text.Json.JsonSerializer.Serialize(activity.Value)}");
            
            var data = activity.Value?.Data as JsonElement?;
            
            if (data == null)
            {
                log.Info("[TASK_SUBMIT] No data received - activity.Value.Data is null");
                return new Microsoft.Teams.Api.TaskModules.Response(
                    new Microsoft.Teams.Api.TaskModules.MessageTask("No data received"));
            }

            log.Info($"[TASK_SUBMIT] Data: {data.Value.GetRawText()}");

            var title = data.Value.TryGetProperty("taskName", out var titleElement) ? titleElement.GetString() : null;
            var description = data.Value.TryGetProperty("taskDescription", out var descElement) ? descElement.GetString() : null;
            var reminderTimeStr = data.Value.TryGetProperty("reminderTime", out var timeElement) ? timeElement.GetString() : null;
            
            log.Info($"[TASK_SUBMIT] Parsed values - Title: {title}, Description: {description}, Time: {reminderTimeStr}");

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(reminderTimeStr))
            {
                return new Microsoft.Teams.Api.TaskModules.Response(
                    new Microsoft.Teams.Api.TaskModules.MessageTask("Please fill in all required fields"));
            }

            log.Info($"[TASK_SUBMIT] Task created: {title}, Time: {reminderTimeStr}");

            // Parse the reminder datetime (comes as ISO string from HTML)
            if (!DateTimeOffset.TryParse(reminderTimeStr, out var nextReminder))
            {
                return new Microsoft.Teams.Api.TaskModules.Response(
                    new Microsoft.Teams.Api.TaskModules.MessageTask("Invalid time format"));
            }

            // Get selected days from the form
            DayOfWeek[] selectedDays;
            if (data.Value.TryGetProperty("selectedDays", out var daysElement) && daysElement.ValueKind == JsonValueKind.Array)
            {
                var daysList = new List<DayOfWeek>();
                foreach (var dayValue in daysElement.EnumerateArray())
                {
                    if (int.TryParse(dayValue.GetString(), out var dayNum))
                    {
                        daysList.Add((DayOfWeek)dayNum);
                    }
                }
                selectedDays = daysList.Count > 0 ? daysList.ToArray() : new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
            }
            else
            {
                // Default to all days if none selected
                selectedDays = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
            }

            // Store the task details
            var userId = activity.From?.Id ?? "default";
            var taskDetail = new SaveTaskDetail
            {
                Title = title,
                Description = description ?? string.Empty,
                DateTime = nextReminder,
                SelectedDays = selectedDays
            };

            _taskDetails.AddOrUpdate(userId, 
                new List<SaveTaskDetail> { taskDetail }, 
                (key, existingList) =>
                {
                    existingList.Add(taskDetail);
                    return existingList;
                });

            // Build frequency description for confirmation message
            string frequencyDesc = selectedDays.Length == 7 ? "daily" : 
                                   selectedDays.Length == 1 ? $"every {selectedDays[0]}" :
                                   $"on {string.Join(", ", selectedDays)}";

            // Send confirmation message to chat
            var confirmationMessage = $"Task submitted successfully. You will get a recurring reminder for the task at a scheduled time.";

            await client.Send(confirmationMessage);

            return new Microsoft.Teams.Api.TaskModules.Response(
                new Microsoft.Teams.Api.TaskModules.MessageTask("Task scheduled successfully!"));
        }

        private static Microsoft.Teams.Api.TaskModules.Response CreateWebpageDialog(IConfiguration configuration, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            var botEndpoint = configuration["ApplicationBaseUrl"];
            if (string.IsNullOrEmpty(botEndpoint))
            {
                botEndpoint = "http://localhost:3978"; // Fallback for local development
            }
            else
            {
                log.Info($"Using BotEndpoint: {botEndpoint}/tabs/ScheduleTask");
            }

            var taskInfo = new TaskInfo
            {
                Title = "Webpage Dialog",
                Width = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(1000),
                Height = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(800),
                Url = $"{botEndpoint}/tabs/ScheduleTask"
            };

            return new Microsoft.Teams.Api.TaskModules.Response(
                new Microsoft.Teams.Api.TaskModules.ContinueTask(taskInfo));
        }
        

        private void StoreConversationReference(Microsoft.Teams.Api.Activities.MessageActivity activity)
        {
            var userId = activity.From?.Id;
            if (!string.IsNullOrEmpty(userId) && activity.Conversation != null)
            {
                var reference = new Microsoft.Teams.Api.ConversationReference
                {
                    Bot = activity.Recipient,
                    Conversation = activity.Conversation,
                    ChannelId = activity.ChannelId,
                    ServiceUrl = activity.ServiceUrl
                };

                _conversationReferences.AddOrUpdate(userId, reference, (key, oldValue) => reference);
            }
        }

        private static Microsoft.Teams.Cards.AdaptiveCard CreateTaskSchedulerCard()
        {
            var card = new Microsoft.Teams.Cards.AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Please click here to schedule a recurring task reminde")
                    {
                        Size = TextSize.Large,
                        Weight = TextWeight.Bolder
                    }
                },
                Actions = new List<Microsoft.Teams.Cards.Action>
                {
                    new TaskFetchAction(new Dictionary<string, object?> { { "opendialogtype", "webpage_dialog" } })
                    {
                        Title = "Schedule task"
                    }
                }
            };

            return card;
        }
    }
}