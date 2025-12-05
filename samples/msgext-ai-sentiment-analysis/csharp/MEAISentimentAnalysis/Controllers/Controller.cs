using Microsoft.Teams.Api.MessageExtensions;
using Microsoft.Teams.Api.TaskModules;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using Microsoft.Teams.Common;
using OpenAI.Chat;
using System.Text.RegularExpressions;
using AdaptiveCard = Microsoft.Teams.Cards.AdaptiveCard;
using MessageExtensionResponse = Microsoft.Teams.Api.MessageExtensions.Response;
using TaskModuleSize = Microsoft.Teams.Api.TaskModules.Size;

namespace MEAISentimentAnalysis.Controllers
{
    [TeamsController]
    public class Controller
    {
        private readonly ConfigOptions _config;
        private readonly string _openAIApiKey;
        private readonly string _modelName;
        private readonly string _applicationBaseUrl;

        public Controller(IConfiguration configuration)
        {
            _config = configuration.Get<ConfigOptions>() ?? throw new NullReferenceException("ConfigOptions");
            _openAIApiKey = _config.Teams.ApiKey ?? throw new NullReferenceException("OpenAI:ApiKey is not configured");
            _modelName = _config.Teams.ModelName ?? "gpt-4o-mini";
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? "https://localhost";
        }

        // Handle message extension submit action (when action is submitted)
        public async Task<MessageExtensionResponse> OnSubmitAction(
            [Context] Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.SubmitActionActivity activity,
            [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            var commandId = activity.Value?.CommandId;

            if (commandId == "analyzeSentiment")
            {
                return await HandleAnalyzeSentiment(activity, log);
            }

            log.Error($"Unknown command: {commandId}");
            return CreateErrorResponse("Unknown command");
        }

        // Handle message extension fetch task (when task module is opened)
        [Invoke("composeExtension/fetchTask")]
        public async Task<ActionResponse> OnMessageExtensionFetchTask(
            [Context] Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.FetchTaskActivity activity,
            [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {

            var commandId = activity.Value?.CommandId;

            if (commandId == "me-sentiment-ai")
            {
                return await HandleAnalyzeSentimentFetchTask(activity, log);
            }

            return CreateErrorTaskResponse("Unknown command");
        }

        private async Task<MessageExtensionResponse> HandleAnalyzeSentiment(
            Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.SubmitActionActivity activity, 
            Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                var textToAnalyze = activity.Value?.MessagePayload?.Body?.Content ?? string.Empty;
                
                // Remove HTML tags and extra whitespace
                textToAnalyze = Regex.Replace(textToAnalyze, @"<[^>]+>|&nbsp;", "").Trim();

                if (string.IsNullOrEmpty(textToAnalyze))
                {
                    return CreateErrorResponse("No text to analyze");
                }

                // Perform sentiment analysis using OpenAI
                var sentimentResult = await AnalyzeSentimentWithOpenAI(textToAnalyze, log);

                // Create adaptive card with results
                var card = CreateSentimentResultCard(textToAnalyze, sentimentResult);

                var attachment = new Microsoft.Teams.Api.MessageExtensions.Attachment
                {
                    ContentType = Microsoft.Teams.Api.ContentType.AdaptiveCard,
                    Content = card
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
                log.Error($"Error analyzing sentiment: {ex.Message}");
                return CreateErrorResponse($"Error analyzing sentiment: {ex.Message}");
            }
        }

        private async Task<ActionResponse> HandleAnalyzeSentimentFetchTask(
            Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.FetchTaskActivity activity, 
            Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                var textToAnalyze = activity.Value?.MessagePayload?.Body?.Content ?? string.Empty;
                
                // Remove HTML tags and extra whitespace
                textToAnalyze = Regex.Replace(textToAnalyze, @"<[^>]+>|&nbsp;", "").Trim();

                if (string.IsNullOrEmpty(textToAnalyze))
                {
                    return CreateErrorTaskResponse("No text to analyze");
                }

                // Perform sentiment analysis using OpenAI
                var sentimentResult = await AnalyzeSentimentWithOpenAI(textToAnalyze, log);

                // Create adaptive card with results
                var card = CreateSentimentResultCard(textToAnalyze, sentimentResult);

                return new ActionResponse
                {
                    Task = new ContinueTask(new TaskInfo
                    {
                        Title = "Sentiment Analysis Result",
                        Height = new Union<int, TaskModuleSize>(TaskModuleSize.Medium),
                        Width = new Union<int, TaskModuleSize>(TaskModuleSize.Medium),
                        Card = new Microsoft.Teams.Api.Attachment(card)
                    })
                };
            }
            catch (Exception ex)
            {
                log.Error($"Error analyzing sentiment: {ex.Message}");
                return CreateErrorTaskResponse($"Error analyzing sentiment: {ex.Message}");
            }
        }

        private async Task<string> AnalyzeSentimentWithOpenAI(string text, Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                var client = new ChatClient(_modelName, _openAIApiKey);

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You will be provided with a text message, and your task is to classify its sentiment as positive, neutral, or negative. Respond with only one word: Positive, Neutral, or Negative."),
                    new UserChatMessage(text)
                };

                var completion = await client.CompleteChatAsync(messages);
                var assistantReply = completion.Value.Content[0].Text.Trim();
                var sentimentLower = assistantReply.ToLower();
                
                // Try exact match first (most common case)
                if (sentimentLower == "positive") return "Positive";
                if (sentimentLower == "negative") return "Negative";
                if (sentimentLower == "neutral") return "Neutral";
                
                // Fallback: Check if response contains sentiment words
                if (sentimentLower.Contains("positive")) return "Positive";
                if (sentimentLower.Contains("negative")) return "Negative";
                if (sentimentLower.Contains("neutral")) return "Neutral";
                
                // Default to Neutral if unable to determine
                return "Neutral";
            }
            catch (Exception ex)
            {
                log.Error($"OpenAI API error: {ex.Message}");
                throw;
            }
        }

