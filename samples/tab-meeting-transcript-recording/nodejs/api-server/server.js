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
let eventDetails = [];
let token = null;
let eventUpdated = false;

//This method obtains an access token and makes requests to the Microsoft Graph API to fetch events, online meetings, transcripts, and recordings.
app.get('/GetLoginUserInformation', async (req, res) => {
    try {
        const accessToken = await getToken(req);
        const result = await getData(accessToken);
        res.send(result);
    } catch (error) {
        res.status(403).json({ error: error.message });
    }
});

//handles incoming webhook events related to changes in calendar events.
app.post('/webhookEvents', async (req, res, next) => {
    if (req.query && req.query.validationToken) {
        res.send(req.query.validationToken);
    }
    else {
        eventUpdated = true;
        res.status(200).send('OK');
    }
});

//handles incoming webhook events related to changes in meeting transcripts.
app.post('/webhookTranscripts', async (req, res, next) => {
    if (req.query && req.query.validationToken) {
        res.send(req.query.validationToken);
    }
    else {
        var onlineMeetingId = req.query.meetingId;
        if (token != null) {
            const graphApiEndpointOnlineTranscripts = `https://graph.microsoft.com/beta/me/onlineMeetings/${onlineMeetingId}/transcripts`;
            const responseBodyTranscripts = await getApiData(graphApiEndpointOnlineTranscripts, token);
            const responseTranscriptsData = JSON.parse(responseBodyTranscripts);
            eventDetails = eventDetails.map(event => {
                if (event.onlineMeetingId === onlineMeetingId) {
                    if (event.recordingId) {
                        return {
                            ...event, transcriptsId: responseTranscriptsData.value[0].id,
                            condition: true
                        };
                    }
                    else {
                        return {
                            ...event, transcriptsId: responseTranscriptsData.value[0].id
                        };
                    }
                }
                return event;
            });
            res.status(200).send('OK');
        }
    }
});

//handles incoming webhook events related to changes in meeting recordings.
app.post('/webhookRecordings', async (req, res, next) => {
    if (req.query && req.query.validationToken) {
        res.send(req.query.validationToken);
    }
    else {
        var onlineMeetingId = req.query.meetingId;
        if (token != null) {
            const graphApiEndpointOnlineRecordings = `https://graph.microsoft.com/beta/me/onlineMeetings/${onlineMeetingId}/recordings`;
            const responseBodyRecordings = await getApiData(graphApiEndpointOnlineRecordings, token);
            const responseRecordingsData = JSON.parse(responseBodyRecordings);
            eventDetails = eventDetails.map(event => {
                if (event.onlineMeetingId === onlineMeetingId) {
                    if (event.transcriptsId) {
                        return {
                            ...event, recordingId: responseRecordingsData.value[0].id,
                            condition: true
                        };
                    }
                    else {
                        return { ...event, recordingId: responseRecordingsData.value[0].id };
                    }
                }
                return event;
            });
            res.status(200).send('OK');
        }
    }
});

//This method sends back updated events.
app.get('/getUpdatedEvents', async (req, res) => {
    res.send({ eventDetails, eventUpdated });
});

// This method is used to fetch meeting transcripts.
app.get('/getMeetingTranscripts', async (req, res) => {
    try {
        const accessToken = await getToken(req);
        const graphApiEndpointOnlineTranscriptsData = `https://graph.microsoft.com/beta/me/onlineMeetings/${req.query.meetingId}/transcripts/${req.query.transcriptId}/content?$format=text/vtt`;
        const response = await getApiData(graphApiEndpointOnlineTranscriptsData, accessToken);
        res.send(response);
    } catch (error) {
        res.status(403).json({ error: error.message });
    }
});

// This method fetches meeting recordings and passes it as a streamable content
app.get('/getMeetingRecordings', async (req, res) => {
    try {
        const accessToken = await getToken(req);
        const graphApiEndpointOnlineRecordData = `https://graph.microsoft.com/beta/me/onlineMeetings/${req.query.meetingId}/recordings/${req.query.recordingId}/content`;
        const response = await axios.get(graphApiEndpointOnlineRecordData, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
            responseType: 'arraybuffer'
        });

        if (response.status === 200) {
            // Set the appropriate response headers for video content
            res.set('Content-Type', 'video/mp4');
            res.set('Content-Disposition', 'inline; filename=video.mp4');

            // Create a Readable stream from the video content
            const videoStream = new Readable();
            videoStream._read = () => { };
            videoStream.push(response.data);
            videoStream.push(null);

            // Pipe the video stream to the response
            videoStream.pipe(res);
        } else {
            console.error('Failed to retrieve video content.');
            res.status(500).send('Failed to retrieve video content.');
        }
    } catch (error) {
        res.status(403).json({ error: error.message });
    }
});

