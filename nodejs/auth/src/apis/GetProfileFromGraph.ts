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

import * as express from "express";
import * as request from "request-promise";

// Gets the basic user profile from Graph, using the id_token in the Authorization header
export class GetProfileFromGraph {

    public constructor(
        private clientId: string,
        private clientSecret: string,
    ) { }

    public listen(): express.RequestHandler {
        return async (req: express.Request, res: express.Response) => {
            let encodedToken = res.locals.encodedToken;
            let token = res.locals.token;
            let tenantId = token["tid"];
            let graphAccessToken: string;
            let tokenEndpoint: string;
            let params: any;
            // The version of the endpoint to use for OBO flow must match the one used to get the initial token
            switch (token.ver) {
                case "1.0":
                {
                    // AAD v1 endpoint token
                    tokenEndpoint = `https://login.microsoftonline.com/${tenantId}/oauth2/token`;
                    params = {
                        grant_type: "urn:ietf:params:oauth:grant-type:jwt-bearer",
                        assertion: encodedToken,
                        client_id: this.clientId,
                        client_secret: this.clientSecret,
                        resource: "https://graph.microsoft.com",
                        requested_token_use: "on_behalf_of",
                        scope: "openid",
                    } as any;
                    break;
                }

                case "2.0":
                {
                    // AAD v2 endpoint token
                    tokenEndpoint = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`;
                    params = {
                        grant_type: "urn:ietf:params:oauth:grant-type:jwt-bearer",
                        assertion: encodedToken,
                        client_id: this.clientId,
                        client_secret: this.clientSecret,
                        requested_token_use: "on_behalf_of",
                        scope: "https://graph.microsoft.com/User.Read",
                    } as any;
                    break;
                }

                default:
                    throw new Error(`Unsupported Azure AD endpoint version ${token.ver}`);
            }

            try {
                let tokenResponse = await request.post({ url: tokenEndpoint, form: params, json: true });
                graphAccessToken = tokenResponse.access_token;
            }
            catch (ex) {
                console.error("Error getting access token for Graph via On-Behalf-Of: ", ex);

                // If this exception is due to additional consent required, the client code can use this show a consent popup.
                // A production app should carefully examine server information sent to the client, to avoid leaking information inadvertently 
                if (ex.error && (ex.error.error === "invalid_grant" || ex.error.error === "interaction_required")) {
                    res.status(403).send(ex.error.error);
                } else {
                    res.status(500);
                }
                return;
            }

            // The OBO grant flow can fail with error interaction_required if there are Conditional Access policies set.
            // This example does not handle that. See https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols-oauth-on-behalf-of#error-response-example

            // Get user profile from Graph
            let options = {
                url: "https://graph.microsoft.com/v1.0/me",
                json: true,
                headers: {
                    "Authorization": `Bearer ${graphAccessToken}`,
                },
            };
            let profile = await request.get(options);

            // Return profile as response
            res.status(200).send(profile);
        };
    }
}
