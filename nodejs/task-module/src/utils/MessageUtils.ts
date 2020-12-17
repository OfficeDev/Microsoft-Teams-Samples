// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

import urlJoin = require("url-join");
import * as builder from "botbuilder";
import * as utils from ".";
import * as request from "request";
import * as logger from "winston";

// Helpers for working with messages

// Creates a new Message
// Unlike the botbuilder constructor, this defaults the textFormat to "xml"
// tslint:disable-next-line:typedef
export function createMessage(session: builder.Session, text = "", textFormat = "xml"): builder.Message {
    return new builder.Message(session)
        .text(text)
        .textFormat("xml");
}

// Get the channel id in the event
export function getChannelId(event: builder.IEvent): string {
    let sourceEvent = (event.sourceEvent !== undefined) ? event.sourceEvent : event;
    if (sourceEvent && sourceEvent.channel) {
        return sourceEvent.channel.id;
    }

    return "";
}

// Get the team id in the event
export function getTeamId(event: builder.IEvent): string {
    let sourceEvent = (event.sourceEvent !== undefined) ? event.sourceEvent : event;
    if (sourceEvent && sourceEvent.team) {
        return sourceEvent.team.id;
    }
    return "";
}

// Get the tenant id in the event
export function getTenantId(event: builder.IEvent): string {
    let sourceEvent = (event.sourceEvent !== undefined) ? event.sourceEvent : event;
    if (sourceEvent && sourceEvent.tenant) {
        return sourceEvent.tenant.id;
    }
    return "";
}

// Returns true if this is message sent to a channel
export function isChannelMessage(event: builder.IEvent): boolean {
    return !!getChannelId(event);
}

// Returns true if this is message sent to a group (group chat or channel)
export function isGroupMessage(event: builder.IEvent): boolean {
    return event.address.conversation.isGroup || isChannelMessage(event);
}

// Strip all mentions from text
export function getTextWithoutMentions(message: builder.IMessage): string {
    let text = message.text;
    if (message.entities) {
        message.entities
            .filter(entity => entity.type === "mention")
            .forEach(entity => {
                text = text.replace(entity.text, "");
            });
        text = text.trim();
    }
    return text;
}

// Get all user mentions
export function getUserMentions(message: builder.IMessage): builder.IEntity[] {
    let entities = message.entities || [];
    let botMri = message.address.bot.id.toLowerCase();
    return entities.filter(entity => (entity.type === "mention") && (entity.mentioned.id.toLowerCase() !== botMri));
}

// Create a mention entity for the user that sent this message
export function createUserMention(message: builder.IMessage): builder.IEntity {
    let user = message.address.user;
    let text = "<at>" + user.name + "</at>";
    let entity = {
        type: "mention",
        mentioned: user,
        entity: text,
        text: text,
    };
    return entity;
}

// Gets the members of the given conversation.
// Parameters:
//      chatConnector: Chat connector instance.
//      address: Chat connector address. "serviceUrl" property is required.
//      conversationId: [optional] Conversation whose members are to be retrieved, if not specified, the id is taken from address.conversation.
// Returns: A list of conversation members.
export async function getConversationMembers(chatConnector: builder.ChatConnector, address: builder.IChatConnectorAddress, conversationId?: string): Promise<any[]> {
    // Build request
    conversationId = conversationId || address.conversation.id;
    let options: request.Options = {
        method: "GET",
        // We use urlJoin to concatenate urls. url.resolve should not be used here,
        // since it resolves urls as hrefs are resolved, which could result in losing
        // the last fragment of the serviceUrl
        url: urlJoin(address.serviceUrl, `/v3/conversations/${conversationId}/members`),
        json: true,
    };

    let response = await sendRequestWithAccessToken(chatConnector, options);
    if (response) {
        return response;
    } else {
        throw new Error("Failed to get conversation members.");
    }
}

// Starts a 1:1 chat with the given user.
// Parameters:
//      chatConnector: Chat connector instance.
//      address: Chat connector address. "bot", "user" and "serviceUrl" properties are required.
//      channelData: Channel data object. "tenant" property is required.
// Returns: A copy of "address", with the "conversation" property referring to the 1:1 chat with the user.
export async function startConversation(chatConnector: builder.ChatConnector, address: builder.IChatConnectorAddress, channelData: any): Promise<builder.IChatConnectorAddress> {
    // Build request
    let options: request.Options = {
        method: "POST",
        // We use urlJoin to concatenate urls. url.resolve should not be used here,
        // since it resolves urls as hrefs are resolved, which could result in losing
        // the last fragment of the serviceUrl
        url: urlJoin(address.serviceUrl, "/v3/conversations"),
        body: {
            bot: address.bot,
            members: [address.user],
            channelData: channelData,
        },
        json: true,
    };

    let response = await sendRequestWithAccessToken(chatConnector, options);
    if (response && response.hasOwnProperty("id")) {
        return createAddressFromResponse(address, response);
    } else {
        throw new Error("Failed to start conversation: no conversation ID returned.");
    }
}

