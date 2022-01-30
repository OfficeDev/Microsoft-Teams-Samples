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
const msal = require('@azure/msal-node');
const path = require('path');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });
const clientId = process.env.CLIENT_ID;
const clientSecret = process.env.CLIENT_SECRET;
const graphScopes = ['https://graph.microsoft.com/' + process.env.GRAPH_SCOPES];
let handleQueryError = function (err) {
    console.log("handleQueryError called: ", err);
    return new Response(JSON.stringify({
        code: 400,
        message: 'Stupid network Error'
    }));
};
app.get('/getGraphAccessToken', (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const msalClient = new msal.ConfidentialClientApplication({
        auth: {
            clientId: clientId,
            clientSecret: clientSecret
        }
    });
    const ssoToken = req.query.ssoToken
    let tenantId = jwt_decode_1.default(ssoToken)['tid'];//Get the tenant ID from the decoded token

    msalClient.acquireTokenOnBehalfOf({
        authority: `https://login.microsoftonline.com/${tenantId}`,
        oboAssertion: req.query.ssoToken,
        scopes: graphScopes,
        skipCache: true
      })
      .then( async (result) => {     
                let graphPhotoEndpoint = `https://graph.microsoft.com/v1.0/users/${req.query.upn}/photo/$value`;
                let graphRequestParams = {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'image/jpg',
                        "authorization": "bearer " + result.accessToken
                    }
                }
                let response = yield node_fetch_1.default(graphPhotoEndpoint,graphRequestParams).catch(handleQueryError);
                if(!response.ok){
                    console.error("ERROR: ", response);
                }
                else{
                    const imageBuffer = yield response.arrayBuffer().catch(this.unhandledFetchError); // Get image data as raw binary data
                    // Convert binary data to an image URL and set the url in state
                    const imageUri = 'data:image/png;base64,' + Buffer.from(imageBuffer).toString('base64');
                    res.json(imageUri);
                }
      })
      .catch((error) => {
        console.log("error"+ error.errorCode);
        res.status(403).json({ error: 'consent_required' });
    });
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