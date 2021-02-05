"use strict";
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const node_fetch_1 = require("node-fetch");
const express = require("express");
const jwt_decode_1 = require("jwt-decode");
const app = express();
require('dotenv').config();
const clientId = process.env.CLIENT_ID;
const clientSecret = process.env.CLIENT_SECRET;
const graphScopes = 'https://graph.microsoft.com/' + process.env.GRAPH_SCOPES;
let handleQueryError = function (err) {
    console.log("handleQueryError called: ", err);
    return new Response(JSON.stringify({
        code: 400,
        message: 'Stupid network Error'
    }));
};
app.get('/getGraphAccessToken', (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const ssoToken = req.query.ssoToken;
    let tenantId = jwt_decode_1.default(ssoToken)['tid']; //Get the tenant ID from the decoded token
    let accessTokenEndpoint = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`;
    //Create your access token query parameters
    //Learn more: https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow#first-case-access-token-request-with-a-shared-secret
    let accessTokenQueryParams = {
        grant_type: 'urn:ietf:params:oauth:grant-type:jwt-bearer',
        client_id: clientId,
        client_secret: clientSecret,
        assertion: req.query.ssoToken,
        scope: graphScopes,
        requested_token_use: "on_behalf_of",
    };
    let body = new URLSearchParams(accessTokenQueryParams).toString();
    let accessTokenReqOptions = {
        method: 'POST',
        headers: {
            Accept: "application/json",
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: body
    };
    let response = yield node_fetch_1.default(accessTokenEndpoint, accessTokenReqOptions).catch(handleQueryError);
    let data = yield response.json();
    console.log(`${data.token_type} token received`);
    if (!response.ok) {
        if ((data.error === 'invalid_grant') || (data.error === 'interaction_required')) {
            //This is expected if it's the user's first time running the app ( user must consent ) or the admin requires MFA
            console.log("User must consent or perform MFA. You may also encouter this error if your client ID or secret is incorrect.");
            res.status(403).json({ error: 'consent_required' }); //This error triggers the consent flow in the client.
        }
        else {
            //Unknown error
            console.log('Could not exchange access token for unknown reasons.');
            res.status(500).json({ error: 'Could not exchange access token' });
        }
    }
    else {
        //The on behalf of token exchange worked. Return the token (data object) to the client.
        res.send(data);
    }
}));
// Handles any requests that don't match the ones above
app.get('*', (req, res) => {
    console.log("Unhandled request: ", req);
    res.status(404).send("Path not defined");
});
const port = process.env.PORT || 5000;
app.listen(port);
console.log('API server is listening on port ' + port);
//# sourceMappingURL=server.js.map