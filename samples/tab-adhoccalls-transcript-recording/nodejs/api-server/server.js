const express = require('express');
const bodyParser = require('body-parser');
const app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
const path = require('path');
const axios = require('axios');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const tenantIds = process.env.TENANT_ID;
const userIds = process.env.USER_ID;
const auth = require('./auth'); 
const baseUrl = process.env.BASE_URL;
let eventDetails = [];
const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });

  //uses Axios to make an HTTP GET request to the specified URL with the provided access token
  async function getApiData(url, accessToken, isBinary = false) {
    try {
      const response = await axios.get(url, {
        headers: {
          Authorization: `Bearer ${accessToken}`
        },
        responseType: isBinary ? 'arraybuffer' : 'json'
      });
      
      if (isBinary) {
        return response.data; // Return raw binary data
      } else {
        return JSON.stringify(response.data);
      }
    } catch (error) {
      console.error('Error fetching API data:', error);
    }
  }
  
app.post('/handleAdhocCallTranscriptNotification', async (req, res) => {
    if (req.query && req.query.validationToken) {
        return res.send(req.query.validationToken); // For Graph subscription validation
    }

    const notifications = req.body?.value || [];
    const accessTokenNew = await auth.getAccessToken(tenantIds);
    for (const note of notifications) {
        if (note.resource) {
            const match = note.resource.match(/adhocCalls\/([^/]+)\/transcripts\/([^/]+)/);
            if (match) {
                const callId = match[1];
                const transcriptId = match[2];
                console.log(`CallId: ${callId}, TranscriptId: ${transcriptId}`);

                if (accessTokenNew) {
                    try {
                      const endpoint = `https://graph.microsoft.com/beta/users/${userIds}/adhocCalls/${callId}/transcripts/${transcriptId}/content?$format=text/vtt`; 
                      const transcriptData = await getApiData(endpoint, accessTokenNew);
                      // If endpoint returns JSON
                      let transcriptContent;
                      try {
                          transcriptContent = JSON.parse(transcriptData);
                      } catch {
                          transcriptContent = transcriptData; // fallback to raw
                      }
                      io.emit('transcript', transcriptContent);
                      // Deduplicate storage
                      if (!eventDetails.some(e => e.callId === callId && e.transcriptId === transcriptId)) {
                          eventDetails.push({ callId, transcriptId, content: transcriptContent });
                      }
                    } catch (err) {
                        console.error("Error fetching transcript:", err.message);
                    }
                }
            }
        }
    }
    res.sendStatus(202);
});

app.post('/handleAdhocCallRecordingNotification', async (req, res) => {
    if (req.query && req.query.validationToken) {
        return res.send(req.query.validationToken); // For Graph subscription validation
    }

    const notifications = req.body?.value || [];
    const accessTokenNew = await auth.getAccessToken(tenantIds);

    for (const note of notifications) {
        if (note.resource) {
            const match = note.resource.match(/adhocCalls\/([^/]+)\/recordings\/([^/]+)/);
            if (match) {
                const callId = match[1];
                const recordingId = match[2];
                console.log(`CallId: ${callId}, RecordingId: ${recordingId}`);

                if (accessTokenNew) {
                    // Directly call recording endpoint
                    const endpoint_1 = `https://graph.microsoft.com/beta/users/${userIds}/adhocCalls/${callId}/recordings/${recordingId}/content`;
                    try {
                        const binaryData = await getApiData(endpoint_1, accessTokenNew, true);
                        // Convert to Base64 for safe Socket.IO transfer
                        const base64Video = Buffer.from(binaryData).toString('base64');
                        // Emit to clients
                        io.emit('recording', {
                            callId,
                            recordingId,
                            videoData: base64Video
                        });

                        // Deduplicate storage
                        if (!eventDetails.some(e => e.callId === callId && e.recordingId === recordingId)) {
                            eventDetails.push({ callId, recordingId, videoData: base64Video });
                        }

                    } catch (err) {
                        console.error("Error fetching recordings:", err.message);
                    }
                }
            }
        }
    }

    res.sendStatus(202); // Acknowledge receipt
});

