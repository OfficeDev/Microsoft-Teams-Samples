const axios = require('axios');
const jwt = require('jsonwebtoken');
const jwksClient = require('jwks-rsa');

async function validateToken(token) {
    try {
        const { header, payload } = jwt.decode(token, { complete: true });
        // use a new client to fetch pubkey from MS
        const client = jwksClient({ jwksUri: 'https://login.microsoftonline.com/common/discovery/keys' });
        // fetch public key based on your token kid (you can do this manually as per the page link above by hitting the ms link
        var key = await client.getSigningKey(header.kid).then((key) => key.publicKey || key.rsaPublicKey);

        if (!key) {
            throw new Error('Public key not found');
        }
        
        const verifyOptions = { algorithms: ['RS256'], header: header };
        const fullToken = jwt.verify(token, key, verifyOptions);
        return fullToken;
    } catch (error) {
        console.log(error);
        throw new Error('Invalid token: ' + error.message);
    }
}

module.exports = { validateToken };