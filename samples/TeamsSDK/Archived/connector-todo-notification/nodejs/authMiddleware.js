'use strict';

const jwt = require('jsonwebtoken');
const jwksClient = require('jwks-rsa');

/**
 * Middleware that requires a valid Microsoft Entra ID bearer token before an
 * endpoint runs. This prevents anonymous callers from registering webhooks or
 * triggering outbound notifications. The authenticated user's object id and
 * tenant id are attached to req.user so webhooks can be bound to that context.
 */

const tenantId = process.env.MicrosoftAppTenantId || process.env.TenantId || 'common';
const audience = process.env.MicrosoftAppId;

const client = jwksClient({
    jwksUri: `https://login.microsoftonline.com/${tenantId}/discovery/v2.0/keys`,
    cache: true,
    rateLimit: true
});

function getSigningKey(header, callback) {
    client.getSigningKey(header.kid, (err, key) => {
        if (err) {
            callback(err);
            return;
        }
        callback(null, key.getPublicKey());
    });
}

function requireAuth(req, res, next) {
    const authHeader = req.headers.authorization || '';
    const match = authHeader.match(/^Bearer\s+(.+)$/i);
    if (!match) {
        res.status(401).json({ error: 'Missing bearer token.' });
        return;
    }

    if (!audience) {
        // Fail closed if the application is not configured for authentication.
        res.status(500).json({ error: 'Authentication is not configured.' });
        return;
    }

    const verifyOptions = {
        audience: audience,
        issuer: [
            `https://login.microsoftonline.com/${tenantId}/v2.0`,
            `https://sts.windows.net/${tenantId}/`
        ],
        algorithms: ['RS256']
    };

    jwt.verify(match[1], getSigningKey, verifyOptions, (err, decoded) => {
        if (err) {
            res.status(401).json({ error: 'Invalid bearer token.' });
            return;
        }

        req.user = {
            objectId: decoded.oid,
            tenantId: decoded.tid
        };
        next();
    });
}

module.exports = { requireAuth };
