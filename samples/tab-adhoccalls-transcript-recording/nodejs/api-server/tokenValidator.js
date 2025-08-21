// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const axios = require('axios');
const jwt = require('jsonwebtoken');
const jwksClient = require('jwks-rsa');

async function validateToken(token, appRegistrationId) {
    try {
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
            // Validate azp (Authorized Party) or appid claim
            const azpOrAppid = fullToken.azp || fullToken.aud;
            if (!azpOrAppid || azpOrAppid !== appRegistrationId) {
                throw new Error(`Invalid azp/appid claim: expected '${appRegistrationId}', got '${azpOrAppid}'`);
            }
            return fullToken;
    } catch (error) {
        console.error('Token validation error:', error);
        throw new Error('Invalid token: ' + error.message);
    }
}

module.exports = { validateToken };