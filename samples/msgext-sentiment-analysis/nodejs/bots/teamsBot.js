// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require("botbuilder");
const { Configuration, OpenAIApi } = require("openai");
var textToAnalyze = "";
var sentimentResponse = "";

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;

            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome. This sample helps you to send sentiment for any text message");
                }
            }

            await next();
        });
    }

    // fetch and handle message extension task module 
    async handleTeamsMessagingExtensionFetchTask(context, action) {

        textToAnalyze = action.messagePayload.body.content;
        textToAnalyze = textToAnalyze.replace(/(<([^>]+)>)/ig, '');

        // Configure your apikey
        const configuration = new Configuration({
            apiKey: process.env.apiKey
        });

        // Updaitng configuration with OpenAI API
        const openai = new OpenAIApi(configuration);

        const analyzeSentiment = async (tweet) => {
            try {
                const messages = [
                    {
                        role: 'system',
                        content: `You will be provided with a tweet, and your task is to classify its sentiment as positive, neutral, or negative`,
                    },
                ]

                // CreateChatCompletion api call
                const response = await openai.createChatCompletion({
                    model: "gpt-3.5-turbo",
                    messages: messages,
                    temperature: 0,
                    max_tokens: 256,
                    top_p: 1,
                    frequency_penalty: 0,
                    presence_penalty: 0,
                });

                // Extract the AI's response from the API call
                const aiResponse = response.data.choices[0].message.content.trim();

                return aiResponse;
            }
            catch (error) {
                console.error('Error:', error);
                throw error;
            }
        };

        // get sentiment response for selected teams message
        await analyzeSentiment(textToAnalyze)
            .then(sentiment =>
                sentimentResponse = sentiment
            ).catch(err => console.error('Error:', err));

        // return sentiment analysis result to task module
        return {
            task: {
                type: 'continue',
                value: {
                    width: 600,
                    height: 400,
                    title: 'Sentiment Analysis',
                    url: this.baseUrl + "/sentimentModule?title=" + textToAnalyze + "&result=" + sentimentResponse
                }
            }
        };
    }
}

module.exports.TeamsBot = TeamsBot;