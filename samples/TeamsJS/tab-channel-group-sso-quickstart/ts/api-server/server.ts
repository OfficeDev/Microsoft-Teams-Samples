// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import fetch from 'node-fetch';
import * as express from 'express';
import jwt_decode, { JwtPayload } from 'jwt-decode';
const path = require('path');
import * as msal from '@azure/msal-node';
const app = express();
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE.replace('\lib', '') });
const clientId = process.env.CLIENT_ID;
const clientSecret = process.env.CLIENT_SECRET;
const graphScopes = [']https://graph.microsoft.com/' + process.env.GRAPH_SCOPES];

let handleQueryError = function (err: string) {
    console.log("handleQueryError called: ", err);
    return new Response(JSON.stringify({
        code: 400,
        message: 'Stupid network Error'
    }));
};

app.get('/getGraphAccessToken', async (req,res) => {
    const msalClient = new msal.ConfidentialClientApplication({
        auth: {
            clientId: clientId,
            clientSecret: clientSecret
        }
    });
    const ssoToken = req.query.ssoToken as string;
    let tenantId = jwt_decode<JwtPayload>(ssoToken)['tid']; //Get the tenant ID from the decoded token

    msalClient.acquireTokenOnBehalfOf({
        authority: `https://login.microsoftonline.com/${tenantId}`,
        oboAssertion: req.query.ssoToken as string,
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
                let response = await fetch(graphPhotoEndpoint,graphRequestParams);
                if(!response.ok){
                    console.error("ERROR: ", response);
                }
                else{
                    const imageBuffer = await response.arrayBuffer(); // Get image data as raw binary data
                    // Convert binary data to an image URL and set the url in state
                    const imageUri = 'data:image/png;base64,' + Buffer.from(imageBuffer).toString('base64');
                    res.json(imageUri);
                }
      })
      .catch((error) => {
        console.log("error"+ error.errorCode);
        res.status(403).json({ error: 'consent_required' });
    });
});

// Handles any requests that don't match the ones above
app.get('*', (req,res) =>{
    console.log("Unhandled request: ",req);
    res.status(404).send("Path not defined");
});

const port = process.env.PORT || 5000;
app.listen(port);

console.log('API server is listening on port ' + port);
