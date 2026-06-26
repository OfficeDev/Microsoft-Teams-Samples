const express = require('express');
const jwt = require('jsonwebtoken');
const TokenStore = require('../services/TokenStore');
const fetch = require('node-fetch');

const router = express.Router();

router.get('/callback', async (req, res) => {
    const { code, state } = req.query;

    if (!code || !state) {
        return res.status(400).send('Missing code or state in the callback');
    }

    try {
        const response = await fetch(`https://${process.env.AUTH0_DOMAIN}/oauth/token`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                grant_type: 'authorization_code',
                client_id: process.env.AUTH0_CLIENT_ID,
                client_secret: process.env.AUTH0_CLIENT_SECRET,
                code,
                redirect_uri: `${process.env.APP_URL}/api/auth/callback`
            })
        });

        if (!response.ok) {
            throw new Error('Failed to exchange code for token');
        }

        const data = await response.json();
        const { access_token } = data;

        if (access_token) {
            TokenStore.setToken(state, access_token);
            res.redirect('/public/auth-end.html');
        } else {
            res.status(400).send('No access token received');
        }
    } catch (err) {
        console.error('Auth callback error:', err);
        res.status(500).send('Authentication failed');
    }
});

module.exports = router;