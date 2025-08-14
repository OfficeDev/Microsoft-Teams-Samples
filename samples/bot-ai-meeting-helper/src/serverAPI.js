// <copyright file="serverAPI.js" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

const express = require('express'); 
const msal = require('@azure/msal-node');
const app = express();
const path = require('path');
const axios = require('axios');
const { DateTime } = require('luxon');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const url = require('url');
const config = require('./config');
  
const baseUrl = config.BOT_ENDPOINT; 
let eventDetails = []; 
const server = require('http').createServer(app); 
const auth = require('./auth'); 
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const { ConnectorClient, MicrosoftAppCredentials } = require('botframework-connector');

const credentials = new MicrosoftAppCredentials(config.botId,config.botPassword);
const { RecursiveCharacterTextSplitter } = require("langchain/text_splitter"); 
const sampleTranscription = require('./sampleTranscription');
const { date } = require('azure-storage');
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

//Function to retrieve all meeting events from the calendar.
async function getTodaysMeetingAgenda(req, context) {
  
  const currentDate = new Date();
  const startDate = currentDate.toISOString();

  const endDate = new Date();
  endDate.setDate(endDate.getDate() + 10);
 
  // const Graphurl =  `https://graph.microsoft.com/v1.0/users/` + context.activity.from.aadObjectId + `/calendar/events?$filter=start/dateTime ge '${currentDate}'&$orderby=start/dateTime&$expand=instances`;
  const Graphurl =  `https://graph.microsoft.com/v1.0/users/` + context.activity.from.aadObjectId + `/calendarView?startDateTime=${startDate}&endDateTime=${endDate.toISOString()}`;
    
  const accessToken = await auth.getAccessToken(context.activity.conversation.tenantId);
  try {
    console.log(accessToken);     
    const response = await axios.get(Graphurl, {
      headers: {
         Authorization: `Bearer ${accessToken}`
      }
    });  
      return BindCard(response.data.value); 
  } catch (error) {
    // throw new Error('Failed to get calendar events: ' + error.message + " accessToken = " + accessToken + " url = " + Graphurl + " Bot_ID = " + process.env.BOT_ID + " Password = "+ process.env.BOT_PASSWORD);
    throw new Error('Failed to get calendar events: ' + error.message);
  }
}

// Function to fetch the meeting transcription text for a specific meeting using the join URL.
async function getMeetingTranscription(joinWebUrl, userId, tenantId, meetingId) {
  try { 

     // const onlineMeetingDetail = await getMeetingDetailsUsingSubscription(joinWebUrl, userId, tenantId);

      const accessToken = await auth.getAccessToken(tenantId); 
      
      client.trackEvent({ name: "response getMeetingTranscription", properties: { onlineMeetingDetail: meetingId } });
            
      // Make GET request to Microsoft Graph API
      const responseTranscriptionURL = await axios.get(`https://graph.microsoft.com/v1.0/users/${userId}/onlineMeetings/${meetingId}/transcripts`, {
        headers: {
          Authorization: `Bearer ${accessToken}`
        }
      });

      client.trackEvent({ name: "responseTranscriptionURL", properties: { responseTranscriptionURL:"line # 100", onlineMeetingDetail: responseTranscriptionURL.data.value } });
            
      // get Latest URL 
      const response = await axios.get(`${responseTranscriptionURL.data.value[responseTranscriptionURL.data.value.length-1].transcriptContentUrl + '?$format=text/vtt'}`, {
        headers: {
          Authorization: `Bearer ${accessToken}`
        }
      });

    console.log(response.data)
    client.trackEvent({ name: "response By Transcription URL", properties: { responseTranscriptionURL:"line # 110",onlineMeetingDetail: response.data } });
          
      // Extract and return the meetings from the response data
      return response.data;
  } catch (error) {
      client.trackException({ exception: error, properties: { getMeetingTranscription: "Error fetching meetings" } });

      console.error("Error fetching meetings:", error);
      throw error;
  }
}

// Function to fetch online meeting details filtered by the join web URL.
async function getMeetingDetailsUsingSubscription(joinWebUrl, userId, tenantId) {
  try {
    // Encode the join web URL
    const encodedJoinWebUrl = decodeURIComponent(joinWebUrl);
    const accessToken = await auth.getAccessToken(tenantId); 
    
    client.trackEvent({ name: "call getMeetingDetailsUsingSubscription", properties: { tenantId: tenantId } });
            
    
    // Construct the URL with filter query
    const url = `https://graph.microsoft.com/v1.0/users/${userId}${joinWebUrl.replace('communications','')}`;

     // return url+ " ::: " +  accessToken;
    // Make GET request to Microsoft Graph API
    const response = await axios.get(url, {
      headers: {
        Authorization: `Bearer ${accessToken}`
      }
    });

    client.trackEvent({ name: "response getMeetingDetailsUsingSubscription", properties: { response: response } });
            
    return response.data.value[0];

  } catch (error) {
    client.trackException({ exception: error, properties: { getOnlineMeeting: "Error fetching meetings" } });

    console.error("Error fetching meetings:", error);
    throw error;
  }
}

