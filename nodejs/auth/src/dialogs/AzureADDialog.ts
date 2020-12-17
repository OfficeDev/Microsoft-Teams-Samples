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

import * as request from "request-promise";
import * as builder from "botbuilder";
import { IdentityProviderDialog } from "./IdentityProviderDialog";

export const AZUREAD_DIALOG = "AzureADDialog";
const graphProfileUrl = "https://graph.microsoft.com/v1.0/me";

export class AzureADDialog extends IdentityProviderDialog {

    constructor(
        connectionName: string)
    {
        super(AZUREAD_DIALOG, connectionName);
    }

    public get displayName() { return "Azure AD"; }

    protected async getProfileFromProvider(accessToken: string): Promise<any> {
        let options = {
            url: graphProfileUrl,
            json: true,
            headers: {
                "Authorization": `Bearer ${accessToken}`,
            },
        };
        return await request.get(options);
    }

    protected async getProfileCard(accessToken: string): Promise<builder.Attachment> {
        const profile = await this.getProfileFromProvider(accessToken);
        return builder.CardFactory.thumbnailCard(
            profile.displayName,
            `<b>E-mail</b>: ${profile.mail}<br/>
            <b>Title</b>: ${profile.jobTitle}<br/>
            <b>Office location</b>: ${profile.officeLocation}`,
            [],
            [],
            { subtitle: profile.userPrincipalName });
    }
}
