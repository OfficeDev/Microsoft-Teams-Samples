// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const express = require('express');
const bodyParser = require('body-parser');
const axios = require('axios');
const path = require('path');
require('dotenv').config({ path: path.join(__dirname, '.env') });
const auth = require('./auth');

const app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });

const tenantIds = process.env.TENANT_ID;
const userId = process.env.USER_ID;

/**
 * Fetches data from a specified API endpoint using an access token for authentication.
 *
 * This function performs an HTTP GET request to a Microsoft Graph API (or any other API) endpoint.
 * It supports retrieving both JSON and binary (e.g., file, recording) responses depending on the use case.
 *
 * @async
 * @function getApiData
 * @param {string} url - The API endpoint URL to fetch data from.
 * @param {string} accessToken - The bearer token used for authorization in the request header.
 * @param {boolean} [isBinary=false] - Optional flag indicating whether to expect binary data (true) or JSON (false).
 * @returns {Promise<string|ArrayBuffer>} - Resolves with a JSON string for text responses, or an ArrayBuffer for binary responses.
 * @throws {Error} Logs an error message if the request fails.
 */
async function getApiData(url, accessToken, isBinary = false) {
  try {
    const response = await axios.get(url, {
      headers: { Authorization: `Bearer ${accessToken}` },
      responseType: isBinary ? 'arraybuffer' : 'json'
    });
    return isBinary ? response.data : JSON.stringify(response.data);
  } catch (error) {
    console.error('Error fetching API data:', error);
  }
}

/**
 * POST /fetchingTranscriptsandRecordings
 *
 * Fetches paginated transcript and recording data for a specific user’s adhoc calls
 * from Microsoft Graph API. Handles pagination using @odata.nextLink and emits data
 * in real-time to connected clients via Socket.IO.
 *
 * Features:
 * - Retrieves paged transcript and recording data (default: 10 items per page)
 * - Extracts and formats transcript content into readable text
 * - Emits both transcript and recording data to connected clients
 * - Supports next page loading through Graph API’s @odata.nextLink
 *
 * Request Body Parameters:
 * @param {boolean} [isNextPage=false] - Indicates whether to fetch the next page of data.
 * @param {string|null} [transcriptsNextLink=null] - Next page link for transcripts (if available).
 * @param {string|null} [recordingsNextLink=null] - Next page link for recordings (if available).
 *
 * Emits:
 * - `transcriptsData`: Formatted transcript data, pagination info.
 * - `recordingsData`: Recording metadata with playback URLs and tokens.
 *
 * Response:
 * @returns {Object} JSON response containing:
 *  - status: "success" or "error"
 *  - message: Summary of fetched items
 *  - transcripts: { data, count, hasNext, nextLink }
 *  - recordings: { data, count, hasNext, nextLink }
 *  - pagination: { pageSize, isNextPage }
 *
 * Example Request:
 * POST /fetchingTranscriptsandRecordings
 * {
 *   "isNextPage": false,
 *   "transcriptsNextLink": null,
 *   "recordingsNextLink": null
 * }
 */
