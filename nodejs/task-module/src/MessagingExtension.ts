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
import * as msteams from "botbuilder-teams";
import { TeamsBot } from "./TeamsBot";
import * as faker from "faker";

export class MessagingExtension extends TeamsBot {
    constructor(
        bot: TeamsBot,
    )
    {
        super(bot._connector, bot._botSettings);
        // Cast as an msTeams.TeamsChatConnector and attach the onQuery event
        (this._connector as msteams.TeamsChatConnector).onQuery("getRandomText", this.generateRandomResponse);
    }

    private generateRandomResponse(event: builder.IEvent, query: msteams.ComposeExtensionQuery, callback: any): void {
        // If the user supplied a title via the cardTitle parameter then use it or use a fake title
        let title = query.parameters && query.parameters[0].name === "cardTitle"
            ? query.parameters[0].value
            : faker.lorem.sentence();

        let randomImageUrl = "https://loremflickr.com/200/200"; // Faker's random images uses lorempixel.com, which has been down a lot

        // Build the data to send
        let attachments = [];

        // Generate 5 results to send with fake text and fake images
        for (let i = 0; i < 5; i++) {
            attachments.push(
                new builder.ThumbnailCard()
                    .title(title)
                    .text(faker.lorem.paragraph())
                    .images([new builder.CardImage().url(`${randomImageUrl}?random=${i}`)])
                    .toAttachment());
        }

        // Build the response to be sent
        let response = msteams.ComposeExtensionResponse
            .result("list")
            .attachments(attachments)
            .toResponse();

        // Send the response to teams
        callback(null, response, 200);
    }
}
