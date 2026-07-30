// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using OpenAI.Chat;

namespace MessagingExtensionReminder.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _applicationBaseUrl;
        private readonly string _azureOpenAIEndpoint;
        private readonly string _azureOpenAIApiVersion;
        private readonly string _chatCompletionModelName;

        public ActivityBot(IConfiguration configuration)
        {
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new ArgumentNullException("ApplicationBaseUrl");
            _azureOpenAIEndpoint = configuration["AZURE_OPENAI_ENDPOINT"] ?? throw new ArgumentNullException("AZURE_OPENAI_ENDPOINT");
            _azureOpenAIApiVersion = configuration["AZURE_OPENAI_API_VERSION"] ?? "2025-01-01-preview";
            _chatCompletionModelName = configuration["CHAT_COMPLETION_MODEL_NAME"] ?? throw new ArgumentNullException("CHAT_COMPLETION_MODEL_NAME");
        }

        /// <summary>
        /// Handles the messaging extension fetch task invoke to analyze sentiment of a message.
        /// </summary>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            var textToAnalyze = action.MessagePayload.Body.Content ?? string.Empty;
            textToAnalyze = Regex.Replace(textToAnalyze, @"<[^>]+>|", string.Empty).Trim();

            var sentimentResult = string.Empty;

            // Initialize Azure OpenAI client using Entra ID bearer token auth
            var credential = new DefaultAzureCredential();
            var azureClient = new AzureOpenAIClient(new Uri(_azureOpenAIEndpoint), credential);
            var chatClient = azureClient.GetChatClient(_chatCompletionModelName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You will be provided with a message, and your task is to classify its sentiment as Positive, Neutral, or Negative. Only respond with one of these three words."),
                new UserChatMessage(textToAnalyze)
            };

            try
            {
                var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
                var assistantReply = response.Value.Content[0].Text.ToLower();

                if (assistantReply.Contains("positive"))
                {
                    sentimentResult = "Positive";
                }
                else if (assistantReply.Contains("negative"))
                {
                    sentimentResult = "Negative";
                }
                else
                {
                    sentimentResult = "Neutral";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return GetTaskModuleResponse(textToAnalyze, sentimentResult);
        }

        /// <summary>
        /// Builds the messaging extension action response with the task module URL.
        /// </summary>
        private MessagingExtensionActionResponse GetTaskModuleResponse(string textToAnalyze, string result)
        {
            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Url = _applicationBaseUrl + "/SentimentModel?title=" + textToAnalyze + "&result=" + result,
                        Height = 400,
                        Width = 600,
                        Title = "Sentiment Analysis",
                    }
                }
            };
        }
    }
}
