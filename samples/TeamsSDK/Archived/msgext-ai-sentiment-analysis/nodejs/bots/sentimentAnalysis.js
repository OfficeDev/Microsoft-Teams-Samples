// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require("botbuilder");
const { AzureOpenAI } = require("openai");
const { DefaultAzureCredential, getBearerTokenProvider } = require("@azure/identity");

class SentimentAnalysis extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;

        // Initialize Azure OpenAI client using Entra ID bearer token auth
        const credential = new DefaultAzureCredential();
        const scope = "https://cognitiveservices.azure.com/.default";
        const azureADTokenProvider = getBearerTokenProvider(credential, scope);

        this.openai = new AzureOpenAI({
            azureADTokenProvider,
            endpoint: process.env.AZURE_OPENAI_ENDPOINT,
            apiVersion: process.env.AZURE_OPENAI_API_VERSION || "2025-01-01-preview"
        });

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
        const textToAnalyze = action.messagePayload.body.content.replace(/(<([^>]+)>)/ig, '');
        console.log("Text to analyze:", textToAnalyze);

        let sentimentResponse = "Not available";

        try {
            const response = await this.openai.chat.completions.create({
                model: process.env.CHAT_COMPLETION_MODEL_NAME || "gpt-5-mini",
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
            console.error("Azure OpenAI error:", error.message);
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
}

module.exports.SentimentAnalysis = SentimentAnalysis;