app.post('/createAdhocCallRecordingSubscription', async (req, res) => {
  try {
    const accessToken = await auth.getAccessToken(tenantIds);
    const resource = `/communications/adhocCalls/getAllRecordings`;
    const notificationUrl = baseUrl + '/handleAdhocCallRecordingNotification';

    let existingSubscriptions = [];
    try {
      const apiResponse = await axios.get(`https://graph.microsoft.com/beta/subscriptions`, {
        headers: {
          accept: "application/json",
          "content-type": "application/json",
          authorization: "Bearer " + accessToken
        }
      });
      existingSubscriptions = apiResponse.data.value || [];
    } catch (ex) {
      console.error("Error fetching existing subscriptions:", ex.message);
    }

    let existingSubscription = existingSubscriptions.find(
      subscription => subscription.resource === resource
    );

    if (existingSubscription && existingSubscription.notificationUrl !== notificationUrl) {
      console.log(`Deleting outdated subscription: ${existingSubscription.id}`);
      await deleteSubscription(existingSubscription.id, accessToken);
      existingSubscription = null;
    }

    if (existingSubscription) {
      return res.json({
        status: "already_exists",
        message: "Subscription already exists",
        subscription: existingSubscription
      });
    }

    const subscription = {
      changeType: 'created',
      notificationUrl,
      resource,
      lifecycleNotificationUrl: notificationUrl,
      expirationDateTime: new Date(Date.now() + 3600000).toISOString() // 1 hour
    };

    const response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`, subscription, {
      headers: { Authorization: `Bearer ${accessToken}` }
    });

    console.log('Subscription created:', response.data);

    return res.json({
      status: "created",
      message: "Subscription created successfully",
      subscription: response.data
    });

  } catch (error) {
    console.error("Error creating subscription:", error.message);
    res.status(500).json({ status: "error", message: error.message });
  }
});

app.post('/createAdhocCallTranscriptSubscription', async (req, res) => {
  try {
    const accessToken = await auth.getAccessToken(tenantIds);
    const resource = `/communications/adhocCalls/getAllTranscripts`;
    const notificationUrl = baseUrl + '/handleAdhocCallTranscriptNotification';

    let existingSubscriptions = [];
    try {
      const apiResponse = await axios.get(`https://graph.microsoft.com/beta/subscriptions`, {
        headers: {
          accept: "application/json",
          "content-type": "application/json",
          authorization: "Bearer " + accessToken
        }
      });
      existingSubscriptions = apiResponse.data.value || [];
    } catch (ex) {
      console.error("Error fetching existing subscriptions:", ex.message);
    }

    let existingSubscription = existingSubscriptions.find(
      subscription => subscription.resource === resource
    );

    if (existingSubscription && existingSubscription.notificationUrl !== notificationUrl) {
      console.log(`Deleting outdated subscription: ${existingSubscription.id}`);
      await deleteSubscription(existingSubscription.id, accessToken);
      existingSubscription = null;
    }

    if (existingSubscription) {
      return res.json({
        status: "already_exists",
        message: "Subscription already exists",
        subscription: existingSubscription
      });
    }

    const subscription = {
      changeType: 'created',
      notificationUrl,
      resource,
      lifecycleNotificationUrl: notificationUrl,
      expirationDateTime: new Date(Date.now() + 3600000).toISOString() // 1 hour
    };

    const response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`, subscription, {
      headers: { Authorization: `Bearer ${accessToken}` }
    });

    console.log('Subscription created:', response.data);

    return res.json({
      status: "created",
      message: "Subscription created successfully",
      subscription: response.data
    });

  } catch (error) {
    console.error("Error creating subscription:", error.message);
    res.status(500).json({ status: "error", message: error.message });
  }
});


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
server.listen(port);

console.log('API server is listening on port ' + port);