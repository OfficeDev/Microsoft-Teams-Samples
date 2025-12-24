using Microsoft.Teams.Api.Activities.Invokes;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Common;
using System.Collections.Concurrent;
using System.Text.Json;
using JoinTeamByQR.Helpers;
using TaskModuleResponse = Microsoft.Teams.Api.TaskModules.Response;
using TaskModuleSize = Microsoft.Teams.Api.TaskModules.Size;
using ILogger = Microsoft.Teams.Common.Logging.ILogger;

namespace JoinTeamByQR.Controllers
{
    [TeamsController]
    public class Controller
    {
        private readonly ConcurrentDictionary<string, string> _tokenCache;
        private readonly ConfigOptions _config;

        public Controller(ConfigOptions config, ConcurrentDictionary<string, string> tokenCache)
        {
            _config = config;
            _tokenCache = tokenCache;
        }

        [TaskFetch]
        public TaskModuleResponse OnTaskFetch(
            [Context] Tasks.FetchActivity activity, 
            [Context] ILogger log)
        {
            var data = activity.Value?.Data as JsonElement?;
            string? dialogType = null;

            if (data != null && data.Value.TryGetProperty("opendialogtype", out var dialogTypeElement) && dialogTypeElement.ValueKind == JsonValueKind.String)
            {
                dialogType = dialogTypeElement.GetString();
            }

            log.Info($"[TASK_FETCH] Dialog type: {dialogType}");

            if (dialogType == "qr_generator")
            {
                var taskInfo = new Microsoft.Teams.Api.TaskModules.TaskInfo
                {
                    Title = "QR Code Generator",
                    Width = new Union<int, TaskModuleSize>(500),
                    Height = new Union<int, TaskModuleSize>(600),
                    Url = $"{_config.Teams.ApplicationBaseUrl}/generate.html"
                };

                return new TaskModuleResponse(new Microsoft.Teams.Api.TaskModules.ContinueTask(taskInfo));
            }

            return new TaskModuleResponse(new Microsoft.Teams.Api.TaskModules.MessageTask("Unknown dialog type"));
        }

        [TaskSubmit]
        public async Task<TaskModuleResponse?> OnTaskSubmit(
            [Context] Tasks.SubmitActivity activity, 
            [Context] IContext.Client client, 
            [Context] ILogger log)
        {
            var data = activity.Value?.Data as JsonElement?;
            
            if (data != null)
            {
                log.Info($"[TASK_SUBMIT] Received data: {data}");

                // Check if this is a team join request from QR scan
                if (data.Value.TryGetProperty("teamId", out var teamIdElement) && 
                    data.Value.TryGetProperty("userId", out var userIdElement))
                {
                    var teamId = teamIdElement.GetString();
                    var userId = userIdElement.GetString();

                    if (!string.IsNullOrEmpty(teamId) && !string.IsNullOrEmpty(userId))
                    {
                        // Get token and add user to team
                        if (_tokenCache.TryGetValue("Token", out var token) && !string.IsNullOrEmpty(token))
                        {
                            try
                            {
                                await JoinTeamHelper.AddUserToTeam(token, teamId, userId);
                                
                                // Send a message to the chat confirming the user was added
                                await client.Send("User added successfully to the team!");
                                
                                return new TaskModuleResponse(new Microsoft.Teams.Api.TaskModules.MessageTask("Successfully added to the team!"));
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Error adding user to team: {ex.Message}");
                                
                                // Send error message to chat
                                await client.Send($"Failed to add user to team: {ex.Message}");
                        
                                return new TaskModuleResponse(new Microsoft.Teams.Api.TaskModules.MessageTask($"Error joining team: {ex.Message}"));
                            }
                        }
                        else
                        {
                            await client.Send("Authentication token not found. Please sign in first using '/signin'.");
                            return new TaskModuleResponse(new Microsoft.Teams.Api.TaskModules.MessageTask("Authentication token not found. Please sign in first."));
                        }
                    }
                }
            }

            // Return null to close the dialog
            return null;
        }
    }
}
