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
import * as constants from "../constants";
import * as utils from "../utils";
import { cardTemplates } from "./CardTemplates";
import { renderACAttachment, renderO365ConnectorAttachment } from "../utils/CardUtils";

// Dialog for the Adaptive Card tester
export class ACGeneratorDialog extends builder.IntentDialog
{
    constructor(private dialogId: string) {
        super({ recognizeMode: builder.RecognizeMode.onBegin });
    }

    public register(bot: builder.UniversalBot, rootDialog: builder.IntentDialog): void {
        bot.dialog(this.dialogId, this);

        this.onBegin((session, args, next) => { this.onDialogBegin(session, args, next); });
        this.onDefault((session) => { this.onMessageReceived(session); });
    }

    // Handle start of dialog
    private async onDialogBegin(session: builder.Session, args: any, next: () => void): Promise<void> {
        next();
    }

    // Handle message
    private async onMessageReceived(session: builder.Session): Promise<void> {
        if (session.message.text === "") {
            if ((session.message.value !== undefined) && (session.message.value.acBody !== undefined)) {
                try {
                    let card = JSON.parse(session.message.value.acBody);
                    // Check to see if the body is an Adaptive Card
                    if ((card.type !== undefined) && (card.type.toLowerCase() === "adaptivecard")) {
                        session.endDialog(new builder.Message(session).addAttachment(
                            renderACAttachment(card, null),
                        ));
                    }
                    // Check to see if it's an Office 365 Connector Card
                    if ((card["@type"] !== undefined) && (card["@type"].toLowerCase() === "messagecard")) {
                        session.endDialog(new builder.Message(session).addAttachment(
                            renderO365ConnectorAttachment(card, null),
                        ));
                    }
                    // Check to see if it's a Bot Framework card
                    if (card.contentType !== undefined) {
                        session.endDialog(new builder.Message(session).addAttachment(card));
                    }
                }
                catch {
                    session.send("Error parsing Adaptive Card JSON.");
                }
            }
            else {
                console.log("AC payload: " + JSON.stringify(session.message.value));
            }
        }
        else {
            // Message might contain @mentions which we would like to strip off in the response
            let text = utils.getTextWithoutMentions(session.message);
            if (text === constants.DialogId.ACTester) {
                // The user has typed "actester"
                session.send(new builder.Message(session).addAttachment(
                    renderACAttachment(cardTemplates.acTester, null),
                ));
            }
            // session.send("You said: %s", text);
        }
    }
}
