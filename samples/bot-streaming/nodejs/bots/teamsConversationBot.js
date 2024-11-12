const { ActivityHandler, MessageFactory } = require('botbuilder');
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const fs = require('fs');
const path = require('path');
const { StreamType, ChannelData } = require('./streamingModels'); // Models for streaming
const { AdaptiveCardTemplate } = require('adaptivecards-templating'); // For Adaptive Card templating

class TeamsConversationBot extends ActivityHandler {
    constructor(config) {
        super();

        this._appId = process.env.MicrosoftAppId;
        this._appPassword = process.env.MicrosoftAppPassword;
        this._appTenantId = process.env.MicrosoftAppTenantId;
        this._endpoint = process.env.AzureOpenAIEndpoint; // Azure OpenAI Endpoint (e.g., https://<resource-name>.openai.azure.com/)
        this._key = process.env.AzureOpenAIKey; // Azure OpenAI API Key
        this._deployment = process.env.AzureOpenAIDeployment; // Deployment name (model)

        // Initialize Azure OpenAI Client
        const credential = new AzureKeyCredential(this._key);
        this.openAIClient = new OpenAIClient(this._endpoint, credential);
        
        this.adaptiveCardTemplate = path.join(__dirname, "Resources", "CardTemplate.json");
    }

    async onMessageActivity(turnContext) {
        let userInput = turnContext.activity.text.trim().toLowerCase();
        try {
            let contentBuilder = '';
            let streamSequence = 1;
            const rps = 1000; // 1 RPS (Requests per second)
            
            // Prepare the initial informative message
            let channelData = new ChannelData({
                streamType: StreamType.Informative,
                streamSequence: streamSequence,
            });

            let streamId = await this.buildAndSendStreamingActivity(turnContext, "Getting the information...", channelData);

            // Request to Azure OpenAI API to generate a chat response (streaming)
            const response = await this.openAIClient.getChatCompletions(this._deployment, [
                { role: 'system', content: 'You are an AI great at storytelling which creates compelling fantastical stories.' },
                { role: 'user', content: userInput }
            ], {
                streaming: true,
            });

            const stopwatch = new Date();
            for await (const message of response) {
                streamSequence++;
                
                if (message.choices[0].finish_reason) {
                    channelData.streamType = StreamType.Final;
                    await this.buildAndSendStreamingActivity(turnContext, contentBuilder, channelData);
                    break;
                }

                contentBuilder += message.choices[0].delta.content;

                // Send chunks once RPS is reached
                if (contentBuilder.length > 0 && new Date() - stopwatch > rps) {
                    channelData.streamType = StreamType.Streaming;
                    await this.buildAndSendStreamingActivity(turnContext, contentBuilder, channelData);
                    stopwatch.setTime(new Date().getTime()); // Restart the stopwatch
                }
            }
        } catch (error) {
            await turnContext.sendActivity(error.message);
        }
    }

    async buildAndSendStreamingActivity(turnContext, text, channelData) {
        const isStreamFinal = channelData.streamType === StreamType.Final;
        const streamingActivity = {
            type: isStreamFinal ? 'message' : 'typing',
            id: channelData.streamId,
            channelData: channelData,
            text: text,
        };

        // Include streaming info in entities
        streamingActivity.entities = [{
            type: 'streaminfo',
            properties: {
                streamId: channelData.streamId,
                streamType: channelData.streamType.toString(),
                streamSequence: channelData.streamSequence
            }
        }];

        if (isStreamFinal) {
            // Build the adaptive card if it's the final stream
            const template = fs.readFileSync(this.adaptiveCardTemplate, 'utf8');
            const adaptiveCard = new AdaptiveCardTemplate(template);
            const cardData = { finalStreamText: text };
            const attachment = {
                contentType: "application/vnd.microsoft.card.adaptive",
                content: JSON.parse(adaptiveCard.expand(cardData)),
            };
            streamingActivity.attachments = [attachment];
            streamingActivity.text = "This is what I've got:";
        }

        return await this.sendStreamingActivity(turnContext, streamingActivity);
    }

    async sendStreamingActivity(turnContext, streamingActivity) {
        try {
            const response = await turnContext.sendActivity(streamingActivity);
            return response.id;
        } catch (error) {
            console.error("Error while sending streaming activity: ", error);
            await turnContext.sendActivity(MessageFactory.text("Error while sending streaming activity: " + error.message));
            throw new Error("Error sending activity: " + error.message);
        }
    }

    async onInstallationUpdateActivity(turnContext) {
        if (turnContext.activity.conversation.conversationType === 'channel') {
            await turnContext.sendActivity("Welcome to Streaming demo bot! Unfortunately, streaming is not yet available for channels or group chats.");
        } else {
            await turnContext.sendActivity("Welcome to Streaming demo bot! You can ask me a question and I'll do my best to answer it.");
        }
    }
}

module.exports.TeamsConversationBot = TeamsConversationBot;