//creates subscriptions for Microsoft Graph calendar events and fetches existing subscriptions.
app.post('/createsubscription', async (req, res) => {
    try {
        const accessToken = await getToken(req);
        let existingSubscriptions = null;
        let resource = '/me/events';
        let notificationUrl = baseUrl + '/webhookEvents'

        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/v1.0/subscriptions`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + accessToken
                }
            });
            existingSubscriptions = apiResponse.data.value;
        }
        catch (ex) {
            return null;
        }

        var existingSubscription = existingSubscriptions.find(subscription => subscription.resource === resource);

        if (existingSubscription != null && existingSubscription.notificationUrl != notificationUrl) {
            console.log(`CreateNewSubscription-ExistingSubscriptionFound: ${resource}`);
            deleteSubscription(existingSubscription.id, accessToken);
            existingSubscription = null;
        }

        if (existingSubscription == null || existingSubscription.notificationUrl != notificationUrl) {
            const subscription = {
                changeType: 'created,updated,deleted',
                notificationUrl: notificationUrl,
                resource: resource,
                expirationDateTime: new Date(Date.now() + 3600000).toISOString() // 12 hours
            };

            const response = await axios.post(`https://graph.microsoft.com/v1.0/subscriptions`, subscription, {
                headers: {
                    Authorization: `Bearer ${accessToken}`
                }
            });

            console.log('Subscription created:', response.data);
            return response.data;
        }
    } catch (error) {
        res.status(403).json({ error: error.message });
    }
});

// This method retrieves information about the event details.
async function getData(accessToken) {
    eventDetails = [];
    eventUpdated = false;
    try {
        const graphApiEndpointEvents = 'https://graph.microsoft.com/beta/me/events';
        const response = await axios.get(graphApiEndpointEvents, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        });

        if (response.status === 200) {
            const responseData = response.data;
            if (responseData.value.length > 0) {
                const allEvents = responseData.value;
                for (const element of allEvents) {
                    if (element.isOnlineMeeting === true && element.subject != "" && DateTime.now().minus({ months: 1 }) <= DateTime.fromISO(element.start.dateTime)) {
                        const Obj = {
                            subject: element.subject,
                            start: DateTime.fromISO(element.start.dateTime).toFormat('MMM dd h:mm a'),
                            end: DateTime.fromISO(element.end.dateTime).toFormat('MMM dd h:mm a'),
                            organizer: element.organizer.emailAddress.name
                        };

                        // Get Join URL
                        const joinUrl = element.onlineMeeting.joinUrl;
                        const graphApiEndpointJoinUrl = `https://graph.microsoft.com/v1.0/me/onlineMeetings?$filter=JoinWebUrl%20eq%20'${joinUrl}'`;

                        const responseBodyJoinUrl = await getApiData(graphApiEndpointJoinUrl, accessToken);
                        const responseJoinUrlData = JSON.parse(responseBodyJoinUrl);

                        if (responseJoinUrlData && responseJoinUrlData.value.length > 0) {
                            for (const JoinWebUrlData of responseJoinUrlData.value) {
                                Obj.onlineMeetingId = JoinWebUrlData.id;

                                // Get OnlineMeetingId
                                const onlineMeetingId = JoinWebUrlData.id;
                                const graphApiEndpointOnlineTranscripts = `https://graph.microsoft.com/beta/me/onlineMeetings/${onlineMeetingId}/transcripts`;

                                const responseBodyTranscripts = await getApiData(graphApiEndpointOnlineTranscripts, accessToken);
                                const responseTranscriptsData = JSON.parse(responseBodyTranscripts);

                                const graphApiEndpointOnlineRecordings = `https://graph.microsoft.com/beta/me/onlineMeetings/${onlineMeetingId}/recordings`;

                                const responseBodyRecordings = await getApiData(graphApiEndpointOnlineRecordings, accessToken);
                                const responseRecordingsData = JSON.parse(responseBodyRecordings);

                                if (responseTranscriptsData && responseTranscriptsData.value.length > 0) {
                                    for (const TranscriptsData of responseTranscriptsData.value) {
                                        Obj.transcriptsId = TranscriptsData.id;
                                    }
                                }
                                else {
                                    createTranscriptSubscription(Obj.onlineMeetingId, accessToken);
                                }

                                if (responseRecordingsData && responseRecordingsData.value.length > 0) {
                                    for (const RecordingsData of responseRecordingsData.value) {
                                        Obj.recordingId = RecordingsData.id;
                                        if (Obj.transcriptsId != null)
                                            Obj.condition = true;
                                    }
                                }
                                else {
                                    createRecordingSubscription(Obj.onlineMeetingId, accessToken);
                                }
                            }
                        }
                        eventDetails.push(Obj);
                    }
                }
            }
        }

        return eventDetails;
    } catch (error) {
        console.error('Error fetching event data:', error);
        throw error;
    }
}

