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
import * as jwt from "jsonwebtoken";
import { OpenIdMetadata } from "./OpenIdMetadata";

// Middleware to validate the id_token from AAD in the Authorization header
export class ValidateAzureADToken {

    public constructor(
        private openIdMetadataV1: OpenIdMetadata,
        private openIdMetadataV2: OpenIdMetadata,
        private appId: string,
    ) { }

    public listen(): express.RequestHandler {
        return (req: express.Request, res: express.Response, next: any) => {
            // Get bearer token
            let authHeaderMatch = /^Bearer (.*)/i.exec(req.header("authorization"));
            if (!authHeaderMatch) {
                console.error("No Authorization header provided");
                res.sendStatus(401);
                return;
            }

            // Decode token and get signing key
            const encodedToken = authHeaderMatch[1];
            const decodedToken = jwt.decode(encodedToken, { complete: true });
            let openIdMetadata: OpenIdMetadata;

            switch (decodedToken["payload"].ver) {
                case "2.0":
                    openIdMetadata = this.openIdMetadataV2;
                    break;
                case "1.0":
                default:
                    openIdMetadata = this.openIdMetadataV1;
                    break;
            }
            openIdMetadata.getKey(decodedToken["header"].kid, (key) => {
                if (!key) {
                    console.error("Invalid signing key or OpenId metadata document");
                    res.sendStatus(500);
                    return;
                }

                // Verify token
                const verifyOptions: jwt.VerifyOptions = {
                    algorithms: ["RS256", "RS384", "RS512"],
                    issuer: openIdMetadata.getIssuer(decodedToken["payload"].tid),
                 // audience: this.appId,
                    clockTolerance: 300,
                };
                try {
                    console.log(this.appId);
                    res.locals.token = jwt.verify(encodedToken, key.key, verifyOptions);
                    res.locals.encodedToken = encodedToken;
                    next();
                } catch (e) {
                    console.error("Invalid bearer token", e);
                    res.sendStatus(401);
                }
            });
        };
    }
}
