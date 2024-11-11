// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Diagnostics;
using Activity = Microsoft.Bot.Schema.Activity;
using TeamsConversationBot.Bots.Models.Streaming;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsConversationBot : TeamsActivityHandler
    {
        private string _appId;
        private string _appPassword;
        private string _appTenantId;

        public TeamsConversationBot(IConfiguration config)
        {
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
            _appTenantId = config["MicrosoftAppTenantId"];
        }

        private readonly string streamingText = Path.Combine(".", "Resources", "streamingText.txt");
        private readonly string streamingLongText = Path.Combine(".", "Resources", "streamingLongText.txt");
        private readonly string streamingRichText = Path.Combine(".", "Resources", "streamingRichText.txt");
        private readonly string streamingAdaptiveCard = Path.Combine(".", "Resources", "streaming-ac.json");
        private readonly string otherStreamingScenariosCard = Path.Combine(".", "Resources", "OthersStreamingScenariosCard.json");
        private readonly string[] chunkedParameters = ["-chars", "-chunks"];

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            List<string> splitted = turnContext.Activity.Text?.Trim().ToLower().Split(' ',StringSplitOptions.RemoveEmptyEntries).ToList();

            string command = splitted[0];
            if (string.IsNullOrEmpty(command))
            {
                JObject actiondata = JObject.FromObject(turnContext.Activity.Value);
                command = actiondata["value"].ToString();
            }

            string textContent = File.ReadAllText(streamingText);
            switch (command)
            {
                case "info": //Scenario 1-(A): 1.Informative messages -> 2.Streaming content -> 3.Final message
                    await informativeStreamingActivity(turnContext, cancellationToken, textContent).ConfigureAwait(false);
                    break;
                case "stream": //Scenario 1-(B): 1.Streaming text content -> 2.Final message
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1).ConfigureAwait(false);
                    break;
                case "stream-long": //Scenario 1-(B): 1.Streaming text content -> 2.Final message
                    await sendStreamingActivity(turnContext, cancellationToken, File.ReadAllText(streamingLongText), 1).ConfigureAwait(false);
                    break;
                case "stream-rich-text": //Scenario 1-(C): 1.Streaming rich content -> 2.Final message
                    string richTextContent = File.ReadAllText(streamingRichText);
                    await sendStreamingActivity(turnContext, cancellationToken, richTextContent, 1).ConfigureAwait(false);
                    break;
                case "stream-empty": //Scenario 1-(D): 1.Streaming without text 
                    await emptyStartStreamingActivity(turnContext, cancellationToken, textContent).ConfigureAwait(false);
                    break;
                case "stream-disordered": //Scenario 1-(E): Streaming content and final message unordered
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, disordered: true).ConfigureAwait(false);
                    break;
                case "stream-overwrite": //Scenario 1-(F): 1.Streaming text content -> 2.Final message overwrites the streaming content sent before
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, overwrite: true).ConfigureAwait(false);
                    break;
                case "stream-update": //Scenario 1-(G): 1.Streaming content -> 2. Send MessageUpdate while streaming
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, updateInParallel: true).ConfigureAwait(false);
                    break;
                case "stream-info-final": //Scenario 1-(H): 1.Streaming content -> 2.Informative message -> 3.Final message
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, followWithInfo: true).ConfigureAwait(false);
                    break;
                case "info-final": //Scenario 1-(I): 1.Informative message -> 2.Final content
                    await infoFinalStreamingActivity(turnContext, cancellationToken, textContent).ConfigureAwait(false);
                    break;
                case "info-empty": //Scenario 1-(J): 1.Informative message without text
                    await sendInformativeStreamingActivity(turnContext, cancellationToken, string.Empty, 1).ConfigureAwait(false);
                    break;
                case "final-empty": //Scenario 1-(K): 1.Informative message -> 2.Final message without text
                    await infoFinalStreamingActivity(turnContext, cancellationToken, string.Empty).ConfigureAwait(false);
                    break;
                case "final-continue": //Scenario 1-(L):  1.Informative message -> 2.Final message -> 3.Streaming resumes.
                    await toBeContinuedStreamingActivity(turnContext, cancellationToken, textContent, StreamType.Streaming).ConfigureAwait(false);
                    break;
                case "final-info": //Scenario 1-(M): 1.Informative message -> 2.Final message -> 3.Informative message
                    await toBeContinuedStreamingActivity(turnContext, cancellationToken, textContent, StreamType.Informative).ConfigureAwait(false);
                    break;
                case "text":// Send the complete message
                    await turnContext.SendActivityAsync(MessageFactory.Text(textContent), cancellationToken);
                    break;
                case "text-stream"://Scenario 1-(N): 1.Regular message -> 2.Streaming content using the same ActivityId as StreamId
                    var response = await turnContext.SendActivityAsync(MessageFactory.Text("This is a regular message to be updated with streaming"), cancellationToken);
                    Task.Delay(1000).Wait();
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, response.Id).ConfigureAwait(false);
                    break;
                case "stream-expire": //Scenario 1-(0): 1.Streaming text content for over 60 seconds long so it expires
                    await sendStreamingActivity(turnContext, cancellationToken, File.ReadAllText(streamingLongText), 1, expireStream: true).ConfigureAwait(false);
                    break;
                case "stream-ac": //Scenario 1-(P): 1.Streaming starts as text but final message sends text + adaptive card as attachment
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, acAttached: 1).ConfigureAwait(false);
                    break;
                case "stream-ac+": //Scenario 1-(Q): 1.Streaming starts as text but final message sends text + multiple adaptive cards as attachments
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, acAttached: 2).ConfigureAwait(false);
                    break;
                case "stream-ac+-carousel": //Scenario 1-(R):  1.Streaming starts as text but final message sends text + multiple adaptive cards as attachments, specifying carousel layout
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, acAttached: 2, isCarousel: true).ConfigureAwait(false);
                    break;
                case "stream-ac-only": //Scenario 1-(S): 1.Streaming starts as text but final message sends an adaptive card as attachment (no text)
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, acAttached: 1, acPlusText: false).ConfigureAwait(false);
                    break;
                case "stream-ac+-only": //Scenario 1-(T): 1.Streaming starts as text but final message sends multiple adaptive cards as attachments (no text)
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, acAttached: 2, acPlusText: false).ConfigureAwait(false);
                    break;
                case "stream-ac+-only-carousel": //Scenario 1-(U): 1.Streaming starts as text but final message sends multiple adaptive cards as attachments (no text), specifying carousel layout
                    await sendStreamingActivity(turnContext, cancellationToken, textContent, 1, acAttached: 2, acPlusText: false, isCarousel: true).ConfigureAwait(false);
                    break;
                case "stream-chunked": //Scenario 1-(V): 1.Streaming specific number of chunks, each with specific char lenght
                    splitted.RemoveAt(0);
                    IDictionary<string, int> args = ResolveChunkedAguments(splitted);
                    await sendStreamingActivitySpecificChunks(turnContext, cancellationToken, textContent, args).ConfigureAwait(false);
                    break;
                case "text-ac": // Send regular message with text and an adaptive card as attachment
                    await sendOtherStreamingScenariosCard(turnContext, cancellationToken, true).ConfigureAwait(false);
                    break;
                case "text-ac+": // Send regular message with text and multiple adaptive cards as attachments
                    await sendOtherStreamingScenariosCard(turnContext, cancellationToken, true, 1).ConfigureAwait(false);
                    break;
                case "text-ac+-carousel": //Send regular message with text and multiple adaptive cards as attachments, specifying carousel layout
                    await sendOtherStreamingScenariosCard(turnContext, cancellationToken, true, 1, isCarousel: true).ConfigureAwait(false);
                    break;
                case "ac+": //Send regular message with multiple adaptive cards as attachments (no text)
                    await sendOtherStreamingScenariosCard(turnContext, cancellationToken, false, 1).ConfigureAwait(false);
                    break;
                case "ac+-carousel": //Send regular message with multiple adaptive cards as attachments (no text), specifying carousel layout
                    await sendOtherStreamingScenariosCard(turnContext, cancellationToken, false, 1, isCarousel: true).ConfigureAwait(false);
                    break;
                case "+":
                    await sendOtherStreamingScenariosCard(turnContext, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    try
                    {
                        Activity message = MessageFactory.Text("Hello there! please review the available commands to test streaming feature. Write '+' for more options");
                        await turnContext.SendActivityAsync(message, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    { 
                        var something = ex.Message;
                    }
                    break;
            }
        }

        private async Task informativeStreamingActivity(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string textContent)
        {
            //Bot sends the first request with content && the content is informative loading message.
            string id = await sendInformativeStreamingActivity(turnContext, cancellationToken, "Gathering creative ideas...", 1).ConfigureAwait(false);
            Task.Delay(1000).Wait();

            //Bot sends the second request with content && the content is informative loading message.
            await sendInformativeStreamingActivity(turnContext, cancellationToken, "Imaging characters and location...", 2, id).ConfigureAwait(false);
            Task.Delay(1000).Wait();

            // Bot sends the third request with content && the content is actual streaming content.
            await sendStreamingActivity(turnContext, cancellationToken, textContent, 3, id).ConfigureAwait(false);
        }

        private async Task emptyStartStreamingActivity(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string textContent)
        {
            //Bot sends the first request without content 
            Activity streamingActivity = (Activity)Activity.CreateTypingActivity();
            streamingActivity.ChannelData = new ChannelData { StreamSequence = 1 };

            string streamId = await sendStreamingActivityAsync(turnContext, cancellationToken, streamingActivity).ConfigureAwait(false);

            // Bot sends the second request already streaming content.
            await sendStreamingActivity(turnContext, cancellationToken, textContent, 2, streamId).ConfigureAwait(false);
        }

        private async Task<string> infoFinalStreamingActivity(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string textContent)
        {
            //Bot sends the first request with informative content
            string id = await sendInformativeStreamingActivity(turnContext, cancellationToken, "Gathering creative ideas...", 1).ConfigureAwait(false);
            Task.Delay(1000).Wait();

            // Create a new instance of the ChannelData class, with the StreamType as Final
            ChannelData channelData = new ChannelData
            {
                StreamType = StreamType.Final,
                StreamSequence = 2,
                StreamId = id
            };

            //Build and send the activity with corresponding data and get the response to be used for subsequent streaming calls
            await buildAndSendStreamingActivity(turnContext, cancellationToken, textContent, channelData).ConfigureAwait(false);

            return id;
        }

        private async Task toBeContinuedStreamingActivity(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string textContent, StreamType streamType)
        {
            //Bot sends the first request with informative content and final message
            string id = await infoFinalStreamingActivity(turnContext, cancellationToken, textContent).ConfigureAwait(false); 
            Task.Delay(1000).Wait();

            // Bot sends second request with content and StreamType as Final
            ChannelData channelData = new ChannelData
            {
                StreamType = streamType,
                StreamSequence = 3,
                StreamId = id
            };

            string text = streamType.Equals(StreamType.Streaming) ? "Streaming to be continued..." : "Additional Info message.";
            await buildAndSendStreamingActivity(turnContext, cancellationToken, textContent + text, channelData).ConfigureAwait(false);
        }

        private async Task<string> sendInformativeStreamingActivity(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string text, int streamSequence, string streamId = null)
        {
            // Create a new instance of the ChannelData class, which consists of the streaming properties to be sent
            ChannelData channelData = new ChannelData
            {
                StreamType = StreamType.Informative,
                StreamSequence = streamSequence,
                StreamId = streamId
            };

            //Build and send the activity with corresponding data and get the response to be used for subsequent streaming calls
            return await buildAndSendStreamingActivity(turnContext, cancellationToken, text, channelData).ConfigureAwait(false);
        }

        private async Task sendStreamingActivity(
            ITurnContext<IMessageActivity> turnContext, 
            CancellationToken cancellationToken, 
            string textContent, 
            int streamSequenceStartingPoint, 
            string streamId = null,
            bool disordered = false,
            bool overwrite = false,
            bool followWithInfo = false, 
            bool updateInParallel = false, 
            bool expireStream = false,
            int acAttached = 0,
            bool acPlusText = true, 
            bool isCarousel = false)
        {
            string[] words = textContent.Split(' ');
            StringBuilder sb = new StringBuilder();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Dictionary<string, ChannelData> chunks = new Dictionary<string, ChannelData>();

            for (int i = 0; i < words.Length; i++)
            {
                //Build the streaming content to be sent as the text in the Activity
                sb.Append($"{words[i]}");
                sb.Append(' ');

                int delay = expireStream? 500 : 1;
                Task.Delay(delay).Wait();

                bool isFinalChunk = i == words.Length - 1;

                if (stopwatch.ElapsedMilliseconds >= 1250 || isFinalChunk)
                {
                    //Build the ChannelData object with the streaming properties
                    ChannelData channelData = new ChannelData
                    {
                        StreamType = isFinalChunk ? StreamType.Final : StreamType.Streaming,
                        StreamSequence = streamSequenceStartingPoint + i,
                        StreamId = streamId
                    };

                    // Send disordered
                    if (disordered || followWithInfo)
                    {
                        chunks.Add(sb.ToString(), channelData);
                        stopwatch.Restart();
                        continue;
                    }

                    // Overwrite the last message sent if the overwrite flag is set to true
                    if (isFinalChunk)
                    {
                        if (overwrite)
                        {
                            sb.Clear();
                            sb.Append("Nevermind...Just forget it");
                        }

                        if (acAttached != 0)
                        {
                            sb.Clear();
                            if (acPlusText) 
                            {
                                sb.Append("Streaming text as attachment");
                            }
                            await buildAndSendStreamingActivity(turnContext, cancellationToken, sb.ToString(), channelData, acAttached, isCarousel).ConfigureAwait(false);
                            continue;
                        }
                    }

                    //Build and send the activity with corresponding data
                    string responseStreamId = await buildAndSendStreamingActivity(turnContext, cancellationToken, sb.ToString(), channelData).ConfigureAwait(false);

                    //Set the streamId to the first response id that was returned so that streaming can continue
                    if (streamId == null)
                    {
                        streamId = responseStreamId;

                        //Try to update the same activity while streaming
                        if (updateInParallel)
                        {
                            Activity updateActivity = MessageFactory.Text("Wait, gathering some more ideas...");
                            updateActivity.Id = streamId;
                            await turnContext.UpdateActivityAsync(updateActivity).ConfigureAwait(false);

                            Task.Delay(500).Wait();
                        }
                    }

                    // Reset the stopwatch so that the next streaming activity can be sent after 1 second
                    stopwatch.Restart();
                }
            }

            if (disordered)
            {
                await sendDisorderedStreamingActivity(turnContext, cancellationToken, chunks).ConfigureAwait(false);
            }

            if (followWithInfo)
            {
                await sendStreamingFollowedWithInfoActivity(turnContext, cancellationToken, chunks).ConfigureAwait(false);
            }
        }

        private async Task sendStreamingActivitySpecificChunks(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken,
            string textContent,
            IDictionary<string, int> args)
        {
            if (args.Count == 0)
            {
               await turnContext.SendActivityAsync(MessageFactory.Text("For 'stream-chunked' command provide either or both parameters: -chars, -chunks"), cancellationToken).ConfigureAwait(false);
            }

            args.TryGetValue(chunkedParameters[0], out int charsToIncludePerChunk);
            args.TryGetValue(chunkedParameters[1], out int totalChunksToSend);

            // Both params are provided - String will be sliced to the number of chunks set in the param
            if (charsToIncludePerChunk != 0 && totalChunksToSend != 0)
            {
                var chunks = textContent.Chunk(charsToIncludePerChunk).Select(x => new string(x)).ToList();
                await sendChunksByCharCount(chunks, totalChunksToSend, turnContext, cancellationToken).ConfigureAwait(false);
            }

            // If no chunks param provided, set the total number of chunks to send based on the char chunk length
            else if (totalChunksToSend == 0)
            {
                var chunks = textContent.Chunk(charsToIncludePerChunk).Select(x => new string(x)).ToList();
                await sendChunksByCharCount(chunks, chunks.Count, turnContext, cancellationToken).ConfigureAwait(false);
            }

            // If no char lenght for chunks is provided, calculate the amount of chars to set per chunk
            else if (charsToIncludePerChunk == 0)
            {
                List<string> chunks = new();
                StringBuilder sb = new StringBuilder();

                string[] words = textContent.Split(' ');

                // Complete Chunks build
                int reminderWords = words.Length % totalChunksToSend;
                int wordsPerCompleteChunk = (words.Length - reminderWords) / (totalChunksToSend - 1);
                int startIndexForFinalChunk = words.Length - reminderWords;

                for (int i = 0; i < startIndexForFinalChunk; i++)
                {
                    sb.Append($"{words[i]}");
                    sb.Append(' ');

                    if (i != 0)
                    {
                        int counterReminder = i % wordsPerCompleteChunk;
                        if (counterReminder == 0)
                        {
                            chunks.Add(sb.ToString());
                        }
                    }
                }

                // Final Chunk build
                for (int i = startIndexForFinalChunk; i < words.Length; i++)
                {
                    sb.Append($"{words[i]}");
                    sb.Append(' ');
                }
                chunks.Add(sb.ToString());

                // Send prepared chunks
                int streamSequenceStartingPoint = 1;
                string streamId = null;

                for (int i = 0; i < chunks.Count; i++)
                {
                    ChannelData channelData = new ChannelData
                    {
                        StreamType = StreamType.Streaming,
                        StreamSequence = streamSequenceStartingPoint + i,
                        StreamId = streamId
                    };

                    if (i == chunks.Count - 1)
                    {
                        channelData.StreamType = StreamType.Final;
                    }

                    //Build and send the activity with corresponding data
                    string responseStreamId = await buildAndSendStreamingActivity(turnContext, cancellationToken, chunks[i], channelData).ConfigureAwait(false);

                    // Adding a delay to respect the current RPS of 1
                    Task.Delay(1000).Wait();

                    //Set the streamId to the first response id that was returned so that streaming can continue
                    if (streamId == null)
                    {
                        streamId = responseStreamId;
                    }
                }
            }
        }

        private IDictionary<string, int> ResolveChunkedAguments(List<string> parameters)
        {
            IDictionary<string, int> arguments = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (string arg in chunkedParameters)
            {
                var paramIndex = parameters.IndexOf(arg);
                if (paramIndex >= 0)
                {
                    arguments.Add(arg, Int32.Parse(parameters[paramIndex + 1]));
                }
            }

            return arguments;
        }

        private async Task sendChunksByCharCount(List<string> chunks, int totalChunks, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            int streamSequenceStartingPoint = 1;
            string streamId = null;

            for (int i = 0; i < totalChunks; i++)
            {
                bool isFinalChunk = i == totalChunks - 1;
                sb.Append($"{chunks[i]}");

                ChannelData channelData = new ChannelData
                {
                    StreamType = StreamType.Streaming,
                    StreamSequence = streamSequenceStartingPoint + i,
                    StreamId = streamId
                };

                if (isFinalChunk)
                {
                    channelData.StreamType = StreamType.Final;
                }

                //Build and send the activity with corresponding data
                string responseStreamId = await buildAndSendStreamingActivity(turnContext, cancellationToken, sb.ToString(), channelData).ConfigureAwait(false);

                // Adding a delay to respect the current RPS of 1
                Task.Delay(950).Wait();

                //Set the streamId to the first response id that was returned so that streaming can continue
                if (streamId == null)
                {
                    streamId = responseStreamId;
                }
            }
        }

        private async Task sendStreamingFollowedWithInfoActivity(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, Dictionary<string, ChannelData> chunks)
        {
            string streamId = null;

            for (int i = 0; i < chunks.Count - 2; i++)
            {
                string key = chunks.ElementAt(i).Key;
                ChannelData value = chunks.ElementAt(i).Value;
                value.StreamId = streamId;

                string responseStreamId = await buildAndSendStreamingActivity(turnContext, cancellationToken, key, value).ConfigureAwait(false);

                //Set the streamId to the first response id that was returned so that streaming can continue
                if (streamId == null)
                {
                    streamId = responseStreamId;
                }

                Task.Delay(500).Wait();
            }

            //Send info message
            await sendInformativeStreamingActivity(turnContext, cancellationToken, "Getting all the information...", chunks.Count - 1, streamId).ConfigureAwait(false);
            Task.Delay(1000).Wait();

            //Send final message
            string lastKey = chunks.Last().Key;
            ChannelData lastValue = chunks.Last().Value;
            lastValue.StreamId = streamId;
            lastValue.StreamSequence = chunks.Count;
            await buildAndSendStreamingActivity(turnContext, cancellationToken, lastKey, lastValue).ConfigureAwait(false);
        }

        private async Task sendDisorderedStreamingActivity( ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, Dictionary<string, ChannelData> chunks)
        {
            Random random = new Random();
            string streamId = null;

            List<string> keys = chunks.Keys.OrderBy(x => random.Next()).ToList();

            //Check if the first message is a final message. Sending the final message as first message without ChannelData.StreamId will fail
            string firstItemString = keys.ElementAt(0);
            ChannelData firstItemStreamType = chunks[firstItemString];
            if (firstItemStreamType.StreamType == StreamType.Final)
            {
                keys.RemoveAt(0);
                keys.Add(firstItemString);
            }

            for (int i = 0; i < keys.Count(); i++)
            {
                string key = keys.ElementAt(i);
                ChannelData value = chunks[key];
                value.StreamId = streamId;

                string responseStreamId = await buildAndSendStreamingActivity(turnContext, cancellationToken, key, value).ConfigureAwait(false);

                //Set the streamId to the first response id that was returned so that streaming can continue
                if (streamId == null)
                {
                    streamId = responseStreamId;
                }

                Task.Delay(500).Wait();
            }
        }

        private async Task<string> buildAndSendStreamingActivity(
            ITurnContext<IMessageActivity> turnContext, 
            CancellationToken cancellationToken, 
            string text, 
            ChannelData channelData, 
            int addAttachment = 0,
            bool isCarousel = false)
        {
            // Streaming start streamsequence should be 1.
            if (channelData.StreamId == null)
            {
                channelData.StreamSequence = 1;
            }

            Activity streamingActivity = new Activity();
            streamingActivity.Type = channelData.StreamType.ToString().Equals(StreamType.Final.ToString()) ? ActivityTypes.Message : ActivityTypes.Typing;
            streamingActivity.Id = channelData.StreamId;
            streamingActivity.ChannelData = channelData;
            
            var jobject = new 
            {
                streamId = channelData.StreamId,
                streamType = channelData.StreamType.ToString(),
                streamSequence = channelData.StreamSequence,
            };

            streamingActivity.Entities = new List<Entity> 
            { 
                new Entity("streaminfo")
                {
                  Properties = JObject.FromObject(jobject)
                }
            };
            
            var attachments = new List<Attachment>();

            if (!string.IsNullOrEmpty(text))
            {
                streamingActivity.Text = text;
            }

            // Attachment 1
            var adaptiveCardJson = File.ReadAllText(Path.Combine(streamingAdaptiveCard));
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            //Attachment 2
            var adaptiveCardJsonAdditionalCards = File.ReadAllText(Path.Combine(otherStreamingScenariosCard));
            var adaptiveCardAttachmentAdditionalCards = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJsonAdditionalCards),
            };

            if (addAttachment == 1)
            {
                attachments.Add(adaptiveCardAttachment);
                streamingActivity.Attachments = attachments;
            }

            if (addAttachment == 2)
            {
                attachments.Add(adaptiveCardAttachment);
                attachments.Add(adaptiveCardAttachmentAdditionalCards);
                streamingActivity.Attachments = attachments;

                if (isCarousel)
                {
                    streamingActivity.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                }
            }

            return await sendStreamingActivityAsync(turnContext, cancellationToken, streamingActivity).ConfigureAwait(false);
        }

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

        private async Task sendOtherStreamingScenariosCard(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, 
            bool addText = false, 
            int additionalAttachments = 0,
            bool isCarousel = false)
        {
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);

            // Attachment 1
            string otherOptionsCard = File.ReadAllText(otherStreamingScenariosCard);
            var otherOptionsCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(otherOptionsCard),
            };

            if (addText)
            {
                reply.Text = "Additional message sent along with attachment in reply";
            }
            
            reply.Attachments.Add(otherOptionsCardAttachment);

            if (additionalAttachments != 0)
            {
                // Attachment 2
                var adaptiveCardJson = File.ReadAllText(Path.Combine(streamingAdaptiveCard));
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson),
                };

                reply.Attachments.Add(adaptiveCardAttachment);

                if (isCarousel)
                {
                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                }
            }

            await sendStreamingActivityAsync(turnContext, cancellationToken, reply).ConfigureAwait(false);
        }

        //---------------------------------TeamsActivityHandler.cs--------------------------------------------
        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var teamMember in membersAdded)
            {
                if (teamMember.Id != turnContext.Activity.Recipient.Id && turnContext.Activity.Conversation.ConversationType != "personal")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to the team {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);
                }
            }
        }

        protected override async Task OnTeamsMembersAddedDispatchAsync(IList<ChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (TeamsChannelAccount member in membersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    // Send a message to introduce the bot to the team.
                    var heroCard = new HeroCard(text: $"The {member.Name} bot has joined {teamInfo.Name}");
                    // Sends an activity to the sender of the incoming activity.
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
                }
                else
                {
                    var heroCard = new HeroCard(text: $"{member.Name} joined {teamInfo.Name}");
                    // Sends an activity to the sender of the incoming activity.
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
                }
            }
        }

        protected override async Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation.ConversationType == "channel")
            {
                await turnContext.SendActivityAsync($"Welcome to Streaming demo bot. This bot is configured in {turnContext.Activity.Conversation.Name}");
            }
            else
            {
                await turnContext.SendActivityAsync("Welcome to Streaming demo bot.");
            }
        }

        //-----Subscribe to Conversation Events in Bot integration
        protected override async Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{channelInfo.Name} is the Channel created");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsChannelRenamedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{channelInfo.Name} is the new Channel name");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{channelInfo.Name} is the Channel deleted");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (TeamsChannelAccount member in membersRemoved)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    // The bot was removed
                    // You should clear any cached data you have for this team
                }
                else
                {
                    var heroCard = new HeroCard(text: $"{member.Name} was removed from {teamInfo.Name}");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
                }
            }
        }
        protected override async Task OnTeamsTeamRenamedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{teamInfo.Name} is the new Team name");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }
        protected override async Task OnReactionsAddedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var reaction in messageReactions)
            {
                var newReaction = $"You reacted with '{reaction.Type}' to the following message: '{turnContext.Activity.ReplyToId}'";
                var replyActivity = MessageFactory.Text(newReaction);
                await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            }
        }

        protected override async Task OnReactionsRemovedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var reaction in messageReactions)
            {
                var newReaction = $"You removed the reaction '{reaction.Type}' from the following message: '{turnContext.Activity.ReplyToId}'";
                var replyActivity = MessageFactory.Text(newReaction);
                await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            }
        }

        // This method is invoked when message sent by user is updated in chat.
        protected override async Task OnTeamsMessageEditAsync(ITurnContext<IMessageUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyActivity = MessageFactory.Text("Message is updated");
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        // This method is invoked when message sent by user is undeleted in chat.
        protected override async Task OnTeamsMessageUndeleteAsync(ITurnContext<IMessageUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyActivity = MessageFactory.Text("Message is undeleted");
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        // This method is invoked when message sent by user is soft deleted in chat.
        protected override async Task OnTeamsMessageSoftDeleteAsync(ITurnContext<IMessageDeleteActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyActivity = MessageFactory.Text("Message is soft deleted");
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }
    }
}
