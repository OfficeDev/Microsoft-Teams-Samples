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

import * as builder from "botbuilder";

// Simple middleware to save a mapping of AAD object ID -> Teams IDs (user ID, conversation ID, service URL)
// This is a memory-based store for demonstration purposes only
export class UserMappingMiddleware {

    constructor(
        private aadObjectIdToBotUserMap: Map<string, any>, 
    )
    { }

    public async onTurn(context: builder.TurnContext, next) {
        if (!context) {
            throw new Error('Context is null');
        };

        const activity = context.activity;
        if (activity.channelId === "msteams" && 
            (activity.type === "conversationUpdate" || activity.type === "message" || activity.type === "invoke")) {
            this.aadObjectIdToBotUserMap.set(activity.from.aadObjectId, 
                { userId: activity.from.id, conversationId: activity.conversation.id, serviceUrl: activity.serviceUrl });
        }

        await next();
    };
}
