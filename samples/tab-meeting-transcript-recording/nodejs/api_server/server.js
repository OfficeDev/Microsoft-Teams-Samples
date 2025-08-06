const express = require('express');
const jwt_decode = require('jwt-decode');
const msal = require('@azure/msal-node');
const app = express();
const path = require('path');
const axios = require('axios');
const { DateTime } = require('luxon');
const ENV_FILE = path.join(__dirname, '.env');
const { Readable } = require('stream');
require('dotenv').config({ path: ENV_FILE });

const clientId = process.env.APP_REGISTRATION_ID;
const clientSecret = process.env.CLIENT_SECRET;
const baseUrl = process.env.BASE_URL;
const graphScopes = ['https://graph.microsoft.com/User.Read'];
let adhocCallDetails = [];
let token = null;
let callUpdated = false;
const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });

// =======================
// UTILITY FUNCTIONS
// =======================
async function getApiData(url, accessToken) {
    try {
        const response = await axios.get(url, {
            headers: { Authorization: `Bearer ${accessToken}` }
        });
        return JSON.stringify(response.data);
    } catch (error) {
        console.error('Error fetching API data:', error);
        //throw error;
    }
}

async function getToken(req) {
    const msalClient = new msal.ConfidentialClientApplication({
        auth: {
            clientId: clientId,
            clientSecret: clientSecret,
        }
    });

    let tenantId = jwt_decode(req.query.ssoToken)['tid'];
    try {
        const result = await msalClient.acquireTokenOnBehalfOf({
            authority: `https://login.microsoftonline.com/${tenantId}`,
            oboAssertion: req.query.ssoToken,
            scopes: graphScopes,
            skipCache: true
        });
        return result.accessToken;
    } catch (error) {
        throw new Error('consent_required');
    }
}

async function deleteSubscription(subscriptionId, accessToken) {
    try {
        await axios.delete(`https://graph.microsoft.com/v1.0/subscriptions/${subscriptionId}`, {
            headers: {
                "accept": "application/json",
                "contentType": 'application/json',
                "authorization": "bearer " + accessToken
            }
        });
    } catch (error) {
        console.error('Error Deleting Subscription:', error);
    }
}

// ========== POST: WEBHOOKS ==========

app.post('/webhookAdhocTranscripts', async (req, res) => {
    if (req.query && req.query.validationToken) {
        res.send(req.query.validationToken);
    } else {
        const callId = req.query.callId;
        if (token != null) {
            const endpoint = `https://graph.microsoft.com/beta/me/adhocCalls/${callId}/transcripts`;
            const transcriptsResponse = await getApiData(endpoint, token);
            const responseTranscriptsData = JSON.parse(transcriptsResponse);

            adhocCallDetails = adhocCallDetails.map(call => {
                if (call.callId === callId) {
                    return {
                        ...call,
                        transcriptsId: responseTranscriptsData.value[0]?.id ?? null,
                        notify: true,
                        condition: true
                    };
                }
                return call;
            });
            io.emit('message', adhocCallDetails);
            res.status(200).send('OK');
        }
    }
});

app.post('/webhookAdhocRecordings', async (req, res) => {
    if (req.query && req.query.validationToken) {
        res.send(req.query.validationToken);
    } else {
        const callId = req.query.callId;
        if (token != null) {
            const endpoint = `https://graph.microsoft.com/beta/me/adhocCalls/${callId}/recordings`;
            const recordingsResponse = await getApiData(endpoint, token);
            const responseRecordingsData = JSON.parse(recordingsResponse);

            adhocCallDetails = adhocCallDetails.map(call => {
                if (call.callId === callId) {
                    return {
                        ...call,
                        recordingId: responseRecordingsData.value[0]?.id ?? null,
                        notify: true,
                        condition: true
                    };
                }
                return call;
            });
            io.emit('message', adhocCallDetails);
            res.status(200).send('OK');
        }
    }
});

// ========== GET: ADHOC CALL LIST & DETAILS ==========

// Get Adhoc call list (dummy: replace with your logic to list user’s adhoc calls)
app.get('/getAdhocCalls', async (req, res) => {
    try {
        const accessToken = await getToken(req);
        // Replace this with your actual resource of listing adhoc calls
        userId="Use Your User ID";
        const endpoint = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/getAllRecordings`;
        const adhocResponse = await axios.get(endpoint, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        });
        const allCalls = adhocResponse.data.value;
        adhocCallDetails = allCalls
            .filter(callObj => callObj.subject && callObj.startTime)
            .map(callObj => ({
                callId: callObj.id,
                subject: callObj.subject,
                start: DateTime.fromISO(callObj.startTime).toFormat('MMM dd h:mm a'),
                end: callObj.endTime ? DateTime.fromISO(callObj.endTime).toFormat('MMM dd h:mm a') : null,
                organizer: callObj.organizer?.emailAddress?.name ?? '',
            }));
        res.send(adhocCallDetails);
    } catch (error) {
        res.status(403).json({ error: error.message });
    }
});

// Get transcript content for an adhoc call
app.get('/getAdhocCallTranscript', async (req, res) => {
    try {
        const accessToken = await getToken(req);
        const { callId, transcriptId } = req.query;
        const endpoint = `https://graph.microsoft.com/beta/me/adhocCalls/${callId}/transcripts/${transcriptId}/content?$format=text/vtt`;
        const transcriptResponse = await getApiData(endpoint, accessToken);
        res.send(transcriptResponse);
    } catch (error) {
        res.status(403).json({ error: error.message });
    }
});

