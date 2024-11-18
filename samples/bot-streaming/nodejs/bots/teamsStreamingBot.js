const { ActivityHandler, MessageFactory } = require('botbuilder');
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const fs = require('fs');
const path = require('path');
const { StreamType, ChannelData } = require('./streamingModels'); // Models for streaming

class TeamsStreamingBot extends ActivityHandler {
    constructor() {
        super();

        this._appId = process.env.MicrosoftAppId;
        this._appPassword = process.env.MicrosoftAppPassword;
        this._appTenantId = process.env.MicrosoftAppTenantId;
        this._endpoint = process.env.AzureOpenAIEndpoint; // Azure OpenAI Endpoint (e.g., https://<resource-name>.openai.azure.com/)
        this._key = process.env.AzureOpenAIKey; // Azure OpenAI API Key
        this._deployment = process.env.AzureOpenAIDeployment; // Deployment name (model)
        this.adaptiveCardTemplate = path.join(__dirname, "..", "Resources", "CardTemplate.json");
    }

    async onMessageActivity(turnContext) {
        // Create an instance of OpenAIClient with the given endpoint and API key
        const client = new OpenAIClient(this._endpoint, new AzureKeyCredential(this._key));

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
             const events = await client.streamChatCompletions(
                this._deployment,
                [
                    { role: "system", content: 'You are an AI great at storytelling which creates compelling fantastical stories.' },
                    { role: "user", content: userInput }
                ],
                { 
                    temperature: 0.7,
                    frequency_penalty: 0,
                    presence_penalty: 0,
                    stream: true, // Enable streaming
                },
            );

            const stopwatch = new Date();

            for await (const event of events) {
                streamSequence++;

                for (const choice of event.choices) {
                    console.log(choice);
                    console.log(choice.delta?.content);
                    if (choice.finishReason !== null)
                    {
                        channelData.streamType = StreamType.Final;
                        channelData.streamSequence = streamSequence;
                        channelData.streamId = streamId;
                        
                        await this.buildAndSendStreamingActivity(turnContext, contentBuilder, channelData);
                        break;
                    }
                   
                    if (choice.delta && choice.delta.content) {
                        contentBuilder += choice.delta.content;
                    }

                    // Send chunks once RPS is reached
                    if (contentBuilder.length > 0 && new Date() - stopwatch > rps) 
                        {
                            channelData.streamType = StreamType.Streaming;
                            channelData.streamSequence = streamSequence;
                            channelData.streamId = streamId;

                            await this.buildAndSendStreamingActivity(turnContext, contentBuilder, channelData);
                            stopwatch.setTime(new Date().getTime()); // Restart the stopwatch
                        }
                }
            }
        } catch (error) {
            await turnContext.sendActivity(error.event);
        }
    }

    async buildAndSendStreamingActivity(turnContext, text, channelData) {
        const isStreamFinal = channelData.streamType === StreamType.Final;

        const streamingActivity = {
            type: isStreamFinal ? 'message' : 'typing',
            id: channelData.streamId,
            channelData: channelData,
        };

        if (text) {
            streamingActivity.text = text;
        }

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
            try {
                const templateString = fs.readFileSync(this.adaptiveCardTemplate, 'utf8');
                const AdaptiveCards = require('adaptivecards-templating');
                const template = new AdaptiveCards.Template(templateString);
    
                const cardData = { finalStreamText: text };
                const adaptiveCardContent = template.expand(cardData);
    
                const attachment = {
                    contentType: "application/vnd.microsoft.card.adaptive",
                    content: JSON.parse(adaptiveCardContent),
                };
                streamingActivity.attachments = [attachment];
                streamingActivity.text = "This is what I've got:";
            } catch (error) {
                console.error("Error creating adaptive card:", error);
                await turnContext.sendActivity("Error while generating the adaptive card.");
            }
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

module.exports.TeamsStreamingBot = TeamsStreamingBot;
