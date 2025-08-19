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
const userId = process.env.USER_ID;
const auth = require('./auth'); 
const baseUrl = process.env.BASE_URL;
let eventDetails = [];
const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });

/**
 * Generic API Data Fetcher
 * 
 * Makes HTTP GET requests to Microsoft Graph API endpoints with proper authentication.
 * Supports both JSON and binary data retrieval based on the response type needed.
 * 
 * @param {string} url - The Microsoft Graph API endpoint URL
 * @param {string} accessToken - Bearer token for API authentication
 * @param {boolean} isBinary - Whether to expect binary data (for recordings) or JSON data
 * @returns {Promise<string|ArrayBuffer>} - JSON string for text data, ArrayBuffer for binary data
 */
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

/**
 * Webhook Endpoint: Handle Adhoc Call Transcript Notifications
 * 
 * This endpoint receives webhook notifications from Microsoft Graph when new transcripts
 * become available for adhoc calls. It processes the notifications, fetches the actual
 * transcript content, and broadcasts it to connected clients via WebSocket.
 * 
 * Workflow:
 * 1. Validate webhook subscription (responds to validation token requests)
 * 2. Extract call and transcript IDs from notification payload
 * 3. Fetch transcript content from Microsoft Graph API
 * 4. Emit transcript data to connected clients via Socket.IO
 * 5. Store transcript data for deduplication
 * 
 * @route POST /handleAdhocCallTranscriptNotification
 * @param {Object} req - Express request object containing webhook payload
 * @param {Object} res - Express response object
 */
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
                      const endpoint = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${callId}/transcripts/${transcriptId}/content?$format=text/vtt`;
                      const transcriptData = await getApiData(endpoint, accessTokenNew);
                      // API returns WebVTT text directly
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

/**
 * Webhook Endpoint: Handle Adhoc Call Recording Notifications
 * 
 * This endpoint receives webhook notifications from Microsoft Graph when new recordings
 * become available for adhoc calls. It processes the notifications, fetches the binary
 * recording data, converts it to Base64 for safe transmission, and broadcasts it to
 * connected clients via WebSocket.
 * 
 * Workflow:
 * 1. Validate webhook subscription (responds to validation token requests)
 * 2. Extract call and recording IDs from notification payload
 * 3. Fetch binary recording data from Microsoft Graph API
 * 4. Convert binary data to Base64 for JSON-safe transmission
 * 5. Emit recording data to connected clients via Socket.IO
 * 6. Store recording data for deduplication
 * 
 * @route POST /handleAdhocCallRecordingNotification
 * @param {Object} req - Express request object containing webhook payload
 * @param {Object} res - Express response object
 */
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
                    try {
                        // Direct recording URL
                    const recordingUrl = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${callId}/recordings/${recordingId}/content`;

                    // Instead of fetching binary data, just send URL + token to client
                    io.emit('recordingAvailable', {
                        callId,
                        recordingId,
                        url: recordingUrl,
                        token: accessTokenNew
                    });
                    // Store event info if you need deduplication
                    if (!eventDetails.some(e => e.callId === callId && e.recordingId === recordingId)) {
                        eventDetails.push({ callId, recordingId });
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

/**
 * API Endpoint: Create Adhoc Call Recording Subscription
 * 
 * Creates a Microsoft Graph webhook subscription to receive notifications when new
 * recordings become available for adhoc calls. This endpoint handles subscription
 * management including checking for existing subscriptions and creating new ones.
 * 
 * Subscription Management Logic:
 * 1. Check for existing subscriptions with the same resource
 * 2. Delete outdated subscriptions (wrong notification URL)
 * 3. Return existing valid subscriptions without creating duplicates
 * 4. Create new subscription if none exists
 * 
 * @route POST /createAdhocCallRecordingSubscription
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @returns {Object} JSON response with subscription status and details
 */
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

/**
 * API Endpoint: Create Adhoc Call Transcript Subscription
 * 
 * Creates a Microsoft Graph webhook subscription to receive notifications when new
 * transcripts become available for adhoc calls. This endpoint handles subscription
 * management including checking for existing subscriptions and creating new ones.
 * 
 * The logic is identical to the recording subscription endpoint but targets
 * transcript resources instead of recording resources.
 * 
 * @route POST /createAdhocCallTranscriptSubscription
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @returns {Object} JSON response with subscription status and details
 */
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

/**
 * Utility Function: Delete Microsoft Graph Subscription
 * 
 * Deletes a Microsoft Graph webhook subscription by its ID. This is used for
 * cleanup when subscriptions become outdated or need to be replaced.
 * 
 * @param {string} subscriptionId - The unique ID of the subscription to delete
 * @param {string} accessToken - Bearer token for Microsoft Graph API authentication
 * @returns {Promise<void>} - Resolves when deletion is complete or fails silently
 */

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

/**
 * Server Startup Configuration
 * 
 * Starts the HTTP server with Socket.IO support on the configured port.
 * Uses environment variable PORT or defaults to 5000 for local development.
 */
app.get('*', (req, res) => {
    console.log("Unhandled request: ", req);
    res.status(404).send("Path not defined");
});

const port = process.env.PORT || 5000;
server.listen(port);

console.log('API server is listening on port ' + port);