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
const url = require('url');
const config = require('./config');

const clientId = process.env.BOT_ID;
const clientSecret = process.env.BOT_PASSWORD;
const baseUrl = config.BOT_ENDPOINT;
// const tenantId = config.Tenant;
const graphScopes = ['https://graph.microsoft.com/User.Read'];
let eventDetails = [];
let token = null;
let eventUpdated = false;
const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });
const auth = require('./auth');
const { connected } = require('process');
const jwt = require('jsonwebtoken');
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const { ConnectorClient, MicrosoftAppCredentials } = require('botframework-connector');

const credentials = new MicrosoftAppCredentials(config.botId,config.botPassword);
const { RecursiveCharacterTextSplitter } = require("langchain/text_splitter");
const fs = require('fs');
 const sampleTranscription = require('./sampleTranscription');

//Function to retrieve all meeting events from the calendar.
async function getTodaysMeetingAgenda(req, context) {
  
  const currentDate = new Date().toISOString().split('T')[0];
  const Graphurl =  `https://graph.microsoft.com/v1.0/users/` + context.activity.from.aadObjectId + `/calendar/events?$filter=start/dateTime ge '${currentDate}'&$orderby=start/dateTime`;
    
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
async function getMeetingTranscription(joinWebUrl, userId, tenantId) {
  try { 

      const onlineMeetingDetail = await getMeetingDetailsUsingSubscription(joinWebUrl, userId, tenantId);

      const accessToken = await auth.getAccessToken(tenantId); 
      
      // Make GET request to Microsoft Graph API
      const responseTranscriptionURL = await axios.get(`https://graph.microsoft.com/v1.0/users/${userId}/onlineMeetings/${onlineMeetingDetail.id}/transcripts`, {
        headers: {
          Authorization: `Bearer ${accessToken}`
        }
      });

      // get Latest URL 
      const response = await axios.get(`${responseTranscriptionURL.data.value[responseTranscriptionURL.data.value.length-1].transcriptContentUrl + '?$format=text/vtt'}`, {
        headers: {
          Authorization: `Bearer ${accessToken}`
        }
      });

    console.log(response.data)

      // Extract and return the meetings from the response data
      return response.data;
  } catch (error) {
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
    
    // Construct the URL with filter query
    const url = `https://graph.microsoft.com/v1.0/users/${userId}${joinWebUrl.replace('communications','')}`;

     // return url+ " ::: " +  accessToken;
    // Make GET request to Microsoft Graph API
    const response = await axios.get(url, {
      headers: {
        Authorization: `Bearer ${accessToken}`
      }
    });

    return response.data.value[0];

  } catch (error) {
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
                    value: {id: calendar.onlineMeeting.joinUrl}
                  }
            };
        })
    }
};
}

// Function to convert UTC time to local time.
function ConvertTimeToLocal(utcDateString)
{ 
  if(utcDateString.dateTime != undefined)
    {
      return utcDateString.dateTime.toLocaleString(undefined, {
        timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
        month: "2-digit",
        day: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit"
      });
    }
    else
    {
        // Create a new Date object from the UTC timestamp
        const utcDate = new Date(utcDateString);

        // Format options for desired output
        const options = {
            month: '2-digit',  // mm
            day: '2-digit',    // dd
            year: 'numeric',   // yyyy
            hour: '2-digit',   // hh
            minute: '2-digit', // MM
            hour12: true       // AM/PM format
        };

        // Convert UTC date to local date string with specified format
        const localDateString = utcDate.toLocaleString('en-US', options);
        
        return localDateString;
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
async function createSubscription(meetingJoinUrl, userId, conversationId, tenantId) {
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
            const subscriptionEndPoint = new Date(meetingDetails.endDateTime);
            subscriptionEndPoint.setHours(subscriptionEndPoint.getHours() + 1);
            let isoString = ChangeDateTimeFormat(subscriptionEndPoint);
            let subscriptionCreationInformation = {
                resource: resource,
                notificationUrl: notificationUrl,
                expirationDateTime: isoString,
                includeResourceData: true,
                changeType: "updated", 
                clientState: userId + "|" +  tenantId,
                encryptionCertificate: config.Base64EncodedCertificate,
                encryptionCertificateId: config.EncryptionCertificateId
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
  const client = new OpenAIClient(endpoint, new AzureKeyCredential(apiKey));
  let fileChunks = [];
  try {
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
          
          const response = await client.getChatCompletions(deployment_Id, messages, { maxTokens: 2000 });
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
      

        const response = await client.getChatCompletions(deployment_Id, messages_MeetingSummary, { maxTokens: 4095 });
          
        const MeetingSummary = `\n <b> Meeting Name: </b> ${meetingDetails.subject}
                                \n <b> Date:</b>  ${ConvertTimeToLocal(meetingDetails.startDateTime)}
                                \n <b> Meeting Summary:</b> <br /> ${FormatActionItems(response.choices[0].message.content.trim())} <br />
                                 \n <b> Action Items:<b> <br /> \n \n \n ${concatinateActionItemsForUsers}`;

        userinfo.forEach( user => {
          SendUserActivity(user.conversationId, MeetingSummary);
        });
       
        //meetingDetails
        // extractUserActionItems(MeetingSummary, responsesResults.join('\n'), userinfo);
  } catch (error) {
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