        private AdaptiveCard CreateSentimentResultCard(string actualMessage, string result)
        {
            var color = result switch
            {
                "Positive" => TextColor.Good,
                "Negative" => TextColor.Attention,
                _ => TextColor.Default
            };

            return new AdaptiveCard
            {
                Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                Body = new List<CardElement>
                {
                    new TextBlock("Sentiment Analysis Result")
                    {
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Large,
                        Color = TextColor.Accent
                    },
                    new TextBlock("Actual Message")
                    {
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Medium,
                        Spacing = Spacing.Medium
                    },
                    new TextBlock(actualMessage)
                    {
                        Wrap = true,
                        IsSubtle = true
                    },
                    new TextBlock("Result")
                    {
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Medium,
                        Spacing = Spacing.Medium
                    },
                    new TextBlock(result)
                    {
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Large,
                        Color = color
                    }
                }
            };
        }

        private MessageExtensionResponse CreateErrorResponse(string message)
        {
            return new MessageExtensionResponse
            {
                ComposeExtension = new Result
                {
                    Type = ResultType.Message,
                    Text = message
                }
            };
        }

        private ActionResponse CreateErrorTaskResponse(string message)
        {
            var card = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Error")
                    {
                        Weight = TextWeight.Bolder,
                        Color = TextColor.Attention
                    },
                    new TextBlock(message)
                    {
                        Wrap = true
                    }
                }
            };

            return new ActionResponse
            {
                Task = new ContinueTask(new TaskInfo
                {
                    Title = "Error",
                    Height = new Union<int, TaskModuleSize>(TaskModuleSize.Small),
                    Width = new Union<int, TaskModuleSize>(TaskModuleSize.Small),
                    Card = new Microsoft.Teams.Api.Attachment(card)
                })
            };
        }
    }
}