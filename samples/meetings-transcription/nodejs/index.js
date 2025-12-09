const app = require("./app");
const express = require('express');
const cors = require('cors');
const GraphHelper = require('./helpers/graphHelper');
const axios = require('axios');

// Create HTTP server
const server = express();
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

const port = process.env.PORT || process.env.port || 3978;

// Helper function to send card to conversation
async function sendCardToConversation(serviceUrl, conversationId, card, activity) {
  try {
    // Get bot token using tenant-specific endpoint
    const tokenResponse = await axios.post(
      `https://login.microsoftonline.com/${process.env.TENANT_ID}/oauth2/v2.0/token`,
      new URLSearchParams({
        grant_type: 'client_credentials',
        client_id: process.env.CLIENT_ID,
        client_secret: process.env.CLIENT_PASSWORD,
        scope: 'https://api.botframework.com/.default'
      }),
      {
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
      }
    );
    
    const token = tokenResponse.data.access_token;
    
    // Send message with card
    const response = await axios.post(
      `${serviceUrl}v3/conversations/${conversationId}/activities`,
      {
        type: 'message',
        from: activity.recipient,
        recipient: activity.from,
        conversation: { id: conversationId },
        attachments: [
          {
            contentType: 'application/vnd.microsoft.card.adaptive',
            content: card
          }
        ]
      },
      {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );
  } catch (error) {
    console.error("Error sending card:", error.message);
  }
}

// Add middleware to handle meeting end events directly
server.use('/api/messages', async (req, res, next) => {
  if (req.body && req.body.type) {
    // Handle task/fetch directly since SDK isn't processing it
    if (req.body.type === 'invoke' && req.body.name === 'task/fetch') {
      try {
        const meetingId = req.body.value?.data?.meetingId;
        const taskModuleUrl = `${process.env.AppBaseUrl}/home?meetingId=${meetingId}`;
        
        const response = {
          task: {
            type: "continue",
            value: {
              title: "Meeting Transcript",
              height: 600,
              width: 600,
              url: taskModuleUrl,
            },
          },
        };
        
        res.status(200).json(response);
        return; // Don't call next() since we handled it
      } catch (error) {
        console.error("Error handling task/fetch:", error);
      }
    }
    
    // Handle meeting end event directly in middleware since SDK doesn't route it
    if (req.body.type === 'event' && req.body.name === 'application/vnd.microsoft.meetingEnd') {
      const meetingId = req.body.channelData?.meeting?.id;
      const conversationId = req.body.conversation?.id;
      const serviceUrl = req.body.serviceUrl;
      
      try {
        // Wait for transcript to be generated (Teams takes time to process)
        setTimeout(async () => {
          const graphHelper = new GraphHelper();
          let result = "";
          
          try {
            // Retry logic: try up to 3 times with delays
            for (let attempt = 1; attempt <= 3; attempt++) {
              result = await graphHelper.GetMeetingTranscriptionsAsync(meetingId);
              
              if (result && result !== "") {
                break;
              } else {
                if (attempt < 3) {
                  await new Promise(resolve => setTimeout(resolve, 10000)); // Wait 10 seconds between retries
                }
              }
            }
          } catch (graphError) {
            console.error("Graph API Error:", graphError.response?.status, graphError.response?.data?.error?.message || graphError.message);
          }
          
          let cleanedResult = "";
          if (result && result !== "") {
            cleanedResult = result.replace("<v", "");
            
            // Store transcript
            const foundIndex = transcriptsDictionary.findIndex((x) => x.id === meetingId);
            if (foundIndex !== -1) {
              transcriptsDictionary[foundIndex].data = cleanedResult;
            } else {
              transcriptsDictionary.push({
                id: meetingId,
                data: cleanedResult
              });
            }
          }
          
        // Send card with transcript
        try {
          const cardToSend = result && result !== "" ? {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "type": "AdaptiveCard",
            "body": [{
              "type": "TextBlock",
              "text": "Meeting Transcript",
              "weight": "Bolder",
              "size": "Large"
            },
            {
              "type": "Container",
              "items": [{
                "type": "TextBlock",
                "text": cleanedResult,
                "wrap": true,
                "maxLines": 5
              }]
            }],
            "actions": [{
              "type": "Action.ShowCard",
              "title": "Show Full Transcript",
              "card": {
                "type": "AdaptiveCard",
                "body": [{
                  "type": "TextBlock",
                  "text": cleanedResult,
                  "wrap": true
                }]
              }
            }]
          } : {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "type": "AdaptiveCard",
            "body": [{
              "type": "TextBlock",
              "text": "Transcript is still being generated. Please wait a few minutes.",
                "weight": "Bolder",
                "size": "Large"
              }]
            };
            
          await sendCardToConversation(serviceUrl, conversationId, cardToSend, req.body);
        } catch (err) {
          console.error("Error sending card:", err.message);
        }
        }, 30000); // Wait 30 seconds before first attempt
      } catch (error) {
        console.error("Error processing meeting transcript:", error);
      }
    }
  }
  
  next();
});

// Config endpoint for meeting tab
server.get('/config', (req, res) => {
    res.send('<html><body><script src="https://res.cdn.office.net/teams-js/2.0.0/js/MicrosoftTeams.min.js"></script><script>microsoftTeams.app.initialize().then(() => { microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => { microsoftTeams.pages.config.setConfig({ entityId: "meetingTranscript", contentUrl: "https://' + req.headers.host + '/home", websiteUrl: "https://' + req.headers.host + '/home" }); saveEvent.notifySuccess(); }); microsoftTeams.pages.config.setValidityState(true); });</script></body></html>');
});

// Returns view to be open in task module
server.get('/home', async (req, res) => {
    try {
        var transcript = "Transcript not found."
        if (req.query?.meetingId) {
            var foundIndex = transcriptsDictionary.findIndex((x) => x.id === req.query?.meetingId);
                
            if (foundIndex != -1) {
                transcript = `Format: ${transcriptsDictionary[foundIndex].data}`;
            }
            else {
                var graphHelper = new GraphHelper();
                var result = await graphHelper.GetMeetingTranscriptionsAsync(req.query?.meetingId);
                if (result != "") {
                    transcriptsDictionary.push({
                        id: req.query?.meetingId,
                        data: result
                    });

                    transcript = `Format: ${result}`;
                }
            }
        }

        res.render('./views/index', { transcript: transcript });
    } catch (error) {
        console.error("Error in /home endpoint:", error.message);
        res.status(500).send(`Error: ${error.message}`);
    }
});

// Start the application
(async () => {
  await app.start(server);
  server.listen(port, () => {
    console.log(`\nBot started, server listening on http://localhost:${port}`);
  });
})();
