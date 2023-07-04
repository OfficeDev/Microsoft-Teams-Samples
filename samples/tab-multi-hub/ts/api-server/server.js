const fetch = require('node-fetch');
const express = require('express');
const jwt_decode = require('jwt-decode');
const msal = require('@azure/msal-node');
const app = express();
const path = require('path');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const clientId = process.env.CLIENT_ID;
const clientSecret = process.env.CLIENT_SECRET;
const graphScopes = ['https://graph.microsoft.com/' + process.env.GRAPH_SCOPES];
let handleQueryError = function (err) {
    console.log("handleQueryError called: ", err);
    return new Response(JSON.stringify({
        code: 400,
        message: 'Bad Request'
    }));
};

app.get('/getGraphAccessToken', async (req,res) => {

    const msalClient = new msal.ConfidentialClientApplication({
        auth: {
            clientId: clientId,
            clientSecret: clientSecret
        }
    });
    
    let tenantId = jwt_decode(req.query.ssoToken)['tid']; //Get the tenant ID from the decoded toke

    msalClient.acquireTokenOnBehalfOf({
        authority: `https://login.microsoftonline.com/${tenantId}`,
        oboAssertion: req.query.ssoToken,
        scopes: graphScopes,
        skipCache: true

      })
      .then( async (result) => {     
                let graphPhotoEndpoint = `https://graph.microsoft.com/v1.0/users/${req.query.upn}/events?$select=subject,body,bodyPreview,organizer,attendees,start,end,location`;
                let graphRequestParams = {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        "authorization": "bearer " + result.accessToken
                    }
                }
                
                let response = await fetch(graphPhotoEndpoint,graphRequestParams).catch(this.unhandledFetchError);
                
                if (response.ok) {
                    let data = await response.json();
                    res.send(data);
                } else {
                    console.error("ERROR: ", response);
                }
               
      })
      .catch(error => {
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