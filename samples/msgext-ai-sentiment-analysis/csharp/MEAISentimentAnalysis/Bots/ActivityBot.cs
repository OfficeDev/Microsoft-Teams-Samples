// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using OpenAI_API;

namespace MessagingExtensionReminder.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _applicationBaseUrl;
        protected readonly BotState _conversationState;
        private readonly string _openAPIKey;
        private readonly string _chatCompletionModelName;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public ActivityBot(IConfiguration configuration, ConversationState conversationState, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _conversationReferences = conversationReferences;
            _conversationState = conversationState;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
            _openAPIKey = configuration["SECRET_OPENAI_API_KEY"] ?? throw new NullReferenceException("SECRET_OPENAI_API_KEY");
            _chatCompletionModelName = configuration["CHAT_COMPLETION_MODEL_NAME"] ?? throw new NullReferenceException("CHAT_COMPLETION_MODEL_NAME");
        }

        /// <summary>
        /// When OnTurn method receives a submit invoke activity on bot turn, it calls this method.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Provides context for a turn of a bot and.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents a task module response.</returns>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            var textToAnalyze = string.Empty;
            var sentimentResult = string.Empty;

            textToAnalyze = action.MessagePayload.Body.Content;
            textToAnalyze = Regex.Replace(textToAnalyze, @"<[^>]+>|","").Trim();

            string apiKey = _openAPIKey;
            var openAI = new OpenAIAPI(apiKey);

            string systemMessage = "You will be provided with a tweet, and your task is to classify its sentiment as positive, neutral, or negative";

            // Chat message sent or received from the API inlcudes 'Content'.
            var custom_Message = new List<OpenAI_API.Chat.ChatMessage>
            {
            new OpenAI_API.Chat.ChatMessage{Content=systemMessage},
            new OpenAI_API.Chat.ChatMessage{Content=textToAnalyze}
            };

            try
            {
                var response = await openAI.Chat.CreateChatCompletionAsync(
                    model: _chatCompletionModelName,
                    messages: custom_Message,
                    max_tokens: 256 // Adjust as needed
                );

                string assistantReply = response.Choices[0].Message.Content.ToLower();

                // Extract the sentiment analysis result from the assistant's reply
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

            // return actual message and sentiment result
            return this.GetTaskModuleResponse(textToAnalyze, sentimentResult);
        }

        /// <summary>
        /// Get messaging extension action response for sentiment analysis.
        /// </summary>
        /// <param name="textToAnalyze">text to analyze sentiment.</param>
        /// <param name="result">sentiment result for a message.</param>
        /// <returns>MessagingExtensionActionResponse object.</returns>
        private MessagingExtensionActionResponse GetTaskModuleResponse(string textToAnalyze, string result)
        {
            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Url = _applicationBaseUrl + "/" + "SentimentModel?title=" + textToAnalyze + "&result=" + result,
                        Height = 400,
                        Width = 600,
                        Title = "Schedule-task",
                    },
                },
            };
        }
    }
}