function extractMeetingId(joinUrl) {
  const parsedUrl = new URL(joinUrl);
  const pathnameParts = parsedUrl.pathname.split('/');
  const meetingId = pathnameParts[pathnameParts.length - 1];
  return meetingId;
}

// Function to generate event cards and send them to the user. 
function BindCard(calendarEvents)
{
  try
  {
    if(calendarEvents.length)
      {
          return {
            contentType: "application/vnd.microsoft.teams.card.list",
            content: {
                title: "Meeting Events",
                items: calendarEvents.map(calendar => {
                  return {
                          type: "resultItem",
                          title: calendar.subject,
                          subtitle: ConvertTimeToLocal(calendar.start),
                          tap: {
                            type: "invoke",
                            value: {meetingId:calendar.id, joinUrl: calendar.onlineMeeting.joinUrl, endTime: calendar.end.dateTime}
                          }
                    };
                })
            }
        };
      }
      else
      {
         return {
            contentType: "application/vnd.microsoft.teams.card.list",
            content: {
                title: "Meeting events not found.",
                type: "section"
            }
        };
      }
  }
 catch(e)
 {
  console.log(e);
 }
}

// Function to convert UTC time to local time.
function ConvertTimeToLocal(utcDateString)
{   
  let UTCDate = new Date();
  try
  {
    if(utcDateString.dateTime != undefined)
      {
        UTCDate = new Date(utcDateString.dateTime+ 'Z');
      }
      else
      {
        UTCDate = new Date();
      }
    
      let istTime = UTCDate.toLocaleString('en-US', {
        timeZone: config.LocalTimeZone
      }); 
      return istTime;     
  }
  catch(e)
  { 
    client.trackException({ exception: e, properties: { ConvertTimeToLocal: "ConvertTimeToLocal" } });

    return new Date();
  
  }
  
}

// Function to change the date and time format.
function ChangeDateTimeFormat(date)
{
  // Parse the original date string into components
  const year = date.getFullYear();
  const month = ('0' + (date.getMonth() + 1)).slice(-2); // Months are zero indexed, so we add 1
  const day = ('0' + date.getDate()).slice(-2);
  const hours = ('0' + date.getHours()).slice(-2);
  const minutes = ('0' + date.getMinutes()).slice(-2);
  const seconds = ('0' + date.getSeconds()).slice(-2);
  const milliseconds = ('00' + date.getMilliseconds()).slice(-3);

  // Construct the desired format
  const formattedDateString = `${year}-${month}-${day}T${hours}:${minutes}:${seconds}.${milliseconds}Z`;
  return formattedDateString;
}

// Function to create a subscription based on the meeting join URL.
async function createSubscription(meetingJoinUrl, userId, conversationId, tenantId, endTime, meetingId) {
  let existingSubscriptions = null;
  let applicationToken = "";
  let resource = "";
  let  notificationUrl = baseUrl + "/EventHandler";
  try {
  applicationToken = await auth.getAccessToken(tenantId);
  resource = `/communications/onlineMeetings/?$filter=JoinWebUrl eq '${meetingJoinUrl}'`;

      var apiResponse = await axios.get(config.SubscriptionURL, {
          headers: {
              "accept": "application/json",
              "contentType": 'application/json',
              "authorization": "bearer " + applicationToken
          }
      });

      existingSubscriptions = apiResponse.data.value;
  }
  catch (ex) {
      return "Existing Error"+ ex.message;
  }

  // Is existing subscriptions are found, delete the old subscriptions.
  var existingSubscription = existingSubscriptions.find(subscription => subscription.resource === resource);

  if (existingSubscription != null) {
      if(existingSubscription.notificationUrl != notificationUrl)
      {
        console.log(`CreateNewSubscription-ExistingSubscriptionFound: ${resource}`);

        await deleteSubscription(existingSubscription.id,applicationToken);
  
        existingSubscription = null;
      }
      else
      {
        return existingSubscription.id + "|Subscription created.";
      }
  } 

  // Once old subscriptions are deleted, creating new subscription.
  try {
      if (existingSubscription == null) {
        await getMeetingDetailsUsingSubscription(resource, userId, tenantId)
        .then(async meetingDetails => {
              const subscriptionEndPoint = new Date(endTime);
              subscriptionEndPoint.setHours(subscriptionEndPoint.getHours() + 1);
              let isoString = ChangeDateTimeFormat(subscriptionEndPoint);
              let subscriptionCreationInformation = {
                  resource: resource,
                  notificationUrl: notificationUrl ,
                  expirationDateTime: isoString,
                  includeResourceData: true,
                  changeType: "updated", 
                  clientState: userId + "|" +  tenantId,
                  encryptionCertificate: config.Base64EncodedCertificate,
                  encryptionCertificateId: config.EncryptionCertificateId,
                  encryptionCertificateId:meetingDetails.id
              };

              var response = await axios.post(config.SubscriptionURL, subscriptionCreationInformation, {
                  headers: {
                      "accept": "application/json",
                      "contentType": 'application/json',
                      "authorization": "bearer " + applicationToken
                  }
              });

              existingSubscription = response.data;
              return existingSubscription.id + "|Subscription created.";         
        }); 
         return existingSubscription.id + "|Subscription created.";
      }
  }
  catch (e) {
      console.log("Error--" + e);
      return "Something went wrogn! please contact to administrator";
  }
 
}

