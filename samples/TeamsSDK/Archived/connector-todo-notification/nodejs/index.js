const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3978;
const server = express();
const axios = require('axios');
const { validateWebhook } = require('./webhookValidator');
const { requireAuth } = require('./authMiddleware');

// Webhooks registered per authenticated connector context, keyed by "tenantId:objectId".
var subscriptions = {};

function ownerKey(user) {
    return `${user.tenantId}:${user.objectId}`;
}

// Re-validates the destination immediately before the outbound call (defense in depth
// against SSRF) and disables redirects so a trusted host cannot bounce to an internal target.
async function postCard(url, card) {
    const result = await validateWebhook(url);
    if (!result.valid) {
        return;
    }
    try {
        await axios.post(url, card, { maxRedirects: 0 });
    } catch (error) {
        console.error(error.message);
    }
}

var taskList = {
    "task": [
        {
            "Title": "Sample task 1",
            "Assigned": "Alex",
            "Description": "Description for sample task 1"
        },
        {
            "Title": "Sample task 2",
            "Assigned": "Wilbur",
            "Description": "Description for sample task 2"
        }
    ]
}

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});

server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

server.get('/SetupAuth', (req, res, next) => {
    res.render('./views/SetupAuth')
});

server.get('/TaskDetails', (req, res, next) => {
    res.render('./views/TaskDetails', { taskList: JSON.stringify(taskList) })
});

server.post('/Task/Save', requireAuth, (req, res, next) => {
    var task = {
        "Title": req.body.title,
        "Description": req.body.description,
        "Assigned": req.body.assignedTo
    };

    taskList.task.push(task);

    var card = {
        "@type": "MessageCard",
        "summary": "Task Created",
        "sections": [
            {
                "activityTitle": "Task " + task.Title,
                "facts": [
                    {
                        "name": 'Title:',
                        "value": task.Title
                    },
                    {
                        "name": 'Description:',
                        "value": task.Description
                    },
                    {
                        "name": 'Assigned To:',
                        "value": task.Assigned
                    }
                ]
            }],
        "potentialAction": [
            {
                "@context": "http://schema.org",
                "@type": "ViewAction",
                "name": "View Task List",
                "target": [
                    process.env.BaseUrl + "/TaskDetails"
                ]
            }
        ]
    }

    // Only notify the webhook registered by the current authenticated connector context.
    const url = subscriptions[ownerKey(req.user)];
    if (url) {
        postCard(url, card);
    }

    res.json({ status: 'Task saved.' });
});

server.get('/Create', (req, res, next) => {
    res.render('./views/Create')
});

server.get('/SimpleStart', (req, res, next) => {
    res.render('./views/SimpleStart', {
        clientId: JSON.stringify(process.env.MicrosoftAppId),
        tenantId: JSON.stringify(process.env.MicrosoftAppTenantId)
    })
});

server.get('/SimpleEnd', (req, res, next) => {
    res.render('./views/SimpleEnd', {
        clientId: JSON.stringify(process.env.MicrosoftAppId),
        tenantId: JSON.stringify(process.env.MicrosoftAppTenantId)
    })
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.post('/Connector/Save', requireAuth, async (req, res) => {

    // Validate the destination before storing or calling it to prevent SSRF (CWE-918).
    const result = await validateWebhook(req.body.webhookUrl);
    if (!result.valid) {
        res.status(400).json({ error: result.reason });
        return;
    }

    // Bind the webhook to the authenticated connector context (owner + tenant).
    subscriptions[ownerKey(req.user)] = req.body.webhookUrl;

    var link = process.env.BaseUrl + "/TaskDetails"
    var card = {
        "@type": "MessageCard",
        "summary": "Welcome Message",
        "sections": [{
            "activityTitle": "Welcome Message",
            "text": "Teams todo connector is setup we will notify you when new task is created [here](" + link + ")"
        }]
    }

    await postCard(req.body.webhookUrl, card);

    res.json({ status: 'Webhook registered.' });
});