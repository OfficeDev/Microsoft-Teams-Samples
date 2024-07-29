// <copyright file="index.js" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>


// Import required packages
const restify = require("restify");

// This bot's adapter
const adapter = require("./adapter");

// This bot's main dialog.
const app = require("./app/app");

// serever.js
 const serverAPI = require("./serverAPI");

 // database.js
  const database = require("./database");

 const { DecryptionHelper } = require("./helper/decryption-helper");

 const { BotFrameworkAdapter, MemoryStorage, TurnContext, ConsoleTranscriptLogger } = require('botbuilder');


 const { ConnectorClient, MicrosoftAppCredentials } = require('botframework-connector');
const { AzurePowerShellCredential } = require("@azure/identity");
 
const config = require("./config");

const credentials = new MicrosoftAppCredentials(process.env.BOT_ID,process.env.BOT_PASSWORD);

const { Application, ActionPlanner, OpenAIModel, PromptManager } = require("@microsoft/teams-ai");

const { v4: uuidv4 } = require('uuid');

const path = require("path"); 

const { userInfo } = require("os");

const appInsights = require('applicationinsights');

// Configure Application Insights with your instrumentation key or connection string
const instrumentationKey = config.APPINSIGHTS_INSTRUMENTATIONKEY;

// Or use connection string
const connectionString = config.APPINSIGHTS_CONNECTIONSTRING;

appInsights.setup(connectionString || instrumentationKey)
    .setAutoCollectRequests(true)
    .setAutoCollectPerformance(true, true)
    .setAutoCollectExceptions(true)
    .setAutoCollectDependencies(true)
    .setAutoCollectConsole(true, true)
    .setUseDiskRetryCaching(true)
    .start();
    
const client = appInsights.defaultClient;

const model = new OpenAIModel({
  azureApiKey: config.azureOpenAIKey,
  azureDefaultDeployment: config.azureOpenAIDeploymentName,
  azureEndpoint: config.azureOpenAIEndpoint,

  useSystemMessages: true,
  logRequests: true,
});

// Create HTTP server.
const server = restify.createServer();
server.use(restify.plugins.bodyParser());

server.listen(process.env.port || process.env.PORT || 3978, () => {
  console.log(`\nBot Started, ${server.name} listening to ${server.url}`);
});
 
// Store conversation references
const conversationReferences = {};

// Simulated in-memory storage for request IDs
const processedRequests = new Set();

function alreadyProcessed(requestId) {
  return processedRequests.has(requestId);
}

function markAsProcessed(requestId) {
  processedRequests.add(requestId);
} 

// This function is designed for a webhook that triggers when a meeting begins or ends within a particular subscription.
server.post("/EventHandler", async (req, res) => {
  if(req.url === "/EventHandler")
  {
    var notification = req.body.value;
    var responsePayload = await DecryptionHelper.processEncryptedNotification(notification);
    var requestId  = "";
    
    client.trackEvent({ name: "CallEnded line # 104", properties: { EventHandler: responsePayload.eventType } });
            
    if(responsePayload.eventType === "Microsoft.Communication.CallEnded")
      { 
        client.trackEvent({ name: "CallEnded line # 108", properties: { EventHandler: responsePayload.eventType } });
            
         requestId = req.rawHeaders[3] + req.body.value[0].resource; 

        if (alreadyProcessed(requestId)) {
          client.trackEvent({ name: "alreadyProcessed", properties: { EventHandler: `Request ${req.body.value[0].resource} already processed` } });
          console.log(`Request ${requestId} already processed`);
          return res.status(200).end();
        }

        client.trackEvent({ name: "Calling fnction meeting Transcription after hook", properties: { EventHandler: "getMeetingTranscription" } });
            
         // clientState is the combination of UserId | TenantId 
         // encryptionCertificateId is the MeetinId coming from subscription even 
        await serverAPI.getMeetingTranscription(req.body.value[0].resource, req.body.value[0].clientState.split('|')[0], req.body.value[0].clientState.split('|')[1], req.body.value[0].encryptedContent.encryptionCertificateId)
        .then(async meetingDetails => {
           
             if (alreadyProcessed(requestId)) {
              client.trackEvent({ name: "alreadyProcessed", properties: { EventHandler: `Request ${req.body.value[0].resource} already processed` } });
              console.log(`Request ${requestId} already processed`);
              return res.status(200).end();
            }
    
            // Mark this request ID as processed
            markAsProcessed(requestId); 

            // Process the webhook payload
            console.log('Processing webhook payload:', req.body);
          
            client.trackEvent({ name: "meetingDetails", properties: { EventHandler: meetingDetails } });
            
            // Call graph API        
            serverAPI.getMeetingDetailsUsingSubscription(req.body.value[0].resource, req.body.value[0].clientState.split('|')[0], req.body.value[0].clientState.split('|')[1])
            .then(async onlineMeetingDetail => {
              
            
              client.trackEvent({ name: "MeetingEnd", properties: { getMeetingDetailsUsingSubscription: meetingDetails } });
              
              // get User and meeting details
              const UserInfo =  GetUserInformationAndSendActionItems(req.body.value[0].resource,  meetingDetails, onlineMeetingDetail, req.body.value[0].subscriptionId);
            
            });
     
            return res.status(200).end();
        });

        client.trackEvent({ name: "getMeetingTranscription", properties: { EventHandler: requestId } });
            
  }
}
 else if (req.url != undefined && req.url.split('=').length>-1) {
    let url = decodeURIComponent(req.url.split('=')[1]);
    url = url.replace(/\+/g, ' ');
    res.send(url);
  }
  else {
      eventUpdated = true;
      res.status(200).send('OK');
  }
});

