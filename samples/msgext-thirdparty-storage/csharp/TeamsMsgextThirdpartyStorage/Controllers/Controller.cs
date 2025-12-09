using Microsoft.Teams.Api.MessageExtensions;
using Microsoft.Teams.Api.TaskModules;
using Microsoft.Teams.Apps.Annotations;
using System.Text.Json;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Common;
using MessageExtensionResponse = Microsoft.Teams.Api.MessageExtensions.Response;
using TaskModuleSize = Microsoft.Teams.Api.TaskModules.Size;


namespace TeamsMsgextThirdpartyStorage.Controllers
{
    [TeamsController]
    public class Controller
    {
        private readonly ConfigOptions _config;

        public Controller(IConfiguration configuration)
        {
            _config = configuration.Get<ConfigOptions>() ?? throw new NullReferenceException("ConfigOptions");
        }

        /// <summary>
        /// Handles messaging extension fetch task - displays the custom form for file upload
        /// </summary>
        [Invoke("composeExtension/fetchTask")]
        public async Task<ActionResponse> OnFetchTask(
            [Context] Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.FetchTaskActivity activity,
            [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("FetchTask invoked for third-party storage");

            try
            {
                var commandContext = activity.Value?.CommandContext;
                var commandId = activity.Value?.CommandId;
                
                log.Info($"Command Context: {commandContext}");
                log.Info($"Command ID: {commandId}");

                // Always return the task module for file upload
                // The thirdParty context will be set when files are dragged and dropped from third-party storage
                return new ActionResponse
                {
                    Task = new ContinueTask(new TaskInfo
                    {
                        Title = "Third-Party Storage - File Upload",
                        Height = new Union<int, TaskModuleSize>(530),
                        Width = new Union<int, TaskModuleSize>(700),
                        Url = $"{_config.Teams.BaseUrl}/CustomForm"
                    })
                };
            }
            catch (Exception ex)
            {
                log.Error($"Error in OnFetchTask: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Handles messaging extension submit action - processes the uploaded files
        /// </summary>
        [Invoke("composeExtension/submitAction")]
        public async Task<MessageExtensionResponse> OnSubmitAction(
            [Context] Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.SubmitActionActivity activity,
            [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("SubmitAction invoked for third-party storage");

            try
            {
                var commandId = activity.Value?.CommandId;

                if (commandId == "createWithPreview")
                {
                    return CreateWebViewResponse(activity, log);
                }

                throw new NotSupportedException($"Command '{commandId}' is not supported.");
            }
            catch (Exception ex)
            {
                log.Error($"Error in OnSubmitAction: {ex.Message}", ex);
                return new MessageExtensionResponse
                {
                    ComposeExtension = new Result
                    {
                        Type = ResultType.Message,
                        Text = "An error occurred while processing your request. Please try again later."
                    }
                };
            }
        }

        /// <summary>
        /// Creates an adaptive card response with file details
        /// </summary>
        private MessageExtensionResponse CreateWebViewResponse(
            Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.SubmitActionActivity activity,
            Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                // Parse the uploaded data from activity.Value.Data
                var data = activity.Value?.Data;
                log.Info($"Received data: {data?.ToString() ?? "null"}");
                log.Info($"Data type: {data?.GetType().FullName ?? "null"}");
                
                List<FileInfo> uploadedFiles = null;
                
                // Check if data is already a JSON element or needs to be deserialized
                if (data is JsonElement jsonElement)
                {
                    log.Info($"Data is JsonElement, ValueKind: {jsonElement.ValueKind}");
                    uploadedFiles = JsonSerializer.Deserialize<List<FileInfo>>(jsonElement.GetRawText());
                }
                else if (data is string jsonString)
                {
                    log.Info("Data is string");
                    uploadedFiles = JsonSerializer.Deserialize<List<FileInfo>>(jsonString);
                }
                else
                {
                    log.Info("Data is another type, converting to string");
                    uploadedFiles = JsonSerializer.Deserialize<List<FileInfo>>(data?.ToString() ?? "[]");
                }
                
                log.Info($"Deserialized {uploadedFiles?.Count ?? 0} files");

                var cardElements = new List<object>();

                foreach (var file in uploadedFiles ?? new List<FileInfo>())
                {
                    string name = file.Name ?? "Unknown";
                    string type = file.Type ?? string.Empty;
                    
                    log.Info($"Processing file: Name={name}, Type={type}, Size={file.Size}");
                    
                    // Determine the icon based on file extension or MIME type
                    string fileTypeIconUrl = GetFileIconUrl(name, type);
                    log.Info($"Selected icon URL: {fileTypeIconUrl}");

                    cardElements.Add(new
                    {
                        type = "ColumnSet",
                        columns = new object[]
                        {
                            new
                            {
                                type = "Column",
                                width = "auto",
                                items = new object[]
                                {
                                    new
                                    {
                                        type = "Image",
                                        url = fileTypeIconUrl,
                                        size = "Small"
                                    }
                                }
                            },
                            new
                            {
                                type = "Column",
                                width = "stretch",
                                items = new object[]
                                {
                                    new
                                    {
                                        type = "TextBlock",
                                        text = name,
                                        wrap = true
                                    }
                                }
                            }
                        }
                    });
                }

                // Create Adaptive Card
                var adaptiveCard = new
                {
                    type = "AdaptiveCard",
                    body = cardElements,
                    version = "1.4"
                };

                var attachment = new Microsoft.Teams.Api.MessageExtensions.Attachment
                {
                    ContentType = Microsoft.Teams.Api.ContentType.AdaptiveCard,
                    Content = adaptiveCard
                };

                return new MessageExtensionResponse
                {
                    ComposeExtension = new Result
                    {
                        Type = ResultType.Result,
                        AttachmentLayout = Microsoft.Teams.Api.Attachment.Layout.List,
                        Attachments = new List<Microsoft.Teams.Api.MessageExtensions.Attachment> { attachment }
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error($"Error in CreateWebViewResponse: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Determines the appropriate icon URL based on file name extension or MIME type
        /// </summary>
        private string GetFileIconUrl(string fileName, string mimeType)
        {
            // Get file extension
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            switch (extension)
            {
                case ".pdf":
                    return $"{_config.Teams.BaseUrl}/icons/PdfIcon.png";
                
                case ".doc":
                case ".docx":
                    return $"{_config.Teams.BaseUrl}/icons/WordIcons.png";
                
                case ".xls":
                case ".xlsx":
                case ".csv":
                    return $"{_config.Teams.BaseUrl}/icons/ExcelIcon.png";
                
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".bmp":
                case ".svg":
                    return $"{_config.Teams.BaseUrl}/icons/ImageIcon.png";
            }
            
            if (!string.IsNullOrEmpty(mimeType))
            {
                string mimeTypeLower = mimeType.ToLowerInvariant();
                
                if (mimeTypeLower.Contains("pdf"))
                    return $"{_config.Teams.BaseUrl}/icons/PdfIcon.png";
                
                if (mimeTypeLower.Contains("word") || mimeTypeLower.Contains("document"))
                    return $"{_config.Teams.BaseUrl}/icons/WordIcons.png";
                
                if (mimeTypeLower.Contains("spreadsheet") || mimeTypeLower.Contains("excel"))
                    return $"{_config.Teams.BaseUrl}/icons/ExcelIcon.png";
                
                if (mimeTypeLower.Contains("image"))
                    return $"{_config.Teams.BaseUrl}/icons/ImageIcon.png";
            }
            
            return $"{_config.Teams.BaseUrl}/icons/ImageIcon.png";
        }

        private class FileInfo
        {
            [System.Text.Json.Serialization.JsonPropertyName("name")]
            public string Name { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("type")]
            public string Type { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("size")]
            public long Size { get; set; }
        }
    }
}
