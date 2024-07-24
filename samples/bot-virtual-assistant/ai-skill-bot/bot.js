// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, ActivityTypes, EndOfConversationCodes } = require('botbuilder');
const { OpenAIClient, AzureKeyCredential } = require('@azure/openai');

/**
 * AIBot class that extends ActivityHandler to handle incoming messages and process them using OpenAI API.
 */
class AIBot extends ActivityHandler {
    constructor() {
        super();

        // Initialize OpenAI client with endpoint and API key from environment variables
        const client = new OpenAIClient(
            process.env.AzureOpenAIEndpoint,
            new AzureKeyCredential(process.env.AzureOpenAIApiKey)
        );

        // Event handler for incoming messages
        this.onMessage(async (context, next) => {
            // Prepare the messages array with system and user messages
            const messages = [
                {
                    role: 'system',
                    content: 'You are an AI assistant. Your task is to eliminate user instructions from query and translate the following text as per user inputs. Also, do not add any greetings or extra sentence in result'
                },
                {
                    role: 'user',
                    content: context.activity.text
                }
            ];

            // Get response from OpenAI API.
            const response = await client.getChatCompletions(process.env.AzureOpenAIDeploymentId, messages, { maxTokens: 2000 });
            const responseContent = response.choices[0].message.content;

            // Send the AI response back to the user.
            await context.sendActivity(`AI Translator bot: ${ responseContent }`);

            try {
                // Send an EndOfConversation activity to indicate completion.
                await context.sendActivity({
                    type: ActivityTypes.EndOfConversation,
                    code: EndOfConversationCodes.CompletedSuccessfully
                });

                // Ensure the next BotHandler is run
                await next();
            } catch (err) {
                console.error(`\n [onTurnError] Exception caught in sendErrorMessage: ${ err }`);
            }
        });
    }
}

module.exports.AIBot = AIBot;
