// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
 * API Endpoint: Fetch Adhoc Call Transcripts and Recordings
 * 
 * Fetches all transcripts and recordings for a specific Microsoft Teams user
 * using the Microsoft Graph Beta API. Transcripts are formatted into readable
 * HTML with speaker labels, and both transcripts and recordings are emitted
 * in real-time via Socket.IO to connected clients.
 * 
 * The endpoint handles:
 * - Retrieving an access token for Microsoft Graph
 * - Fetching all transcripts and recordings for the user
 * - Formatting transcript content from VTT to HTML
 * - Emitting data to clients using Socket.IO
 * - Returning a summary JSON response with counts of transcripts and recordings
 * 
 * @route POST /fetchingTranscriptsandRecordings
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @returns {Object} JSON response containing counts of transcripts and recordings
 */
app.post('/fetchingTranscriptsandRecordings', async (req, res) => {
  try {
    const accessToken = await auth.getAccessToken(tenantIds);
    
    // Fetch all transcripts
    const transcriptsEndpoint = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/getAllTranscripts(userId='${userId}')`;
    const responseDataString = await getApiData(transcriptsEndpoint, accessToken);
    const responseData = JSON.parse(responseDataString);
    
    const allTranscripts = [];
    
    // Process transcripts
    for (const item of responseData.value || []) {
      console.log('Transcript - callId:', item.callId, 'id:', item.id);

      const endpoint = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${item.callId}/transcripts/${item.id}/content?$format=text/vtt`;
      const transcriptData = await getApiData(endpoint, accessToken);
      
      // Format the transcript content
      const regex = /<v\s+([^>]+)>(.*?)<\/v>/g;
      let match;
      const formattedLines = [];

      while ((match = regex.exec(transcriptData)) !== null) {
        const speaker = match[1].trim();
        const text = match[2].trim();
        formattedLines.push(`<b>${speaker}</b> : ${text}`);
      }

      const formattedContent = formattedLines.join("<br/>");
      
      allTranscripts.push({
        callId: item.callId,
        id: item.id,
        content: transcriptData,
        formattedContent: formattedContent
      });
    }
    
    // Emit all transcripts data
    io.emit('allTranscriptsData', allTranscripts);

    // Fetch all recordings
    const accessTokenVideo = await auth.getAccessToken(tenantIds);
    const transcriptsEndpointVideo = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/getAllRecordings(userId='${userId}')`;
    const responseDataStringVideo = await getApiData(transcriptsEndpointVideo, accessTokenVideo);
    const responseDataVideo = JSON.parse(responseDataStringVideo);

    const allRecordings = [];

    // Process recordings
    for (const item of responseDataVideo.value || []) {
      console.log('Recording - callId:', item.callId, 'id:', item.id);
      
      const recordingUrl = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${item.callId}/recordings/${item.id}/content`;
      
      allRecordings.push({
        callId: item.callId,
        recordingId: item.id,
        url: recordingUrl,
        token: accessTokenVideo
      });
    }
    
    // Emit all recordings data
    io.emit('allRecordingsData', allRecordings);
    
    res.json({ 
      status: "success", 
      message: `Found ${allTranscripts.length} transcripts and ${allRecordings.length} recordings`,
      transcriptsCount: allTranscripts.length,
      recordingsCount: allRecordings.length
    });
    
  } catch (error) {
    console.error("Error creating subscription:", error.message);
    res.status(500).json({ status: "error", message: error.message });
  }
});

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