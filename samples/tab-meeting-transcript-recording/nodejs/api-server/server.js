const fetch = require('node-fetch');
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
const graphScopes = ['https://graph.microsoft.com/User.Read'];


//This method obtains an access token and makes requests to the Microsoft Graph API to fetch events, online meetings, transcripts, and recordings.
app.get('/GetLoginUserInformation', async (req,res) => {
    const msalClient = new msal.ConfidentialClientApplication({
        auth: {
            clientId: clientId,
            clientSecret: clientSecret
        }
    });
    
    let tenantId = jwt_decode(req.query.ssoToken)['tid']; //Get the tenant ID from the decoded toke

    msalClient.acquireTokenOnBehalfOf({
        authority: `https://login.microsoftonline.com/${tenantId}`,
        oboAssertion: req.query.ssoToken,
        scopes: graphScopes,
        skipCache: true
      })
      .then( async (result) => {    
        var result = await getData(result.accessToken)
        res.send(result);
      })
      .catch(error => {
        console.log("error"+ error.errorCode);
        res.status(403).json({ error: 'consent_required' });
    });
});

// This method is used to fetch meeting transcripts.
app.get('/getMeetingTranscripts', async (req,res) => {
    const msalClient = new msal.ConfidentialClientApplication({
        auth: {
            clientId: clientId,
            clientSecret: clientSecret
        }
    });
    
    let tenantId = jwt_decode(req.query.ssoToken)['tid']; //Get the tenant ID from the decoded toke

    msalClient.acquireTokenOnBehalfOf({
        authority: `https://login.microsoftonline.com/${tenantId}`,
        oboAssertion: req.query.ssoToken,
        scopes: graphScopes,
        skipCache: true
      })
      .then( async (result) => {    
        const graphApiEndpointOnlineTranscriptsData = `https://graph.microsoft.com/beta/me/onlineMeetings/${req.query.meetingId}/transcripts/${req.query.transcriptId}/content?$format=text/vtt`;
        const response = await getApiData(graphApiEndpointOnlineTranscriptsData, result.accessToken);
        res.send(response);
      })
      .catch(error => {
        console.log("error"+ error.errorCode);
        res.status(403).json({ error: 'consent_required' });
    });
});

// This method fetches meeting recordings and passes it as a streamable content
app.get('/getMeetingRecordings', async (req,res) => {
    const msalClient = new msal.ConfidentialClientApplication({
        auth: {
            clientId: clientId,
            clientSecret: clientSecret
        }
    });
    
    let tenantId = jwt_decode(req.query.ssoToken)['tid']; //Get the tenant ID from the decoded toke

    msalClient.acquireTokenOnBehalfOf({
        authority: `https://login.microsoftonline.com/${tenantId}`,
        oboAssertion: req.query.ssoToken,
        scopes: graphScopes,
        skipCache: true
      })
      .then( async (result) => {    
        const graphApiEndpointOnlineRecordData = `https://graph.microsoft.com/beta/me/onlineMeetings/${req.query.meetingId}/recordings/${req.query.recordingId}/content`;
        const response = await axios.get(graphApiEndpointOnlineRecordData, {
          headers: {
            Authorization: `Bearer ${result.accessToken}`
          },
          responseType: 'arraybuffer'
        });
    
        if (response.status === 200) {
           // Set the appropriate response headers for video content
          res.set('Content-Type', 'video/mp4');
          res.set('Content-Disposition', 'inline; filename=video.mp4');

            // Create a Readable stream from the video content
          const videoStream = new Readable();
          videoStream._read = () => {};
          videoStream.push(response.data);
          videoStream.push(null);

      // Pipe the video stream to the response
      videoStream.pipe(res);
        } else {
          console.error('Failed to retrieve video content.');
          res.status(500).send('Failed to retrieve video content.');
        }
      })
      .catch(error => {
        console.log("error"+ error.errorCode);
        res.status(403).json({ error: 'consent_required' });
    });
});

// This method retrieves information about the event details.
async function getData(accessToken) {
    const CardResults = [];
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
              if (element.isOnlineMeeting === true) {
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
    
                    if (responseTranscriptsData && responseTranscriptsData.value.length > 0) {
                      for (const TranscriptsData of responseTranscriptsData.value) {
                        Obj.transcriptsId = TranscriptsData.id;
    
                        // Get transcripts Id
                        const TranscriptsId = TranscriptsData.id;
    
                        const graphApiEndpointOnlineRecordings = `https://graph.microsoft.com/beta/me/onlineMeetings/${onlineMeetingId}/recordings`;
    
                        const responseBodyRecordings = await getApiData(graphApiEndpointOnlineRecordings, accessToken);
                        const responseRecordingsData = JSON.parse(responseBodyRecordings);
    
                        if (responseRecordingsData && responseRecordingsData.value.length > 0) {
                          for (const RecordingsData of responseRecordingsData.value) {
                            Obj.recordingId = RecordingsData.id;
                            Obj.condition = true;
    
                            // Get recordings Id
                            const RecordingId = RecordingsData.id;
                          }
                        }
                      }
                    }
                  }
                }
    
                CardResults.push(Obj);
              }
            }
          }
        }
        return CardResults;
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

// Handles any requests that don't match the ones above
app.get('*', (req,res) =>{
    console.log("Unhandled request: ",req);
    res.status(404).send("Path not defined");
});

const port = process.env.PORT || 5000;
app.listen(port);

console.log('API server is listening on port ' + port);