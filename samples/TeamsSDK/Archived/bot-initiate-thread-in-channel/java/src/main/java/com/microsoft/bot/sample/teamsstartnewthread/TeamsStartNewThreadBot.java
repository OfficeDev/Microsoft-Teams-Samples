// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

package com.microsoft.bot.sample.teamsstartnewthread;

import java.util.concurrent.CompletableFuture;

import com.fasterxml.jackson.databind.node.JsonNodeFactory;
import com.fasterxml.jackson.databind.node.ObjectNode;
import com.microsoft.bot.builder.BotFrameworkAdapter;
import com.microsoft.bot.builder.MessageFactory;
import com.microsoft.bot.builder.TurnContext;
import com.microsoft.bot.builder.teams.TeamsActivityHandler;
import com.microsoft.bot.connector.authentication.MicrosoftAppCredentials;
import com.microsoft.bot.integration.Configuration;
import com.microsoft.bot.schema.Activity;
import com.microsoft.bot.schema.ConversationParameters;
import com.microsoft.bot.schema.ConversationReference;

/**
 * Bot that handles the creation of a new thread in a Teams channel when a message is received.
 */
public class TeamsStartNewThreadBot extends TeamsActivityHandler {

    private String appId;
    private String appPassword;

    /**
     * Constructor that initializes the bot with Microsoft App credentials.
     * 
     * @param configuration The configuration object that holds the app credentials.
     */
    public TeamsStartNewThreadBot(Configuration configuration) {
        this.appId = configuration.getProperty("MicrosoftAppId");
        this.appPassword = configuration.getProperty("MicrosoftAppPassword");
    }

    /**
     * This method is triggered when a message activity is received.
     * It creates a new thread in the specified Teams channel and sends a reply.
     *
     * @param turnContext The context for the current turn of the bot.
     * @return A CompletableFuture to indicate the asynchronous operation completion.
     */
    @Override
    protected CompletableFuture<Void> onMessageActivity(TurnContext turnContext) {
        // Extract necessary information from the incoming message
        String teamsChannelId = turnContext.getActivity().teamsGetChannelId();
        String serviceUrl = turnContext.getActivity().getServiceUrl();

        // Create the message that will start the new thread
        Activity message = MessageFactory.text("This will start a new thread in a channel");

        // Prepare Microsoft app credentials
        MicrosoftAppCredentials credentials = new MicrosoftAppCredentials(appId, appPassword);

        // Build the channel data for the new conversation
        ObjectNode channelData = createChannelDataNode(teamsChannelId);

        // Set up the parameters for the new conversation
        ConversationParameters conversationParameters = createConversationParameters(message, channelData);

        // Get the BotFrameworkAdapter from the TurnContext
        BotFrameworkAdapter adapter = (BotFrameworkAdapter) turnContext.getAdapter();

        // Start the new conversation and send the first reply
        return startNewConversation(adapter, teamsChannelId, serviceUrl, credentials, conversationParameters);
    }

    /**
     * Creates the channel data node required for the conversation.
     * 
     * @param teamsChannelId The Teams channel ID.
     * @return The channel data node containing the channel ID.
     */
    private ObjectNode createChannelDataNode(String teamsChannelId) {
        ObjectNode channelData = JsonNodeFactory.instance.objectNode();
        channelData.set("channel", JsonNodeFactory.instance.objectNode().set("id", JsonNodeFactory.instance.textNode(teamsChannelId)));
        return channelData;
    }

    /**
     * Creates the conversation parameters, including the message and channel data.
     * 
     * @param message The message to be sent in the new thread.
     * @param channelData The channel data containing the channel ID.
     * @return The conversation parameters containing all the required information.
     */
    private ConversationParameters createConversationParameters(Activity message, ObjectNode channelData) {
        ConversationParameters conversationParameters = new ConversationParameters();
        conversationParameters.setIsGroup(true);  // Set the conversation as a group
        conversationParameters.setActivity(message);
        conversationParameters.setChannelData(channelData);
        return conversationParameters;
    }

    /**
     * Starts a new conversation and sends the first reply in the thread.
     * 
     * @param adapter The BotFrameworkAdapter to handle the conversation.
     * @param teamsChannelId The Teams channel ID where the thread should be created.
     * @param serviceUrl The service URL for the Teams channel.
     * @param credentials The MicrosoftAppCredentials for authentication.
     * @param conversationParameters The parameters for the new conversation.
     * @return A CompletableFuture indicating the asynchronous operation completion.
     */
    private CompletableFuture<Void> startNewConversation(
        BotFrameworkAdapter adapter,
        String teamsChannelId,
        String serviceUrl,
        MicrosoftAppCredentials credentials,
        ConversationParameters conversationParameters
    ) {
        // Attempt to create the conversation and send the first response
        try {
            return adapter.createConversation(teamsChannelId, serviceUrl, credentials, conversationParameters, (tc) -> {
                ConversationReference reference = tc.getActivity().getConversationReference();
                return tc.getAdapter().continueConversation(appId, reference, (continue_tc) -> {
                    // Ensure that the response is void by applying .thenApply()
                    return continue_tc.sendActivity(MessageFactory.text("This will be the first response to the new thread"))
                            .thenApply(resourceResponse -> null);  // Convert the response to Void
                }).thenApply(resourceResponse -> null);  // No further processing needed
            }).thenApply(started -> null);  // Completion of the conversation creation
        } catch (Exception e) {
            // Log and handle any errors that occur during conversation creation
            e.printStackTrace();
            return CompletableFuture.completedFuture(null);  // Return a completed future in case of error
        }
    }
}
