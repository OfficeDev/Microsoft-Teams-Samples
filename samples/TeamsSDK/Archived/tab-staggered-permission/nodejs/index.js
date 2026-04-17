// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const msal = require('@azure/msal-node');
const express = require('express');
const cors = require('cors');
const { SimpleGraphClient } = require('./simpleGraphClient');

// Load environment variables.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3978;
const server = express();

const tid = process.env.TenantId;

const scopes = ["https://graph.microsoft.com/User.Read"];

let msalClient;

function getMsalClient() {
    if (!msalClient) {
        msalClient = new msal.ConfidentialClientApplication({
            auth: {
                clientId: process.env.MicrosoftAppId,
                clientSecret: process.env.MicrosoftAppPassword,
                authority: `https://login.microsoftonline.com/${process.env.TenantId}`
            }
        });
    }
    return msalClient;
}

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.set('view engine', 'ejs');
server.set('views', __dirname);

// Create HTTP server.
server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});

// Endpoint to fetch Auth tab page.
server.get('/tab', (req, res) => {
    res.render('./views/tab');
});

// Pop-up dialog to ask for additional permissions, redirects to AAD page.
server.get('/auth-start', (req, res) => {
    const scope = req.query.scope;
    const data = {
        clientId: process.env.MicrosoftAppId,
        scope: scope
    };
    res.render('./views/auth-start', { data: JSON.stringify(data) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window.
server.get('/auth-end', (req, res) => {
    const clientId = process.env.MicrosoftAppId;
    res.render('./views/auth-end', { clientId: JSON.stringify(clientId) });
});

// Exchange the id token with access token.
const getDelegateAccessToken = async (tid, token) => {
    try {
        const result = await getMsalClient().acquireTokenOnBehalfOf({
            authority: `https://login.microsoftonline.com/${tid}`,
            oboAssertion: token,
            scopes: scopes,
            skipCache: true
        });
        return result.accessToken;
    } catch (error) {
        console.log("Error occurred: " + error);
        throw error;
    }
};

// Get user photo.
server.post('/GetUserPhoto', async (req, res) => {
    try {
        const idToken = req.body.idToken;
        const accessToken = await getDelegateAccessToken(tid, idToken);
        const client = new SimpleGraphClient(accessToken);
        const userImage = await client.getUserPhoto();
        const result = await userImage.arrayBuffer();
        const imageString = Buffer.from(result).toString('base64');
        res.json({ image: "data:image/png;base64," + imageString });
    } catch (error) {
        console.log("Error getting user photo: " + error);
        res.status(500).json({ error: "Failed to get user photo" });
    }
});

// Get user mails.
server.post('/GetUserMails', async (req, res) => {
    try {
        const idToken = req.body.idToken;
        const accessToken = await getDelegateAccessToken(tid, idToken);
        const client = new SimpleGraphClient(accessToken);
        const userMails = await client.getMailAsync();
        res.json(userMails.value);
    } catch (error) {
        console.log("Error getting user mails: " + error);
        res.status(500).json({ error: "Failed to get user mails" });
    }
});

// Decode JWT token.
server.post('/decodeToken', (req, res) => {
    const token = req.body.idToken;
    if (token !== null && token !== undefined) {
        const base64String = token.split('.')[1];
        const decodedValue = JSON.parse(Buffer.from(base64String,
            'base64').toString('ascii'));
        res.json(decodedValue);
    } else {
        res.status(400).json({ error: "Invalid token" });
    }
});