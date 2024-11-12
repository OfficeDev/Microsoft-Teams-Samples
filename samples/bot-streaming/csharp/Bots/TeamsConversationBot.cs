// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards.Templating;
using Azure.AI.OpenAI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeamsConversationBot.Bots.Models.Streaming;
using Activity = Microsoft.Bot.Schema.Activity;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsConversationBot : TeamsActivityHandler
    {
        private string _appId;
        private string _appPassword;
        private string _appTenantId;

        private string _endpoint;
        private string _key;
        private string _deployment;
        
        private ChatClient _chatClient;
        private readonly string adaptiveCardTemplate = Path.Combine(".", "Resources", "CardTemplate.json");

        public TeamsConversationBot(IConfiguration config)
        {
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
            _appTenantId = config["MicrosoftAppTenantId"];
            _endpoint = config["AzureOpenAIEndpoint"];
            _key = config["AzureOpenAIKey"];
            _deployment = config["AzureOpenAIDeployment"];

            ApiKeyCredential credential = new ApiKeyCredential(_key);
            AzureOpenAIClient azureClient = new(new Uri(_endpoint), credential);
            
            _chatClient = azureClient.GetChatClient(_deployment);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string userInput = turnContext.Activity.Text?.Trim().ToLower();
            try
            {
                StringBuilder contentBuilder = new();
                Stopwatch stopwatch = new Stopwatch();
                int streamSequence = 1; // Sream sequence should always start with 1. 
                int rps = 1000; // The current allowance is 1 RPS.

                /*
                 * We can send an initial streaming message as informative while we get the response from the LLM setting the StreamType to Informative.
                 * This action is helpful to get the streaming sequence started and get messageId back, which we will use later as the StreamId
                 */
                ChannelData channelData = new ChannelData
                {
                    StreamType = StreamType.Informative,
                    StreamSequence = streamSequence,
                };
                string streamId = await buildAndSendStreamingActivity(turnContext, cancellationToken, "Getting the information...", channelData).ConfigureAwait(false);

                // Send request to chat client with suitable specifications 
                CollectionResult<StreamingChatCompletionUpdate> completionUpdates = _chatClient.CompleteChatStreaming(
                [
                    new SystemChatMessage("You are an AI great at storytelling which creates compelling fantastical stories."),
                    new UserChatMessage (userInput),
                ],
                new ChatCompletionOptions()
                {
                    Temperature = (float)0.7,
                    FrequencyPenalty = (float)0,
                    PresencePenalty = (float)0,
                },
                cancellationToken);

                stopwatch.Start(); // Starting stopwatch to chunk by RPS (elapsedMiliseconds)

                foreach (StreamingChatCompletionUpdate streamingChatUpdate in completionUpdates)
                {
                    streamSequence++; // Increment the streamSequence number per each update received for internal purposes
                    
                    /*
                     * If the streaming has ended for some reason, build the final message seeting the ChannelSata.StreamType to Final.
                     * Send the message to the bot and break/continue to prevent further processing.
                     */
                    if (streamingChatUpdate.FinishReason != null)
                    {
                        channelData = new ChannelData
                        {
                            StreamType = StreamType.Final,
                            StreamSequence = streamSequence,
                            StreamId = streamId
                        };
                        await buildAndSendStreamingActivity(turnContext, cancellationToken, contentBuilder.ToString(), channelData).ConfigureAwait(false);
                        break;
                    }

                    /*
                     * Teams Content Streaming feature needs bot developers to build chunks from the LLM responses. 
                     * So, we accumulate what is being send and once RPS is reached request is sent.
                     */

                    foreach (ChatMessageContentPart contentPart in streamingChatUpdate.ContentUpdate)
                    {
                        contentBuilder.Append(contentPart.Text);
                    }

                    if (contentBuilder.Length > 0 && stopwatch.ElapsedMilliseconds > rps)
                    {
                        channelData = new ChannelData
                        {
                            StreamType = StreamType.Streaming,
                            StreamSequence = streamSequence,
                            StreamId = streamId
                        };

                        stopwatch.Restart(); // Restart the stopwatch for the next chunk
                        await buildAndSendStreamingActivity(turnContext, cancellationToken, contentBuilder.ToString(), channelData).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            { 
                await turnContext.SendActivityAsync(ex.Message);
            }
        }

        /// <summary>
        /// Builds the activity with the corresponding data for streaming and sends it.
        /// </summary>
        /// <param name="turnContext">Turn context of the bot</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <param name="text">Text being streamed and to be sent as part of the activity</param>
        /// <param name="channelData">ChannelData information needed for streaming purposes</param>
        /// <returns></returns>
        private async Task<string> buildAndSendStreamingActivity(
            ITurnContext<IMessageActivity> turnContext, 
            CancellationToken cancellationToken, 
            string text, 
            ChannelData channelData)
        {
            bool isStreamFinal = channelData.StreamType.ToString().Equals(StreamType.Final.ToString());
            Activity streamingActivity = new()
            {
                Type = isStreamFinal ? ActivityTypes.Message : ActivityTypes.Typing,
                Id = channelData.StreamId,
                ChannelData = channelData
            };

            /*
             * For the moment, we need to add the streaming information in 2 places: Entities and ChannelData.
             * to prevent breaking changes in the near future.
             * The final placement for this information will be in Entities once the feature is available to 
             * the public. As per DevPreview timing, data is set in ChannelData.
             */
            var streamingInfoProperties = new
            {
                streamId = channelData.StreamId,
                streamType = channelData.StreamType.ToString(),
                streamSequence = channelData.StreamSequence,
            };

            streamingActivity.Entities = new List<Entity>
            {
                new Entity("streaminfo")
                {
                  Properties = JObject.FromObject(streamingInfoProperties)
                }
            };

            if (!string.IsNullOrEmpty(text))
            {
                streamingActivity.Text = text;
            }

            /*
             * We are sending the final streamed mesage as an Adaptive Card Attachment built 
             * using a templated.
             */
            if (isStreamFinal) 
            {
                //Build the adaptive card
                AdaptiveCardTemplate template = new AdaptiveCardTemplate(File.ReadAllText(adaptiveCardTemplate));
                var tempData = new
                {
                    finaltStreamText = text
                };
                var attachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(template.Expand(tempData)),
                };
                
                streamingActivity.Attachments = new List<Attachment>() { attachment };

                //Add text to the activuty
                streamingActivity.Text = "This is what I've got:";
            }

            return await sendStreamingActivityAsync(turnContext, cancellationToken, streamingActivity).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the activity
        /// </summary>
        /// <param name="turnContext">Turn context of the bot</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <param name="streamingActivity">Activity to be sent</param>
        /// <returns>The messageId</returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> sendStreamingActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, IActivity streamingActivity)
        {
            try
            {
                ResourceResponse streamingResponse = await turnContext.SendActivityAsync(streamingActivity, cancellationToken).ConfigureAwait(false);
                return streamingResponse.Id;
            }
            catch (Exception ex)
            {
                ErrorResponseException errorResponse = ex as ErrorResponseException;
                string excetionTemplate = "Error while sending streaming activity: ";
                await turnContext.SendActivityAsync(MessageFactory.Text(excetionTemplate + errorResponse?.Body?.Error?.Message), cancellationToken).ConfigureAwait(false);
                throw new Exception(excetionTemplate + ex.Message);
            }
        }

        //---------------------------------TeamsActivityHandler.cs--------------------------------------------
        protected override async Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation.ConversationType == "channel")
            {
                await turnContext.SendActivityAsync($"Welcome to Streaming demo bot is configured in {turnContext.Activity.Conversation.Name}. Unfurtonately, the streaming feature is not yet available for channels or group chats.");
            }
            else
            {
                await turnContext.SendActivityAsync("Welcome to Streaming demo bot! You can ask me a question and I'll do my best to answer it.");

            }
        }
    }
}