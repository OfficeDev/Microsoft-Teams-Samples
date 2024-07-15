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
    if(responsePayload.eventType === "Microsoft.Communication.CallEnded")
      {
        await serverAPI.getMeetingTranscription(req.body.value[0].resource, req.body.value[0].clientState.split('|')[0], req.body.value[0].clientState.split('|')[1])
        .then(async meetingDetails => {
           
            requestId = req.rawHeaders[3];
           
            if (alreadyProcessed(requestId)) {
              console.log(`Request ${requestId} already processed`);
               return res.status(200).end();
            }
            
            // Process the webhook payload
            console.log('Processing webhook payload:', req.body);
          
            // Call graph API        
            serverAPI.getMeetingDetailsUsingSubscription(req.body.value[0].resource, req.body.value[0].clientState.split('|')[0], req.body.value[0].clientState.split('|')[1])
            .then(async onlineMeetingDetail => {
                 // get User and meeting details
              const UserInfo =  GetUserInformationAndSendActionItems(req.body.value[0].resource,  meetingDetails, onlineMeetingDetail, req.body.value[0].subscriptionId);
            
            });
    
            // Mark this request ID as processed
            markAsProcessed(requestId);
            
            return res.status(200).end();
        });
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

            subscription = await serverAPI.createSubscription(context.activity.value.id, context.activity.from.aadObjectId , context.activity.conversation.id, context.activity.conversation.tenantId);
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

          const saving = await SaveUserAndMeetingDetailsForSubscription(context.activity.value.id, context.activity.from.aadObjectId, context.activity.conversation.id, subscription.split('|')[0], context.activity.conversation.tenantId);
            
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

        return "inserted into database";
  }
  catch(ex)
  {
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

        // Handle UserAndMeeting data here
        console.log('Received User and Meeting:', UserAndMeeting);
        const c = await serverAPI.transcribeAndExtractUserActionItems(AIPrompt,UserAndMeeting, onlineMeetingDetail);
    })
  .catch(error => {
      // Handle errors here
      console.error('Error in getUserAndMeeting:', error.message);
  });

  }
  catch(ex)
  {
    console.log(ex);
  }
}