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
 * API Endpoint: Fetch Existing Adhoc Call Transcripts and Recordings with Pagination
 * 
 * Retrieves existing transcripts and recordings for a specific Microsoft Teams user
 * using the Microsoft Graph Beta API with pagination support. Shows 10 items initially,
 * then 4 items per page for subsequent requests.
 * 
 * The endpoint handles:
 * - Retrieving an access token for Microsoft Graph API
 * - Fetching transcripts and recordings with pagination support
 * - Formatting transcript content from VTT to HTML with speaker identification
 * - Emitting processed data to clients using Socket.IO
 * - Returning pagination information including nextLink for both transcripts and recordings
 * 
 * @route POST /fetchingTranscriptsandRecordings
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @returns {Object} JSON response containing transcripts, recordings, and pagination info
 */
app.post('/fetchingTranscriptsandRecordings', async (req, res) => {
  try {
    const accessToken = await auth.getAccessToken(tenantIds);
    const { isNextPage = false, transcriptsNextLink = null, recordingsNextLink = null } = req.body;
    
    // Determine page size: 10 for both first page and subsequent pages
    const pageSize = 5;
    
    let allTranscripts = [];
    let allRecordings = [];
    let transcriptsNext = null;
    let recordingsNext = null;
    
    // Fetch transcripts
    let transcriptsEndpoint;
    if (transcriptsNextLink) {
      transcriptsEndpoint = transcriptsNextLink;
    } else {
      transcriptsEndpoint = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/getAllTranscripts(userId='${userId}')?$top=${pageSize}`;
    }
    
    const responseDataString = await getApiData(transcriptsEndpoint, accessToken);
    const responseData = JSON.parse(responseDataString);
    
    // Store nextLink for transcripts
    transcriptsNext = responseData['@odata.nextLink'] || null;
    
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

    // Fetch recordings
    let transcriptsEndpointVideo;
    if (recordingsNextLink) {
      transcriptsEndpointVideo = recordingsNextLink;
    } else {
      transcriptsEndpointVideo = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/getAllRecordings(userId='${userId}')?$top=${pageSize}`;
    }
    
    const responseDataStringVideo = await getApiData(transcriptsEndpointVideo, accessToken);
    const responseDataVideo = JSON.parse(responseDataStringVideo);
    
    // Store nextLink for recordings
    recordingsNext = responseDataVideo['@odata.nextLink'] || null;

    // Process recordings
    for (const item of responseDataVideo.value || []) {
      console.log('Recording - callId:', item.callId, 'id:', item.id);
      
      const recordingUrl = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${item.callId}/recordings/${item.id}/content`;
      
      allRecordings.push({
        callId: item.callId,
        recordingId: item.id,
        url: recordingUrl,
        token: accessToken // Token needed to access the recording
      });
    }
    
    // Emit data to clients
    io.emit('transcriptsData', {
      transcripts: allTranscripts,
      hasNext: !!transcriptsNext,
      nextLink: transcriptsNext,
      isNextPage: isNextPage
    });
    
    io.emit('recordingsData', {
      recordings: allRecordings,
      hasNext: !!recordingsNext,
      nextLink: recordingsNext,
      isNextPage: isNextPage
    });
    
    res.json({ 
      status: "success", 
      message: `Found ${allTranscripts.length} transcripts and ${allRecordings.length} recordings`,
      transcripts: {
        data: allTranscripts,
        count: allTranscripts.length,
        hasNext: !!transcriptsNext,
        nextLink: transcriptsNext
      },
      recordings: {
        data: allRecordings,
        count: allRecordings.length,
        hasNext: !!recordingsNext,
        nextLink: recordingsNext
      },
      pagination: {
        pageSize: pageSize,
        isNextPage: isNextPage
      }
    });
    
  } catch (error) {
    console.error("Error fetching transcripts and recordings:", error.message);
    res.status(500).json({ status: "error", message: error.message });
  }
});

/**
 * API Endpoint: Fetch Next Page of Transcripts and Recordings
 * 
 * Handles pagination for transcripts and recordings using @odata.nextLink.
 * This endpoint is specifically designed for "Next" button functionality.
 * 
 * @route POST /fetchNextPage
 * @param {Object} req - Express request object with nextLinks
 * @param {Object} res - Express response object
 * @returns {Object} JSON response containing next page data and pagination info
 */
app.post('/fetchNextPage', async (req, res) => {
  try {
    const accessToken = await auth.getAccessToken(tenantIds);
    const { transcriptsNextLink, recordingsNextLink } = req.body;
    
    let allTranscripts = [];
    let allRecordings = [];
    let newTranscriptsNext = null;
    let newRecordingsNext = null;
    
    // Fetch next transcripts if nextLink provided
    if (transcriptsNextLink) {
      const responseDataString = await getApiData(transcriptsNextLink, accessToken);
      const responseData = JSON.parse(responseDataString);
      
      newTranscriptsNext = responseData['@odata.nextLink'] || null;
      
      // Process transcripts
      for (const item of responseData.value || []) {
        console.log('Next Transcript - callId:', item.callId, 'id:', item.id);

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
    }

    // Fetch next recordings if nextLink provided
    if (recordingsNextLink) {
      const responseDataStringVideo = await getApiData(recordingsNextLink, accessToken);
      const responseDataVideo = JSON.parse(responseDataStringVideo);
      
      newRecordingsNext = responseDataVideo['@odata.nextLink'] || null;

      // Process recordings
      for (const item of responseDataVideo.value || []) {
        console.log('Next Recording - callId:', item.callId, 'id:', item.id);
        
        const recordingUrl = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${item.callId}/recordings/${item.id}/content`;
        
        allRecordings.push({
          callId: item.callId,
          recordingId: item.id,
          url: recordingUrl,
          token: accessToken
        });
      }
    }
    
    // Emit next page data to clients
    io.emit('nextTranscriptsData', {
      transcripts: allTranscripts,
      hasNext: !!newTranscriptsNext,
      nextLink: newTranscriptsNext
    });
    
    io.emit('nextRecordingsData', {
      recordings: allRecordings,
      hasNext: !!newRecordingsNext,
      nextLink: newRecordingsNext
    });
    
    res.json({ 
      status: "success", 
      message: `Found ${allTranscripts.length} more transcripts and ${allRecordings.length} more recordings`,
      transcripts: {
        data: allTranscripts,
        count: allTranscripts.length,
        hasNext: !!newTranscriptsNext,
        nextLink: newTranscriptsNext
      },
      recordings: {
        data: allRecordings,
        count: allRecordings.length,
        hasNext: !!newRecordingsNext,
        nextLink: newRecordingsNext
      }
    });
    
  } catch (error) {
    console.error("Error fetching next page:", error.message);
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