// Get recording content for an adhoc call
app.get('/getAdhocCallRecording', async (req, res) => {
    try {
        const accessToken = await getToken(req);
        const { callId, recordingId } = req.query;
        const endpoint = `https://graph.microsoft.com/beta/me/adhocCalls/${callId}/recordings/${recordingId}/content`;
        const response = await axios.get(endpoint, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
            responseType: 'arraybuffer'
        });

        if (response.status === 200) {
            res.set('Content-Type', 'video/mp4');
            res.set('Content-Disposition', 'inline; filename=video.mp4');
            const videoStream = new Readable();
            videoStream._read = () => { };
            videoStream.push(response.data);
            videoStream.push(null);
            videoStream.pipe(res);
        } else {
            res.status(500).send('Failed to retrieve video content.');
        }
    } catch (error) {
        res.status(403).json({ error: error.message });
    }
});

// ========== SUBSCRIPTION CREATION ==========

async function createAdhocTranscriptSubscription(callId, accessToken) {
    try {
        let existingSubscriptions = null;
        let resource = `communications/adhocCalls/${callId}/transcripts`;
        let notificationUrl = `${baseUrl}/webhookAdhocTranscripts?callId=${callId}`;
        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/beta/subscriptions`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + accessToken
                }
            });
            existingSubscriptions = apiResponse.data.value;
        } catch (ex) { return null; }

        var existingSubscription = existingSubscriptions.find(subscription => subscription.resource === resource);
        if (existingSubscription != null && existingSubscription.notificationUrl != notificationUrl) {
            deleteSubscription(existingSubscription.id, accessToken);
            existingSubscription = null;
        }
        if (existingSubscription == null || existingSubscription.notificationUrl != notificationUrl) {
            const subscription = {
                changeType: 'created',
                notificationUrl: notificationUrl,
                resource: resource,
                expirationDateTime: new Date(Date.now() + 3300000).toISOString() // 1 hour
            };
            token = accessToken;
            const response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`, subscription, {
                headers: { Authorization: `Bearer ${accessToken}` }
            });
            return response.data;
        }
    } catch (error) {
        console.error('Error creating transcript subscription:', error);
    }
}

async function createAdhocRecordingSubscription(callId, accessToken) {
    try {
        let existingSubscriptions = null;
        let resource = `communications/adhocCalls/${callId}/recordings`;
        let notificationUrl = `${baseUrl}/webhookAdhocRecordings?callId=${callId}`;

        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/beta/subscriptions`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + accessToken
                }
            });
            existingSubscriptions = apiResponse.data.value;
        } catch (ex) { return null; }

        var existingSubscription = existingSubscriptions.find(subscription => subscription.resource === resource);
        if (existingSubscription != null && existingSubscription.notificationUrl != notificationUrl) {
            deleteSubscription(existingSubscription.id, accessToken);
            existingSubscription = null;
        }
        if (existingSubscription == null || existingSubscription.notificationUrl != notificationUrl) {
            const subscription = {
                changeType: 'created',
                notificationUrl: notificationUrl,
                resource: resource,
                expirationDateTime: new Date(Date.now() + 3300000).toISOString() // 1 hour
            };
            token = accessToken;
            const response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`, subscription, {
                headers: { Authorization: `Bearer ${accessToken}` }
            });
            return response.data;
        }
    } catch (error) {
        console.error('Error creating recording subscription:', error);
    }
}

// Endpoint to create adhoc call transcript and recording subscriptions
app.post('/createAdhocSubscription', async (req, res) => {
    try {
        const accessToken = await getToken(req);
        const { callId } = req.query;
        await createAdhocTranscriptSubscription(callId, accessToken);
        await createAdhocRecordingSubscription(callId, accessToken);
        res.send({ status: "Subscriptions created/updated" });
    } catch (error) {
        res.status(403).json({ error: error.message });
    }
});

// Fallback for undefined routes
app.get('*', (req, res) => {
    res.status(404).send("Path not defined");
});

const port = process.env.PORT || 5000;
server.listen(port);

console.log('API server is listening on port ' + port);
