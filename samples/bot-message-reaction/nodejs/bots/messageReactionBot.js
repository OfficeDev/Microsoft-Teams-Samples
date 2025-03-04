// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    ActivityHandler,
    MessageFactory
} = require('botbuilder');

/**
 * MessageReactionBot handles incoming messages and reactions in Microsoft Teams.
 * @class
 */
class MessageReactionBot extends ActivityHandler {
    /**
     * Creates an instance of MessageReactionBot.
     * @param {ActivityLog} activityLog - The activity log for tracking sent messages.
     */
    constructor(activityLog) {
        super();
        this._log = activityLog;
        this.onMessage(async (context, next) => {
            await this._sendMessageAndLogActivityId(context, `echo: ${ context.activity.text }`);
            await next();
        });
    }

    /**
     * Handles reactions added to messages.
     * @param {Array} reactionsAdded - The reactions added to the message.
     * @param {Object} context - The context of the activity.
     */
    async onReactionsAddedActivity(reactionsAdded, context) {
        for (const reaction of reactionsAdded) {
            const activity = await this._log.find(context.activity.replyToId);
            if (!activity) {
                // If we had sent the message from the error handler we wouldn't have recorded the Activity Id and so we shouldn't expect to see it in the log.
                await this._sendMessageAndLogActivityId(context, `Activity ${ context.activity.replyToId } not found in the log.`);
            } else {
                await this._sendMessageAndLogActivityId(context, ` added '${ reaction.type }' regarding '${ activity.text }'`);
            }
        }
    }

    /**
     * Handles reactions removed from messages.
     * @param {Array} reactionsRemoved - The reactions removed from the message.
     * @param {Object} context - The context of the activity.
     */
    async onReactionsRemovedActivity(reactionsRemoved, context) {
        for (const reaction of reactionsRemoved) {
            // The ReplyToId property of the inbound MessageReaction Activity will correspond to a Message Activity that was previously sent from this bot.
            const activity = await this._log.find(context.activity.replyToId);
            if (!activity) {
                // If we had sent the message from the error handler we wouldn't have recorded the Activity Id and so we shouldn't expect to see it in the log.
                await this._sendMessageAndLogActivityId(context, `Activity ${ context.activity.replyToId } not found in the log.`);
            } else {
                await this._sendMessageAndLogActivityId(context, `You removed '${ reaction.type }' regarding '${ activity.text }'`);
            }
        }
    }

    /**
     * Sends a message and logs the activity ID.
     * @param {Object} context - The context of the activity.
     * @param {string} text - The text of the message to send.
     * @private
     */
    async _sendMessageAndLogActivityId(context, text) {
        const replyActivity = MessageFactory.text(text);
        const resourceResponse = await context.sendActivity(replyActivity);
        await this._log.append(resourceResponse.id, replyActivity);
    }
}

module.exports.MessageReactionBot = MessageReactionBot;
