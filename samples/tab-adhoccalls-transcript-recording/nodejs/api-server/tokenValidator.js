// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const axios = require('axios');
const jwt = require('jsonwebtoken');
const jwksClient = require('jwks-rsa');

async function validateToken(token) {
    try {
        const ExpectedMicrosoftApps = '0bf30f3b-4a52-48df-9a82-234910c4a086'; // Microsoft Graph Change Tracking
        const { header, payload } = jwt.decode(token, { complete: true });
        // use a new client to fetch pubkey from MS
        const client = jwksClient({ jwksUri: 'https://login.microsoftonline.com/common/discovery/keys' });
        // fetch public key based on your token kid
        var key = await client.getSigningKey(header.kid).then((key) => key.publicKey || key.rsaPublicKey);

        if (!key) {
            throw new Error('Public key not found');
        }

        const verifyOptions = { algorithms: ['RS256'], header: header };
        const fullToken = jwt.verify(token, key, verifyOptions);
        
         // For v2.0 tokens you'll get "azp"
         // For v1.0 tokens you'll get "appid"
        const azpOrAppid = fullToken.azp || fullToken.appid;
        if (azpOrAppid !== ExpectedMicrosoftApps) {
            throw new Error('Not Expected Microsoft Apps.');
        }
        
        return fullToken;
    } catch (error) {
        console.error('Token validation error:', error);
        throw new Error('Invalid token: ' + error.message);
    }
}

module.exports = { validateToken };