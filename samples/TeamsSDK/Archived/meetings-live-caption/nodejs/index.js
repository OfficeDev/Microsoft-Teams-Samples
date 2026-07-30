const path = require('path');
const express = require('express');
const https = require('https');
const fs = require('fs');

const cors = require('cors');
const MeetingApiHelper = require('./helpers/meetingApiHelper');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const allowedTenantId = (process.env.TEAMS_APP_TENANT_ID || '').trim().toLowerCase();

const server = express();
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.use(express.static(path.join(__dirname, 'public')));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

// Specify the paths to your SSL certificate and private key
const sslOptions = {
    key: fs.readFileSync(process.env.SSL_KEY_FILE),
    cert: fs.readFileSync(process.env.SSL_CRT_FILE)
};

const port = process.env.PORT || 3978;

// Create an HTTPS server
const httpsServer = https.createServer(sslOptions, server);

httpsServer.listen(port, () => 
    console.log(`\n${ server.name } listening to https://localhost:${ port }`)
);

function getTenantIdFromRequest(req) {
    return String(req.headers['x-tenant-id'] || '').trim().toLowerCase();
}

function isAllowedTenant(req) {
    // If no tenant is configured, preserve current behavior and allow requests.
    if (!allowedTenantId) {
        return true;
    }

    const requestTenantId = getTenantIdFromRequest(req);
    return requestTenantId !== '' && requestTenantId === allowedTenantId;
}

function tenantGuard(req, res, next) {
    if (!isAllowedTenant(req)) {
        return res.status(403).send();
    }

    return next();
}

// Returns view to be open in task module.
server.get('/Home/Index', (req, res) => {
    res.render('./views/index', {
        allowedTenantId: process.env.TEAMS_APP_TENANT_ID || ''
    });
});

// Returns view to of the config page.
server.get('/Home/Configure', (req, res) => {
    res.render('./views/configure', {
        allowedTenantId: process.env.TEAMS_APP_TENANT_ID || ''
    });
});

// Method to save CART Url in the app.
server.post('/api/meeting/SaveCARTUrl', tenantGuard, async (req, res) => {
    try {
        if (req.body != null && req.body.CartUrl != null) {
            if (req.body.CartUrl.trim() !== '' && req.body.CartUrl.includes('meetingid') && req.body.CartUrl.includes('token')) {
                MeetingApiHelper.setCartUrl(req.body.CartUrl);
                return res.status(200).send();
            }

            return res.status(400).send();
        }

        return res.status(400).send();
    } catch (ex) {
        console.log(ex);
        return res.status(500).send();
    }
});

// Method to send caption in the live meeting.
server.post('/api/meeting/LiveCaption', tenantGuard, async (req, res) => {
    try {
        const response = await MeetingApiHelper.postCaption(req.body.captionText.trim());
        return res.status(response).send();
    } catch (ex) {
        console.log(ex);
        return res.status(500).send();
    }
});