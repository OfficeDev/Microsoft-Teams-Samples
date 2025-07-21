const { ActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const { StreamType, ChannelData } = require('./streamingModels'); // Models for streaming

// Main bot class extending ActivityHandler
class TeamsStreamingBot extends ActivityHandler {
    constructor() {
        super();

        // Initialize bot credentials and Azure OpenAI configuration
        this._appId = process.env.MicrosoftAppId;
        this._appPassword = process.env.MicrosoftAppPassword;
        this._appTenantId = process.env.MicrosoftAppTenantId;
        this._endpoint = process.env.AzureOpenAIEndpoint; // Azure OpenAI Endpoint (e.g., https://<resource-name>.openai.azure.com/)
        this._key = process.env.AzureOpenAIKey; // Azure OpenAI API Key
        this._deployment = process.env.AzureOpenAIDeployment; // Deployment name (model)
    }

    // Handle incoming messages
    async onMessageActivity(turnContext) {
        // Create an instance of OpenAIClient with the given endpoint and API key
        const client = new OpenAIClient(this._endpoint, new AzureKeyCredential(this._key));

        // Get the user input and trim it
        let userInput = turnContext.activity.text.trim().toLowerCase();
        try {
            let contentBuilder = ''; // Initialize content that will be streamed back to the user
            let streamSequence = 1; // Sequence for streaming events
            const rps = 1000; // 1 RPS (Requests per second) - controls streaming rate
            
            // Prepare the initial informative message
            let channelData = new ChannelData({
                streamType: StreamType.Informative, // Indicating this is the start of the stream
                streamSequence: streamSequence,
            });

            // Build and send an initial streaming activity
            let streamId = await this.buildAndSendStreamingActivity(turnContext, "Getting the information...", channelData);

            // Make the OpenAI API request and enable streaming
            const events = await client.streamChatCompletions(
                this._deployment,
                [
                    { role: "system", content: 'You are an AI great at storytelling which creates compelling fantastical stories.' },
                    { role: "user", content: userInput }
                ],
                { 
                    temperature: 0.7, // Controls randomness of output
                    frequency_penalty: 0, // Controls repetition
                    presence_penalty: 0, // Controls appearance of new topics
                    stream: true, // Enable streaming
                },
            );

            // Initialize a stopwatch to manage requests per second
            const stopwatch = new Date();

            // Iterate over the streamed events from OpenAI
            for await (const event of events) {
                streamSequence++; // Increment the sequence for each new chunk of data

                // Loop through the choices in each event
                for (const choice of event.choices) {
                    // If streaming is finished, send the final response and break out of the loop
                    if (choice.finishReason !== null) {
                        channelData.streamType = StreamType.Final; // Mark the stream as finished
                        channelData.streamSequence = streamSequence;
                        channelData.streamId = streamId;
                        
                        await this.buildAndSendStreamingActivity(turnContext, contentBuilder, channelData);
                        break;
                    }

                    // Append the streamed content to the builder
                    if (choice.delta && choice.delta.content) {
                        contentBuilder += choice.delta.content;
                    }

                    // If RPS rate reached, send the current content chunk
                    if (contentBuilder.length > 0 && new Date() - stopwatch > rps) {
                        channelData.streamType = StreamType.Streaming; // Indicating this is a streaming update
                        channelData.streamSequence = streamSequence;
                        channelData.streamId = streamId;

                        await this.buildAndSendStreamingActivity(turnContext, contentBuilder, channelData);
                        stopwatch.setTime(new Date().getTime()); // Reset the stopwatch after sending a chunk
                    }
                }
            }
        } catch (error) {
            // In case of an error, send the error message to the user
            await turnContext.sendActivity(error.message || "An error occurred during streaming.");
        }
    }

    // Build and send a streaming activity (either ongoing or final)
    async buildAndSendStreamingActivity(turnContext, text, channelData) {
        const isStreamFinal = channelData.streamType === StreamType.Final; // Check if this is the final part of the stream

        // Set up the basic streaming activity (either typing or a message)
        const streamingActivity = {
            type: isStreamFinal ? 'message' : 'typing', // 'typing' indicates the bot is working, 'message' when final
            id: channelData.streamId
        };

        // Add the streaming information as an entity
        streamingActivity.entities = [{
            type: 'streaminfo',
            streamId: channelData.streamId,
            streamType: channelData.streamType.toString(),
            streamSequence: channelData.streamSequence
        }];

        // If it's the final stream, attach an AdaptiveCard with the result
        if (isStreamFinal) {
            try {
                var cardJson = {
                    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                    "version": "1.5",
                    "type": "AdaptiveCard",
                    "body": [
                      {
                        "type": "TextBlock",
                        "wrap": true,
                        "text": text // Final text from the streaming process
                      }
                    ]
                };

                // Add the AdaptiveCard to the activity
                streamingActivity.attachments = [CardFactory.adaptiveCard(cardJson)];
            } catch (error) {
                console.error("Error creating adaptive card:", error);
                // If card creation fails, inform the user
                await turnContext.sendActivity("Error while generating the adaptive card.");
            }
        }
        else if (text) {
            // Set text only for non-final (intermediate) messages if needed
            streamingActivity.text = text;
        }

        // Send the streaming activity (either ongoing or final)
        return await this.sendStreamingActivity(turnContext, streamingActivity);
    }

    // Send the streaming activity to the user
    async sendStreamingActivity(turnContext, streamingActivity) {
        try {
            const response = await turnContext.sendActivity(streamingActivity);
            return response.id; // Return the activity ID for tracking
        } catch (error) {
            // If an error occurs during sending, inform the user
            await turnContext.sendActivity(MessageFactory.text("Error while sending streaming activity: " + error.message));
            throw new Error("Error sending activity: " + error.message); // Propagate error
        }
    }

    // Handle installation updates (e.g., when the bot is installed or added to a team)
    async onInstallationUpdateActivity(turnContext) {
        // Check if the activity is from a channel (group chat) or one-on-one
        if (turnContext.activity.conversation.conversationType === 'channel') {
            // Streaming is not yet supported in channels or group chats
            await turnContext.sendActivity("Welcome to Streaming demo bot! Unfortunately, streaming is not yet available for channels or group chats.");
        } else {
            // In one-on-one conversations, the bot can be used
            await turnContext.sendActivity("Welcome to Streaming demo bot! You can ask me a question and I'll do my best to answer it.");
        }
    }
}

// Export the bot class
module.exports.TeamsStreamingBot = TeamsStreamingBot;
