const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const { OpenAI } = require("openai");
const { StreamType, ChannelData } = require('./streamingModels');
const config = require("./config");

// Create storage for conversation history
const storage = new LocalStorage();

// Create the app with storage and authentication
const app = new App({
  botId: config.MicrosoftAppId,
  botPassword: config.MicrosoftAppPassword,
  storage,
});

// Build and send a streaming activity (either ongoing or final)
async function buildAndSendStreamingActivity(context, text, channelData) {
    try {
        console.log(`[STREAMING] Type: ${channelData.streamType}, Text length: ${text ? text.length : 0}`);
        
        const isStreamFinal = channelData.streamType === StreamType.Final;

        if (isStreamFinal) {
            if (!text || text.trim().length === 0) {
                console.log("[STREAMING] Warning: Final message is empty, not sending card");
                await context.send("I apologize, but I wasn't able to generate a response. Please try again.");
                return "streaming-id";
            }

            // Send final message with adaptive card
            const cardJson = {
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.5",
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "TextBlock",
                        "wrap": true,
                        "text": text
                    }
                ]
            };

            console.log("[STREAMING] Sending final card with text:", text.substring(0, 100) + "...");
            await context.send({
                type: 'message',
                attachments: [{
                    contentType: "application/vnd.microsoft.card.adaptive",
                    content: cardJson
                }]
            });
        } else if (channelData.streamType === StreamType.Informative) {
            // Send informative message
            console.log("[STREAMING] Sending informative message:", text);
            await context.send(text);
        } else {
            // Send typing indicator for ongoing streams
            console.log("[STREAMING] Sending typing indicator");
            await context.send({ type: 'typing' });
        }
        return "streaming-id"; // Return a simple ID
    } catch (error) {
        console.error("Error sending streaming activity:", error);
        await context.send("Error while sending streaming activity: " + error.message);
        return null;
    }
}

// Handle incoming messages
app.on("message", async (context) => {
    const activity = context.activity;
    const text = stripMentionsText(activity);

    // Handle installation/welcome message
    if (activity.type === 'installationUpdate' || activity.type === 'conversationUpdate') {
        if (activity.conversation.conversationType === 'channel') {
            await context.send("Welcome to Streaming demo bot! Unfortunately, streaming is not yet available for channels or group chats.");
        } else {
            await context.send("Welcome to Streaming demo bot! You can ask me a question and I'll do my best to answer it.");
        }
        return;
    }

    // Initialize Azure OpenAI client
    const endpoint = config.AzureOpenAIEndpoint;
    const key = config.AzureOpenAIKey;
    const deployment = config.AzureOpenAIDeployment;

    if (!endpoint || !key || !deployment) {
        await context.send("Azure OpenAI configuration is missing. Please set AzureOpenAIEndpoint, AzureOpenAIKey, and AzureOpenAIDeployment in your environment variables.");
        return;
    }

    const client = new OpenAI({
        apiKey: key,
        baseURL: `${endpoint}/openai/deployments/${deployment}`,
        defaultQuery: { 'api-version': '2024-10-21' },
        defaultHeaders: {
            'api-key': key,
        },
    });
    
    console.log(`[OPENAI] Endpoint: ${endpoint}`);
    console.log(`[OPENAI] Deployment: ${deployment}`);
    console.log(`[OPENAI] BaseURL: ${endpoint}/openai/deployments/${deployment}`);
    
    const userInput = text.trim().toLowerCase();
    
    try {
        let contentBuilder = '';
        let streamSequence = 1;
        const rps = 1000; // 1 RPS (Requests per second)
        
        // Send initial informative message
        let channelData = new ChannelData({
            streamType: StreamType.Informative,
            streamSequence: streamSequence,
        });

        await buildAndSendStreamingActivity(context, "Getting the information...", channelData);

        // Make the OpenAI API request with streaming enabled
        console.log(`[OPENAI] Making request with user input: ${userInput}`);
        const events = await client.chat.completions.create({
            model: deployment,  // For Azure OpenAI, this is the deployment name
            messages: [
                { role: "system", content: 'You are an AI great at storytelling which creates compelling fantastical stories.' },
                { role: "user", content: userInput }
            ],
            temperature: 0.7,
            frequency_penalty: 0,
            presence_penalty: 0,
            stream: true,
        });
        
        console.log(`[OPENAI] Stream created, starting to process events...`);

        const stopwatch = new Date();
        let lastUpdateTime = new Date();
        let isStreamComplete = false;

        // Iterate over the streamed events from OpenAI
        let eventCount = 0;
        for await (const event of events) {
            eventCount++;
            console.log(`[OPENAI] Event ${eventCount}:`, JSON.stringify(event, null, 2));
            
            if (isStreamComplete) {
                console.log(`[OPENAI] Stream already complete, skipping event ${eventCount}`);
                break;
            }
            
            if (event.choices && event.choices.length > 0) {
                for (const choice of event.choices) {
                    console.log(`[OPENAI] Processing choice with finish_reason: ${choice.finish_reason}`);
                    
                    // Append the streamed content first
                    if (choice.delta && choice.delta.content) {
                        console.log(`[OPENAI] Adding content: "${choice.delta.content}"`);
                        contentBuilder += choice.delta.content;
                    }

                    // Check if streaming is finished (use finish_reason, not finishReason)
                    if (choice.finish_reason !== null && choice.finish_reason !== undefined) {
                        console.log(`[OPENAI] Stream finished with reason: ${choice.finish_reason}`);
                        channelData.streamType = StreamType.Final;
                        await buildAndSendStreamingActivity(context, contentBuilder, channelData);
                        isStreamComplete = true;
                        break;
                    }

                    // Send periodic updates every 2 seconds or when we have substantial content
                    //const now = new Date();
                    //if (contentBuilder.length > 50 && (now - lastUpdateTime) > 2000) {
                        //await context.send(`Generating response... (${contentBuilder.length} characters so far)`);
                        //lastUpdateTime = now;
                    //}
                }
            }
        }
        
        console.log(`[OPENAI] Stream processing complete. Total events: ${eventCount}, Content length: ${contentBuilder.length}`);

        // Ensure we send final response even if loop exits without finishReason
        if (!isStreamComplete && contentBuilder.length > 0) {
            channelData.streamType = StreamType.Final;
            await buildAndSendStreamingActivity(context, contentBuilder, channelData);
        }
    } catch (error) {
        console.error("Error during streaming:", error);
        await context.send(error.message || "An error occurred during streaming.");
    }
});

module.exports = app;
