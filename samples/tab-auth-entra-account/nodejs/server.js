// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const express = require('express');
const session = require('express-session');
const bodyParser = require('body-parser');
const axios = require('axios');
const path = require('path');
const cookieParser = require('cookie-parser');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const app = express();
const PORT = process.env.PORT || 3978;

app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));

app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());
app.use(cookieParser());
app.use(session({
    secret: 'some secret key',
    resave: false,
    saveUninitialized: true
}));
app.use(express.static(path.join(__dirname, 'public')));

const CLIENT_ID = process.env.ClientId;
const CLIENT_SECRET = process.env.ClientSecret;
const REDIRECT_URI = process.env.REDIRECT_URI;

app.get('/', (req, res) => {
    res.render('index');
});

app.get('/authstart', (req, res) => {
    const { authId, oauthRedirectMethod, hostRedirectUrl } = req.query;
    res.render('authStart', {
        clientId: CLIENT_ID,
        redirectUri: REDIRECT_URI,
        authId,
        oauthRedirectMethod,
        hostRedirectUrl
    });
});

app.get('/Auth/AuthEnd', async (req, res) => {
    const code = req.query.code;
    const state = JSON.parse(decodeURIComponent(req.query.state || '{}'));

    try {
        const tokenResponse = await axios.post('https://login.microsoftonline.com/common/oauth2/v2.0/token', new URLSearchParams({
            client_id: CLIENT_ID,
            scope: 'openid profile email User.Read offline_access',
            code,
            redirect_uri: REDIRECT_URI,
            grant_type: 'authorization_code',
            client_secret: CLIENT_SECRET
        }), {
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            }
        });

        req.session.accessToken = tokenResponse.data.access_token;
        req.session.idToken = tokenResponse.data.id_token;

        res.render('authEnd', {
            hostRedirectUrl: state.hostRedirectUrl,
            idToken: tokenResponse.data.id_token,
            accessToken: tokenResponse.data.access_token
        });

    } catch (error) {
        console.error("Token exchange failed:", error.response?.data || error.message);
        res.render('error', { message: "Token exchange failed" });
    }
});

app.post('/getAuthAccessToken', async (req, res) => {
    const idToken = req.body.idToken;
    
    console.log("Access token from session:", req.session.accessToken);
    console.log("ID token from request body:", idToken);

    if (!req.session.accessToken) {
        console.error("No access token found in session");
        return res.status(401).json({ error: "No access token available" });
    }

    try {
        const profileResponse = await axios.get('https://graph.microsoft.com/v1.0/me', {
            headers: {
                Authorization: `Bearer ${req.session.accessToken}`
            }
        });

        res.json(JSON.stringify(profileResponse.data));

    } catch (error) {
        console.error("Failed to fetch profile:", error.response?.data || error.message);
        res.status(500).send('Failed to fetch profile');
    }
});

app.get('/error', (req, res) => {
    res.render('error', { message: "An error occurred" });
});

app.listen(PORT, () => {
    console.log(`Server is running on http://localhost:${PORT}`);
});
