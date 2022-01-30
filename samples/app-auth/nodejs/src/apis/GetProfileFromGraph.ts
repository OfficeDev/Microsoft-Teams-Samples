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
import * as msal from "@azure/msal-node";

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
            try {
                const msalClient = new msal.ConfidentialClientApplication({
                    auth: {
                        clientId:  this.clientId,
                        clientSecret: this.clientSecret
                    }
                });
                const scopes = ["https://graph.microsoft.com/User.Read email offline_access openid profile"];
                
                msalClient.acquireTokenOnBehalfOf({
                    authority: `https://login.microsoftonline.com/${tenantId}`,
                    oboAssertion: encodedToken,
                    scopes: scopes,
                    skipCache: true
                  })
                  .then( async result => {
                        graphAccessToken = result.accessToken;
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
                    })
                .catch(error => {
                    console.log(error);
                });
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
        };
    }
}