// This is for listening to all messages from the bot.
server.post("/api/messages", async (req, res) => { 
  // Route received a request to adapter for processing
    await adapter.process(req, res, async (context) => {
      if (context.activity.type === 'message') { 
        if(context.activity.text.indexOf('upcoming')>-1)
          { 
            const meetings = await serverAPI.getTodaysMeetingAgenda(req,context);
            // Your message handling logic goes here
            await context.sendActivity({ attachments: [meetings] });
          }       
      }
      else if (context.activity.type === 'invoke') {  
          
          let subscription = "";
          try
          {
            await context.sendActivity({
              type: 'invokeResponse',
              value: {
                  status: 200,
                  body: {
                      message: "action proceeded"
                  }
              }
            });   

            subscription = await serverAPI.createSubscription(context.activity.value.joinUrl, context.activity.from.aadObjectId , context.activity.conversation.id, context.activity.conversation.tenantId, context.activity.value.endTime,context.activity.value.meetingId);
          }
          catch(ex)
          {
            await context.sendActivity("subscription: "+ ex.message);
          }
          try
          {
            await context.sendActivity({
              type: 'invokeResponse',
              value: {
                  status: 200,
                  body: {
                      message: "action proceeded"
                  }
              }
            }); 

          const saving = await SaveUserAndMeetingDetailsForSubscription(context.activity.value.joinUrl, context.activity.from.aadObjectId, context.activity.conversation.id, subscription.split('|')[0], context.activity.conversation.tenantId);
            
            await context.sendActivity({
              type: 'invokeResponse',
              value: {
                  status: 200,
                  body: {
                      message: "action proceeded"
                  }
              }
            });  
          }
          catch(ex)
          {
            console.log(ex.message);
          }
         
          if(subscription.split('|').length>1)
          {
            await context.sendActivity({
              type: 'invokeResponse',
              value: {
                  status: 200,
                  body: {
                      message: subscription.split('|')[1]
                  }
              }
            });   
            
            await context.sendActivity(subscription.split('|')[1]);
          }
      }
      else
      {
        // Dispatch to application for routing
        await app.run(context);
      }      
    });
});

// Function to store all meeting and user information related to subscriptions.
async function SaveUserAndMeetingDetailsForSubscription(joinWebUrl, userId, conversationId, subscriptionId, tenantId)
{  
  try
  {
      let OnlineMeetingId = "";
      let UserInformation = "";

        try
        {
          // get meeting Id using Join Web URL 
          OnlineMeetingId = await serverAPI.getMeetingDetailsUsingSubscription(`/communications/onlineMeetings/?$filter=JoinWebUrl eq '${joinWebUrl}'`, userId, tenantId);    
        }
        catch(ex)
        {
          console.log (ex);
        }

        // Get User Information for saving
        UserInformation = await serverAPI.getUserInformation(userId, tenantId);
      
        // call function to save.
        let data = { onlineMeetingId: OnlineMeetingId.id, 
          conversationId: conversationId,
          subscriptionId: subscriptionId,
          userId: userId,
          displayName: UserInformation.displayName,
          mail: UserInformation.mail,
          givenName: UserInformation.givenName,
          surname: UserInformation.surname,
          userPrincipalName: UserInformation.userPrincipalName
        };

        await database.storeData(config.partitionKey, uuidv4(), data);

        client.trackEvent({ name: "Database_Storing", properties: { getMeetingDetailsUsingSubscription: UserInformation } });
              
        return "inserted into database";
  }
  catch(ex)
  {
    client.trackException({ exception: ex, properties: { SaveUserAndMeetingDetailsForSubscription: "SaveUserAndMeetingDetailsForSubscription" } });
     return "saving = " + ex.message
  }  
}

// Function to retrieve user information from the database for sending action items.
async function GetUserInformationAndSendActionItems(onlineMeetingId, AIPrompt, onlineMeetingDetail, subscriptionId)
{
  try
  {
    // Example usage:
    database.getData("","",subscriptionId)
    .then(async UserAndMeeting => {
        
        client.trackEvent({ name: "Database_sendActivity", properties: { users: UserAndMeeting } });
           
        // Handle UserAndMeeting data here
        console.log('Received User and Meeting:', UserAndMeeting);
        const c = await serverAPI.transcribeAndExtractUserActionItems(AIPrompt,UserAndMeeting, onlineMeetingDetail);
    })
  .catch(error => {
    client.trackException({ exception: error, properties: { getUserAndMeeting: "GetUserInformationAndSendActionItems" } });
    
      // Handle errors here
      console.error('Error in getUserAndMeeting:', error.message);
  });

  }
  catch(ex)
  {
    console.log(ex);
  }
}