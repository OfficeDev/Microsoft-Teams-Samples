// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require("botbuilder");
const { OpenAI } = require("openai");

class SentimentAnalysis extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;

        this.onMembersAdded(async (context, next) => {
            await this.sendWelcomeMessage(context);
            await next();
        });
    }

    async sendWelcomeMessage(turnContext) {
        const { activity } = turnContext;
        for (const member of activity.membersAdded) {
            if (member.id !== activity.recipient.id) {
                await turnContext.sendActivity("Welcome to the Sentiment Analysis Sample. This example demonstrates how to analyze selected text and classify its sentiment as positive, negative, or neutral.");
            }
        }
    }

    async handleTeamsMessagingExtensionFetchTask(context, action) {
        let textToAnalyze = action.messagePayload.body.content.replace(/(<([^>]+)>)/ig, '');
        console.log("Text to analyze:", textToAnalyze);

        const openai = new OpenAI({
            apiKey: process.env.SECRET_OPENAI_API_KEY
        });

        let sentimentResponse = "Not available";

        try {
            const response = await openai.chat.completions.create({
                model: process.env.CHAT_COMPLETION_MODEL_NAME || "gpt-3.5-turbo",
                messages: [
                    {
                        role: 'system',
                        content: "You will be provided with a message, and your task is to classify its sentiment as Positive, Neutral, or Negative. Only respond with one of these three words."
                    },
                    {
                        role: 'user',
                        content: textToAnalyze
                    }
                ],
                temperature: 0
            });

            sentimentResponse = response.choices[0].message.content.trim();
           console.log("Sentiment result:", sentimentResponse);

        } catch (error) {
            console.error("OpenAI error:", error.message);
        }

        const url = `${this.baseUrl}/sentimentModule?title=${encodeURIComponent(textToAnalyze)}&result=${encodeURIComponent(sentimentResponse)}`;

        return {
            task: {
                type: 'continue',
                value: {
                    width: 600,
                    height: 400,
                    title: 'Sentiment Analysis',
                    url: url,
                    fallbackUrl: url
                }
            }
        };
    }

    async run(context) {
        await super.run(context);
    }
}

module.exports.SentimentAnalysis = SentimentAnalysis;

