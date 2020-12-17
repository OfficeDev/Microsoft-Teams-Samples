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

export const LINKEDIN_DIALOG = "LinkedInDialog";
const apiBaseUrl = "https://api.linkedin.com/v2";

export class LinkedInDialog extends IdentityProviderDialog {

    constructor(
        connectionName: string)
    {
        super(LINKEDIN_DIALOG, connectionName);
    }

    public get displayName() { return "LinkedIn"; }

    protected async getProfileFromProvider(accessToken: string): Promise<any> {
        let options = {
            url: `${apiBaseUrl}/me?projection=(id,firstName,lastName,profilePicture(displayImage~:playableStreams))`,
            json: true,
            headers: {
                "Authorization": `Bearer ${accessToken}`,
            },
        };
        const profile = await request.get(options);
        return {
            localizedFirstName: this.getPreferredLocalizedString(profile.firstName),
            localizedLastName: this.getPreferredLocalizedString(profile.lastName),
            profilePictureUrl: profile.profilePicture["displayImage~"].elements[0].identifiers[0].identifier,
        }
    }

    protected async getProfileCard(accessToken: string): Promise<builder.Attachment> {
        let profile = await this.getProfileFromProvider(accessToken);
        return builder.CardFactory.thumbnailCard(
            `${profile.localizedFirstName} ${profile.localizedLastName}`,
            [
                { url: profile.profilePictureUrl }
            ],
            [
            ]);
    }

    private getPreferredLocalizedString(localizedString: any): string {
        const preferredLocale = `${localizedString.preferredLocale.language}_${localizedString.preferredLocale.country}`;
        return localizedString.localized[preferredLocale];
    }
}