app.post('/fetchingTranscriptsandRecordings', async (req, res) => {
  try {
    const accessToken = await auth.getAccessToken(tenantIds);
    const { isNextPage = false, transcriptsNextLink = null, recordingsNextLink = null } = req.body;
    const pageSize = 10;
    
    let allTranscripts = [];
    let allRecordings = [];
    
    // Fetch transcripts
    const transcriptsEndpoint = transcriptsNextLink || 
      `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/getAllTranscripts(userId='${userId}')?$top=${pageSize}`;
    
    const responseDataString = await getApiData(transcriptsEndpoint, accessToken);
    const responseData = JSON.parse(responseDataString);
    const transcriptsNext = responseData['@odata.nextLink'] || null;
    
    // Process transcripts
    for (const item of responseData.value || []) {
      const endpoint = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${item.callId}/transcripts/${item.id}/content?$format=text/vtt`;
      const transcriptData = await getApiData(endpoint, accessToken);
      
      const regex = /<v\s+([^>]+)>(.*?)<\/v>/g;
      const formattedLines = [];
      let match;

      while ((match = regex.exec(transcriptData)) !== null) {
        formattedLines.push(`<b>${match[1].trim()}</b> : ${match[2].trim()}`);
      }
      
      allTranscripts.push({
        callId: item.callId,
        id: item.id,
        content: transcriptData,
        formattedContent: formattedLines.join("<br/>")
      });
    }

    // Fetch recordings
    const recordingsEndpoint = recordingsNextLink || 
      `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/getAllRecordings(userId='${userId}')?$top=${pageSize}`;
    
    const responseDataStringVideo = await getApiData(recordingsEndpoint, accessToken);
    const responseDataVideo = JSON.parse(responseDataStringVideo);
    const recordingsNext = responseDataVideo['@odata.nextLink'] || null;

    // Process recordings
    for (const item of responseDataVideo.value || []) {      
      allRecordings.push({
        callId: item.callId,
        recordingId: item.id,
        url: `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${item.callId}/recordings/${item.id}/content`,
        token: accessToken
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
 * POST /fetchNextPage
 *
 * Fetches the next page of transcript and recording data for a specific user's adhoc calls
 * from Microsoft Graph API using the provided @odata.nextLink URLs.
 * This endpoint supports pagination and emits the additional data to connected clients in real-time.
 *
 * Features:
 * - Retrieves paginated next-page transcript and recording data.
 * - Formats transcript content for display.
 * - Emits next page data to clients via Socket.IO.
 *
 * Request Body Parameters:
 * @param {string|null} transcriptsNextLink - The @odata.nextLink URL for fetching the next transcripts page.
 * @param {string|null} recordingsNextLink - The @odata.nextLink URL for fetching the next recordings page.
 *
 * Emits:
 * - `nextTranscriptsData`: Contains newly fetched transcript data and pagination info.
 * - `nextRecordingsData`: Contains newly fetched recording data and pagination info.
 *
 * Response:
 * @returns {Object} JSON response containing:
 *  - status: "success" or "error"
 *  - message: Summary of fetched additional items
 *  - transcripts: { data, count, hasNext, nextLink }
 *  - recordings: { data, count, hasNext, nextLink }
 *
 * Example Request:
 * POST /fetchNextPage
 * {
 *   "transcriptsNextLink": "https://graph.microsoft.com/beta/users/.../adhocCalls/getAllTranscripts?...",
 *   "recordingsNextLink": "https://graph.microsoft.com/beta/users/.../adhocCalls/getAllRecordings?..."
 * }
 */
app.post('/fetchNextPage', async (req, res) => {
  try {
    const accessToken = await auth.getAccessToken(tenantIds);
    const { transcriptsNextLink, recordingsNextLink } = req.body;
    
    let allTranscripts = [];
    let allRecordings = [];
    let newTranscriptsNext = null;
    let newRecordingsNext = null;
    
    // Fetch next transcripts
    if (transcriptsNextLink) {
      const responseDataString = await getApiData(transcriptsNextLink, accessToken);
      const responseData = JSON.parse(responseDataString);
      newTranscriptsNext = responseData['@odata.nextLink'] || null;
      
      for (const item of responseData.value || []) {
        const endpoint = `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${item.callId}/transcripts/${item.id}/content?$format=text/vtt`;
        const transcriptData = await getApiData(endpoint, accessToken);
        
        const regex = /<v\s+([^>]+)>(.*?)<\/v>/g;
        const formattedLines = [];
        let match;

        while ((match = regex.exec(transcriptData)) !== null) {
          formattedLines.push(`<b>${match[1].trim()}</b> : ${match[2].trim()}`);
        }
        
        allTranscripts.push({
          callId: item.callId,
          id: item.id,
          content: transcriptData,
          formattedContent: formattedLines.join("<br/>")
        });
      }
    }

    // Fetch next recordings
    if (recordingsNextLink) {
      const responseDataStringVideo = await getApiData(recordingsNextLink, accessToken);
      const responseDataVideo = JSON.parse(responseDataStringVideo);
      newRecordingsNext = responseDataVideo['@odata.nextLink'] || null;

      for (const item of responseDataVideo.value || []) {        
        allRecordings.push({
          callId: item.callId,
          recordingId: item.id,
          url: `https://graph.microsoft.com/beta/users/${userId}/adhocCalls/${item.callId}/recordings/${item.id}/content`,
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
 * GET *
 *
 * Catch-all route handler for undefined GET endpoints.
 * Returns a 404 response with a simple message indicating that the path is not defined.
 *
 * This ensures that any unrecognized or invalid GET requests
 * are gracefully handled instead of causing unexpected behavior.
 */
app.get('*', (req, res) => {
  res.status(404).send("Path not defined");
});

const port = process.env.PORT || 5000;
server.listen(port);
console.log('API server is listening on port ' + port);