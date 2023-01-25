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

import * as config from "config";
import * as builder from "botbuilder";
import { RootDialog } from "./dialogs/RootDialog";
import { UserMappingMiddleware } from "./UserMappingMiddleware";
import { ConversationReference, ConversationAccount, ChannelAccount } from "botbuilder";
import { IdentityProviderDialog } from "./dialogs/IdentityProviderDialog";

// =========================================================
// Auth Bot
// =========================================================

export class AuthBot extends builder.TeamsActivityHandler {

    private aadObjectIdToBotUserMap: Map<string, any>;
    private dialogState: builder.StatePropertyAccessor<any>;

    constructor(
        private adapter: builder.BotFrameworkAdapter,
        private conversationState: builder.ConversationState,
        private userState: builder.UserState,
        private rootDialog: RootDialog,
        private identityProviderDialogs: IdentityProviderDialog[],
    )
    {
        super();
        this.dialogState = this.conversationState.createProperty("DialogState");

        this.aadObjectIdToBotUserMap = new Map<string, any>();
        this.adapter.use(new UserMappingMiddleware(this.aadObjectIdToBotUserMap));
                
        this.adapter.onTurnError = this.onTurnError.bind(this);

        this.onMessage(async (context, next) => {
            await this.rootDialog.run(context, this.dialogState);
            await next();
        });
    }

    public async run(context: builder.TurnContext) {
        await super.run(context);

        // Save any state changes. The load happened during the execution of the Dialog.
        await this.conversationState.saveChanges(context, false);
        await this.userState.saveChanges(context, false);
    }

    // Get the user's profile information from all the identity providers that we have tokens for
    public async getUserProfilesAsync(aadObjectId: string): Promise<any> {
        let profiles = {};

        // Get 29:xxx ID of the user
        if (this.aadObjectIdToBotUserMap.has(aadObjectId)) {
            const { userId, conversationId, serviceUrl } = this.aadObjectIdToBotUserMap.get(aadObjectId);

            let conversationRef: Partial<ConversationReference> = {
                bot: { id: "28:" + config.get("bot.appId") } as ChannelAccount,
                user: { id: userId } as ChannelAccount,
                conversation: { id: conversationId } as ConversationAccount,
                serviceUrl: serviceUrl,
            };

            await this.adapter.continueConversation(conversationRef, async (context: builder.TurnContext) => {
                var tasks = this.identityProviderDialogs.map(async (dialog) => {
                    var profile = await dialog.getProfileAsync(context);
                    if (profile) {
                        profiles[dialog.displayName] = profile;
                    }
                });
                await Promise.all(tasks);
            });
        } else {
            // User was not found in the store
        }

        return profiles;
    }

    protected async handleTeamsSigninVerifyState(context, state) {
        await this.rootDialog.run(context, this.dialogState);
    }
    
    private async onTurnError(context: builder.TurnContext, error: Error) {
        // This check writes out errors to console log .vs. app insights.
        // NOTE: In production environment, you should consider logging this to Azure
        //       application insights.
        console.error(`\n [onTurnError] unhandled error: ${ error }`);

        // Send a trace activity, which will be displayed in Bot Framework Emulator
        await context.sendTraceActivity(
            'OnTurnError Trace',
            `${ error }`,
            'https://www.botframework.com/schemas/error',
            'TurnError'
        );

       // Uncomment below commented line for local debugging.
       // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);

       // Clear out state
        await this.conversationState.clear(context);
    }
}