//uses Axios to make an HTTP GET request to the specified URL with the provided access token
async function getApiData(url, accessToken) {
    try {
        const response = await axios.get(url, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        });
        return JSON.stringify(response.data);
    } catch (error) {
        console.error('Error fetching API data:', error);
        //throw error;
    }
}

//creates subscriptions for meeting transcripts
async function createTranscriptSubscription(onlineMeetingId, accessToken) {
    try {
        let existingSubscriptions = null;
        let resource = "communications/onlineMeetings/" + onlineMeetingId + "/transcripts";
        let notificationUrl = baseUrl + '/webhookTranscripts?meetingId=' + onlineMeetingId;
        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/v1.0/subscriptions`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + accessToken
                }
            });
            existingSubscriptions = apiResponse.data.value;
        }
        catch (ex) {
            return null;
        }

        var existingSubscription = existingSubscriptions.find(subscription => subscription.resource === resource);

        if (existingSubscription != null && existingSubscription.NotificationUrl != notificationUrl) {
            console.log(`CreateNewSubscription-ExistingSubscriptionFound: ${resource}`);
            deleteSubscription(existingSubscription.id, accessToken);
            existingSubscription = null;
        }

        const subscription = {
            changeType: 'created',
            notificationUrl: notificationUrl,
            resource: resource,
            expirationDateTime: new Date(Date.now() + 3300000).toISOString() // 1 hour
        };

        token = accessToken;

        const response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`, subscription, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        });

        console.log('Subscription created:', response.data);
        return response.data;
    } catch (error) {
        console.error('Error fetching API data:', error);
        //throw error;
    }
}

//creates subscriptions for meeting recording
async function createRecordingSubscription(onlineMeetingId, accessToken) {
    try {
        let existingSubscriptions = null;
        let resource = "communications/onlineMeetings/" + onlineMeetingId + "/recordings";
        let notificationUrl = baseUrl + '/webhookRecordings?meetingId=' + onlineMeetingId;
        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/v1.0/subscriptions`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + accessToken
                }
            });
            existingSubscriptions = apiResponse.data.value;
        }
        catch (ex) {
            return null;
        }

        var existingSubscription = existingSubscriptions.find(subscription => subscription.resource === resource);

        if (existingSubscription != null && existingSubscription.NotificationUrl != notificationUrl) {
            console.log(`CreateNewSubscription-ExistingSubscriptionFound: ${resource}`);
            deleteSubscription(existingSubscription.id, accessToken);
            existingSubscription = null;
        }

        const subscription = {
            changeType: 'created',
            notificationUrl: notificationUrl,
            resource: resource,
            expirationDateTime: new Date(Date.now() + 3300000).toISOString() // 1 hour
        };

        token = accessToken;

        const response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`, subscription, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        });

        console.log('Subscription created:', response.data);
        return response.data;
    } catch (error) {
        console.error('Error fetching API data:', error);
        //throw error;
    }
}

// Define a function to get the access token
async function getToken(req) {
    const msalClient = new msal.ConfidentialClientApplication({
        auth: {
            clientId: clientId,
            clientSecret: clientSecret
        }
    });

    let tenantId = jwt_decode(req.query.ssoToken)['tid']; // Get the tenant ID from the decoded token

    try {
        const result = await msalClient.acquireTokenOnBehalfOf({
            authority: `https://login.microsoftonline.com/${tenantId}`,
            oboAssertion: req.query.ssoToken,
            scopes: graphScopes,
            skipCache: true
        });

        return result.accessToken;
    } catch (error) {
        console.log("error: " + error.errorCode);
        throw new Error('consent_required');
    }
}

//used to delete a subscription by its subscriptionId in the Microsoft Graph API
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
        //throw error;
    }
}

// Handles any requests that don't match the ones above
app.get('*', (req, res) => {
    console.log("Unhandled request: ", req);
    res.status(404).send("Path not defined");
});

const port = process.env.PORT || 5000;
app.listen(port);

console.log('API server is listening on port ' + port);