// Starts a new reply chain by posting a message to a channel.
// Parameters:
//      chatConnector: Chat connector instance.
//      message: The message to post. The address in this message is ignored, and the message is posted to the specified channel.
//      channelId: Id of the channel to post the message to.
// Returns: A copy of "message.address", with the "conversation" property referring to the new reply chain.
export async function startReplyChain(chatConnector: builder.ChatConnector, message: builder.Message, channelId: string): Promise<builder.IChatConnectorAddress> {
    let activity = message.toMessage();

    // Build request
    let options: request.Options = {
        method: "POST",
        // We use urlJoin to concatenate urls. url.resolve should not be used here,
        // since it resolves urls as hrefs are resolved, which could result in losing
        // the last fragment of the serviceUrl
        url: urlJoin((activity.address as any).serviceUrl, "/v3/conversations"),
        body: {
            isGroup: true,
            activity: activity,
            channelData: {
                teamsChannelId: channelId,
            },
        },
        json: true,
    };

    let response = await sendRequestWithAccessToken(chatConnector, options);
    if (response && response.hasOwnProperty("id")) {
        let address = createAddressFromResponse(activity.address, response) as any;
        if (address.user) {
            delete address.user;
        }
        if (address.correlationId) {
            delete address.correlationId;
        }
        return address;
    } else {
        throw new Error("Failed to start reply chain: no conversation ID returned.");
    }
}

// Send an authenticated request
async function sendRequestWithAccessToken(chatConnector: builder.ChatConnector, options: request.OptionsWithUrl): Promise<any> {
    // Add access token
    await addAccessToken(chatConnector, options);

    // Execute request
    return new Promise<any>((resolve, reject) => {
        request(options, (err, response, body) => {
            if (err) {
                reject(err);
            } else {
                if (response.statusCode < 400) {
                    try {
                        let result = typeof body === "string" ? JSON.parse(body) : body;
                        resolve(result);
                    } catch (e) {
                        reject(e instanceof Error ? e : new Error(e.toString()));
                    }
                } else {
                    let txt = "Request to '" + options.url + "' failed: [" + response.statusCode + "] " + response.statusMessage;
                    reject(new Error(txt));
                }
            }
        });
    });
}

// Add access token to request options
function addAccessToken(chatConnector: builder.ChatConnector, options: request.Options): Promise<void> {
    return new Promise<void>((resolve, reject) => {
        // ChatConnector type definition doesn't include getAccessToken
        (chatConnector as any).getAccessToken((err: any, token: string) => {
            if (err) {
                reject(err);
            } else {
                options.headers = {
                    "Authorization": "Bearer " + token,
                };
                resolve();
            }
        });
    });
}

// Create a copy of address with the data from the response
function createAddressFromResponse(address: builder.IChatConnectorAddress, response: any): builder.IChatConnectorAddress {
    let result = {
        ...address,
        conversation: { id: response["id"] },
        useAuth: true,
    };
    if (result.id) {
        delete result.id;
    }
    if (response["activityId"]) {
        result.id = response["activityId"];
    }
    return result;
}

// Get locale from client info in event
export function getLocale(evt: builder.IEvent): string {
    let event = (evt as any);
    if (event.entities && event.entities.length) {
        let clientInfo = event.entities.find(e => e.type && e.type === "clientInfo");
        return clientInfo.locale;
    }
    return null;
}

// Load a Session corresponding to the given event
export function loadSessionAsync(bot: builder.UniversalBot, event: builder.IEvent): Promise<builder.Session> {
    return new Promise((resolve, reject) => {
        bot.loadSession(event.address, (err: any, session: builder.Session) => {
            if (err) {
                logger.error("Failed to load session", { error: err, address: event.address });
                reject(err);
            } else if (!session) {
                logger.error("Loaded null session", { address: event.address });
                reject(new Error("Failed to load session"));
            } else {
                let locale = getLocale(event);
                if (locale) {
                    (session as any)._locale = locale;
                    session.localizer.load(locale, (err2) => {
                        // Log errors but resolve session anyway
                        if (err2) {
                            logger.error(`Failed to load localizer for ${locale}`, err2);
                        }
                        resolve(session);
                    });
                } else {
                    resolve (session);
                }
            }
        });
    });
}

// Helper function that mimics the getContext() call in the client SDK
// @param BotBuilder.IEvent; if null, session.sourceEvent is used
// @param BotBuilder.Session (used for full messages)
//
// Function uses either an event or a session object; if both are provided, the function returns an error string
export function getContext(event: builder.IEvent, session?: builder.Session): any {
    let entities: any = null;
    if ((event !== undefined) && (session !== undefined)) {
        return "Error: getContext() takes a builder.IEvent or builder.Session object as parameters, but not both.";
    }
    if (event === null) {
        if (session === undefined) {
            // Nothing was passed, return null
            return null;
        }
        else {
            // Use sourceEvent and entities array from the session object
            event = session.message.sourceEvent;
            entities = session.message.entities;
        }
    }
    else {
        // Use entities array from the event object
        entities = (event as any).entities;
    }
    // Define context object and populate as much as possible using existing helper functions
    let context: any = {
        locale: null,
        country: null,
        platform: null,
        timezone: null,
        tenant: getTenantId(event),
        teamsChannelId: getChannelId(event),
        teamsTeamId: getTeamId(event),
        userObjectId: null,
        messageId: null,
        conversationId: null,
        conversationType: null,
        theme: null,
    };
    // Populate locale, country, platform, timezone
    if (entities !== undefined && entities.length) {
        let clientInfo = entities.find(e => e.type && e.type === "clientInfo");
        context.locale = clientInfo.locale;
        context.country = clientInfo.country;
        context.platform = clientInfo.platform;
        context.timezone = clientInfo.timezone;
    }
    if (session !== undefined) {
        // We have a builder.Session object
        context.userObjectId = (session.message.address.user as any).aadObjectId;
        context.messageId = (session.message.address as any).id;
        context.conversationId = session.message.address.conversation.id;
        context.conversationType = session.message.address.conversation.conversationType;
    }
    else {
        // We have a builder.IEvent object
        context.userObjectId = (event.address.user as any).aadObjectId;
        context.messageId = (event.address as any).id;
        context.conversationId = event.address.conversation.id;
        context.conversationType = event.address.conversation.conversationType;
    }

    return context;
}