// Function to retrieve meeting transcription, use Azure OpenAI to summarize the meeting, and extract all action items for all attendees.
async function transcribeAndExtractUserActionItems(meetingNotes, userinfo, meetingDetails) {
  const apiKey = config.azureOpenAIKey;
  const endpoint =  config.azureOpenAIEndpoint;
  const deployment_Id = config.azureOpenAIDeploymentName; 
  const ai_client = new OpenAIClient(endpoint, new AzureKeyCredential(apiKey));
  let fileChunks = [];
  try {
    
       client.trackEvent({ name: "subscription", properties: { transcribeAndExtractUserActionItems: meetingNotes } });
              

        const splitter = new RecursiveCharacterTextSplitter({
            chunkSize: 3000,
            chunkOverlap: 2,
        });
      
      // // Hardcoded meeting notes for testing purpose.
      //  const lenth  = sampleTranscription.length;
      //  meetingNotes = sampleTranscription;

        fileChunks = await splitter.createDocuments([meetingNotes]);
        let responsesResults = [];
       
        let generatedText = "";

        for (const chunk of fileChunks) {
          const chunkContent = chunk.pageContent;
          const messages = [
              { role: "system", content: config.SystemPrompt },
              { role: "user", content: chunkContent }
          ];
          
          const response = await ai_client.getChatCompletions(deployment_Id, messages, { maxTokens: 2000 });
          generatedText = response.choices[0].message.content.trim(); 
          try
          { 
            const data = JSON.parse(generatedText, (key, value) => {
              // Example: Replace escaped double quotes with actual double quotes
              if (typeof value === 'string') {
                  return value.replace(/\\\"/g, '"');
              }
              return value;
            });

            responsesResults.push(data);
          }
          catch(ex)
          {
            console.log(ex);
          }
        } 

        let responseResultArray = [];

        responsesResults.forEach(element => {
            element.forEach(childElement => {
              responseResultArray.push(childElement);        
            })  
        });

        //const combinedMeetings = combineMeetingData(responseResultArray);
        let concatinateActionItemsForUsers = "";
        let meetingSummary = "";
        combineMeetingData(responseResultArray).forEach(childElement => {
         const actionItems= FormatActionItems( childElement['Combined Action Items'], true);

          if(actionItems != "")
          {
            concatinateActionItemsForUsers = concatinateActionItemsForUsers + `\n <b>  ${childElement.user} </b> \n`;
            concatinateActionItemsForUsers = concatinateActionItemsForUsers + `<ul> ${FormatActionItems( childElement['Combined Action Items'], true)}</ul>`;
          } 

          meetingSummary = meetingSummary + FormatActionItems(childElement['Combined Meeting summaries'], false);
        });

        const messages_MeetingSummary = [
          { role: "system", content: "Consolidate give meeting summary in bullet points" },
          { role: "user", content: meetingSummary }
        ];
      

        const response = await ai_client.getChatCompletions(deployment_Id, messages_MeetingSummary, { maxTokens: 4095 });
          
        const MeetingSummary = `\n <b> Meeting Name: </b> ${meetingDetails.subject}
                                \n <b> Date:</b>  ${ConvertTimeToLocal(new Date())}
                                \n <b> Meeting Summary:</b> <br /> ${FormatActionItems(response.choices[0].message.content.trim())} <br />
                                 \n <b> Action Items:<b> <br /> \n \n \n ${concatinateActionItemsForUsers}`;

        userinfo.forEach( user => {
          SendUserActivity(user.conversationId, MeetingSummary);
        });
       
        //meetingDetails
        // extractUserActionItems(MeetingSummary, responsesResults.join('\n'), userinfo);
  } catch (error) {
    // userinfo.forEach( user => {
    //   SendUserActivity(user.conversationId, error);
    // });
      client.trackException({ exception: error, properties: { transcribeAndExtractUserActionItems: "transcribeAndExtractUserActionItems" } });

      console.error('Error fetching data from Azure OpenAI API:', error.message);
  }
}

function escapeSpecialChars(str) {
  return str.replace(/\\/g, '\\\\')  // escape backslashes
            .replace(/'/g, "\\'");   // escape single quotes
}

// Function to combine action items and meeting summaries for each user.
function combineMeetingData(data) {
  const combinedData = {};
  
  data.forEach(item => {
    const { user, "Meeting summary": meetingSummary, "Action Items": actionItems } = item;
    
    if (!combinedData[user]) {
      combinedData[user] = {
        user: user,
        "Meeting summaries": [],
        "Action Items": []
      };
    }
    
    combinedData[user]["Meeting summaries"].push(meetingSummary);
    combinedData[user]["Action Items"].push(actionItems);
  });
  
  // Format output
  const formattedOutput = Object.values(combinedData).map(userObj => ({
    user: userObj.user,
    "Combined Meeting summaries": userObj["Meeting summaries"].join('\n'),
    "Combined Action Items": userObj["Action Items"].join('\n')
  }));
  
  return formattedOutput;
}

// Function to send action items to individual users.
async function SendUserActivity(conversationId, AIPrompt)
{
    try{
          const client = new ConnectorClient(credentials, { baseUri: "https://smba.trafficmanager.net/amer/" });
        // Conversation Id will we save into database.
          await client.conversations.sendToConversation(
            conversationId,
            {
              type: 'message',
              from: { id: process.env.BOT_ID },
              text: AIPrompt
            });
    }
    catch(ex)
    {
      console.log(ex);
    }
}

// Function to extract user-specific action items from generated text and send consolidated action items to all users.
function FormatActionItems(generatedText, withlist) {
  let actionItems = "";

  // Split the generated text into sentences for processing
  const sentences = generatedText.split(/\n|\.\s/);

  // Loop through sentences to find action items assigned to users
  sentences.forEach(sentence => {
    if(withlist)
    {
      if(sentence.trim().indexOf("No action items")!=0 && sentence.trim().indexOf("None")!=0 
    && sentence.trim().indexOf("N/A")!=0 && sentence.trim()!="")
      {
        actionItems = actionItems + "<li>" + sentence + "</li>";
      }  
    }
    else
    {
      actionItems = actionItems + "\n" + sentence;
    }
    
  });  

  return actionItems;
}

// Function to extract user-wise action items from generated text
// and send consolidated action items to all users.
async function extractUserActionItems(MeetingSummary, generatedText, users) {
  let userActionItems = [];

  // Split the generated text into sentences for processing
  const sentences = generatedText.split(/\n|\.\s/);

  // Loop through sentences to find action items assigned to users
  sentences.forEach(sentence => {
      users.forEach( user => {
          const regex = new RegExp(`\\b${user.givenName}|${user.displayName}\\b`, 'gi');
          const matches = regex.exec(sentence);
          if (matches && matches[0]) {
            if(matches.input != user.givenName + ":")
              {
                SendUserActivity(user.conversationId, MeetingSummary + " \n" +  generatedText);
              }         
          }
      });
  });  
}

// Function to retrieve user information using the user ID with the Graph API.
async function getUserInformation(userId, tenantId) {
  try {
    const accessToken = await auth.getAccessToken(tenantId); 
    
    // Construct the URL with filter query
    const url = `https://graph.microsoft.com/v1.0/users/${userId}`;

    // Make GET request to Microsoft Graph API
    const response = await axios.get(url, {
      headers: {
        Authorization: `Bearer ${accessToken}`
      }
    });

    console.log(response.data);
    return response.data;
 
  } catch (error) {
    console.error("Error fetching meetings:", error);
    throw error;
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

  // Export the function
module.exports = {
  getTodaysMeetingAgenda: getTodaysMeetingAgenda,
  createSubscription: createSubscription,
  getMeetingDetailsUsingSubscription: getMeetingDetailsUsingSubscription,
  getUserInformation: getUserInformation,
  getMeetingTranscription: getMeetingTranscription,
  transcribeAndExtractUserActionItems:transcribeAndExtractUserActionItems
};