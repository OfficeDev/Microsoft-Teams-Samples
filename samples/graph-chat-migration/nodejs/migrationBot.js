const { 
  ActivityHandler, 
  TeamsInfo,
  CardFactory,
  MessageFactory
} = require("botbuilder");
const axios = require("axios");
const auth = require("./auth/auth");
const Config = require("./config");

class MigrationBot extends ActivityHandler {
  constructor() {
    super();

    // See https://aka.ms/about-bot-activity-message to learn more about the message and other activity types.
    this.onMessage(async (context, next) => {
      await this.handleMessage(context);
      // By calling next() you ensure that the next BotHandler is run.
      await next();
    });

    this.onMembersAdded(async (context, next) => {
      await this.handleMembersAdded(context);
      await next();
    });
  }

  // Helper function to get access token for Microsoft Graph using existing auth module
  async getGraphAccessToken() {
    try {
      // Use the existing auth module to get access token
      // Using the tenant ID from environment variables
      const tenantId = process.env.MicrosoftAppTenantId;
      const accessToken = await auth.getAccessToken(tenantId);
      return accessToken;
    } catch (error) {
      console.error('Error acquiring token:', error);
      throw error;
    }
  }

  // Helper function to detect conversation type and get appropriate IDs
  getConversationInfo(context) {
    const conversationId = context.activity.conversation?.id;
    const channelData = context.activity.channelData;

    // Check if it's a Teams channel (contains @thread.tacv2)
    if (conversationId && conversationId.includes('@thread.tacv2')) {
      return {
        type: 'channel',
        teamId: channelData?.team?.id,
        channelId: conversationId,
        conversationId: conversationId
      };
    }
    // Check if it's a group chat (starts with 19: but no @thread.tacv2)
    else if (conversationId && conversationId.startsWith('19:')) {
      return {
        type: 'groupchat',
        chatId: conversationId,
        conversationId: conversationId
      };
    }
    // Default fallback
    else {
      return {
        type: 'unknown',
        conversationId: conversationId
      };
    }
  }

  // Helper function to call Microsoft Graph API
  async callGraphAPI(endpoint, method = 'POST', data = null) {
    try {
      const accessToken = await this.getGraphAccessToken();
      const config = {
        method,
        url: endpoint,
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json',
        },
      };

      if (data) {
        config.data = data;
      }

      const response = await axios(config);
      return response;
    } catch (error) {
      console.error('Graph API call failed:', error.response?.data || error.message);
      throw error;
    }
  }

  // Helper function to handle adaptive card form submission
  async handlePostMessageSubmission(context, data) {
    try {
      console.log('Handling post message submission with data:', JSON.stringify(data, null, 2));
      
      const { messageDate, messageTime, messageContent, action } = data;
      
      if (action === 'cancel') {
        await context.sendActivity('Post message operation cancelled.');
        return;
      }
      
      if (action === 'submitPostMessage') {
        if (!messageDate || !messageTime || !messageContent) {
          await context.sendActivity('Please fill in all required fields.');
          return;
        }

        // Combine date and time for Graph API
        const combinedDateTime = `${messageDate}T${messageTime}:00`;
        const formattedTimestamp = new Date(combinedDateTime).toISOString();
        
        // Detect conversation type and get appropriate IDs
        const conversationInfo = this.getConversationInfo(context);
        
        if (conversationInfo.type === 'unknown' || !conversationInfo.conversationId) {
          await context.sendActivity('Unable to retrieve conversation information from the context.');
          return;
        }
        
        // Prepare message data for Graph API
        const chatMessage = {
          createdDateTime: formattedTimestamp,
          from: {
            user: {
              id: Config.UserId, 
              displayName: "Adele Vance",
              userIdentityType: 'aadUser'
            }
          },
          body: {
            contentType: 'html',
            content: messageContent
          }
        };

        let graphEndpoint;


        if (conversationInfo.type === 'groupchat') {
          // Use chats API for group chats
          graphEndpoint = `https://graph.microsoft.com/beta/chats/${conversationInfo.chatId}/messages`;
        } else if (conversationInfo.type === 'channel') {
          // Use teams/channels API for Teams channels
          if (!conversationInfo.teamId) {
            await context.sendActivity('Unable to retrieve team information for this channel.');
            return;
          }
          const teamsDetails = await TeamsInfo.getTeamDetails(context);
          graphEndpoint = `https://graph.microsoft.com/beta/teams/${teamsDetails.aadGroupId}/channels/${conversationInfo.teamId}/messages`;
        }

        // Call Microsoft Graph API using HTTP
        const result = await this.callGraphAPI(graphEndpoint, 'POST', chatMessage);

        if (result && result.data.id) {
          await context.sendActivity(`Message posted successfully to ${conversationInfo.type}! Message ID: ${result.data.id}`);
        } else {
          await context.sendActivity('Failed to post message. Please try again.');
        }
      }
    } catch (error) {
      console.error('Handle post message submission failed:', error);
      await context.sendActivity(`Failed to process request: ${error.message}`);
    }
  }

  async handleMessage(context) {
    let userMessage = context.activity.text?.trim() || '';

    // Check if this is an adaptive card submission
    if (context.activity.value && context.activity.value.action) {
      console.log('Adaptive card submission received in message handler');
      await this.handlePostMessageSubmission(context, context.activity.value);
      return;
    }

    // Remove @mention from the beginning of the message
    // Pattern: <at>BotName</at> command -> command
    userMessage = userMessage.replace(/<at>.*?<\/at>\s*/i, '').trim().toLowerCase();

    switch (userMessage) {
      case 'startmigration':
        await this.handleStartMigration(context);
        break;

      case 'postmessage':
        await this.handlePostMessage(context);
        break;

      case 'completemigration':
        await this.handleCompleteMigration(context);
        break;

      case 'help':
        await this.handleHelp(context);
        break;

      default:
        await context.sendActivity(
          `Hi! I'm the Migration Bot. Type "startMigration" to begin the migration process, or "help" to see available commands.`
        );
        break;
    }
  }

  async handleStartMigration(context) {
    try {
      // Detect conversation type and get appropriate IDs
      const conversationInfo = this.getConversationInfo(context);

      if (conversationInfo.type === 'unknown' || !conversationInfo.conversationId) {
        await context.sendActivity(`Unable to retrieve conversation information from the context.`);
        return;
      }

      let graphEndpoint;

      if (conversationInfo.type === 'groupchat') {
        // Use chats API for group chats
        graphEndpoint = `https://graph.microsoft.com/beta/chats/${conversationInfo.chatId}/startMigration`;
      } else if (conversationInfo.type === 'channel') {
        // Use teams/channels API for Teams channels
        if (!conversationInfo.teamId) {
          await context.sendActivity(`Unable to retrieve team information for this channel.`);
          return;
        }
        const teamsDetails = await TeamsInfo.getTeamDetails(context);
        graphEndpoint = `https://graph.microsoft.com/beta/teams/${teamsDetails.aadGroupId}/channels/${conversationInfo.teamId}/startMigration`;
      }
      const result = await this.callGraphAPI(graphEndpoint);

      if (result.status == 204) {
        await context.sendActivity(`Migration started successfully for ${conversationInfo.type}!`);
      } else {
        await context.sendActivity(`Failed to start migration: ${result.error?.message || 'Unknown error'}`);
      }
    } catch (error) {
      console.error('StartMigration command failed:', error);
      await context.sendActivity(`Failed to start migration: ${error.message}`);
    }
  }

  async handlePostMessage(context) {
    try {
      // Create adaptive card for timestamp and message input
      const adaptiveCard = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.4",
        "speak": "Please provide a timestamp and message to post.",
        "body": [
          {
            "type": "TextBlock",
            "text": "Post Message with Timestamp",
            "weight": "Bolder",
            "size": "Medium",
            "color": "Accent"
          },
          {
            "type": "TextBlock",
            "text": "Please provide the timestamp and message details:",
            "wrap": true,
            "spacing": "Medium"
          },
          {
            "type": "Input.Date",
            "id": "messageDate",
            "label": "Message Date",
            "isRequired": true,
            "errorMessage": "Please select a date."
          },
          {
            "type": "Input.Time",
            "id": "messageTime",
            "label": "Message Time",
            "isRequired": true,
            "errorMessage": "Please select a time."
          },
          {
            "type": "Input.Text",
            "id": "messageContent",
            "label": "Message Content",
            "placeholder": "Enter the message content...",
            "isMultiline": true,
            "maxLength": 1000,
            "isRequired": true,
            "errorMessage": "Please enter a message."
          }
        ],
        "actions": [
          {
            "type": "Action.Submit",
            "title": "Post Message",
            "data": {
              "action": "submitPostMessage"
            }
          },
          {
            "type": "Action.Submit",
            "title": "Cancel",
            "data": {
              "action": "cancel"
            }
          }
        ]
      };

      const cardAttachment = CardFactory.adaptiveCard(adaptiveCard);
      const message = MessageFactory.attachment(cardAttachment);
      message.text = 'Please fill out the form to post a message with a specific timestamp.';

      await context.sendActivity(message);

    } catch (error) {
      console.error('PostMessage command failed:', error);
      await context.sendActivity(`Failed to show post message form: ${error.message}`);
    }
  }

  async handleCompleteMigration(context) {
    try {
      // Detect conversation type and get appropriate IDs
      const conversationInfo = this.getConversationInfo(context);

      if (conversationInfo.type === 'unknown' || !conversationInfo.conversationId) {
        await context.sendActivity(`Unable to retrieve conversation information from the context.`);
        return;
      }

      let graphEndpoint;


      if (conversationInfo.type === 'groupchat') {
        // Use chats API for group chats
        graphEndpoint = `https://graph.microsoft.com/beta/chats/${conversationInfo.chatId}/completeMigration`;
      } else if (conversationInfo.type === 'channel') {
        // Use teams/channels API for Teams channels
        if (!conversationInfo.teamId) {
          await context.sendActivity(`Unable to retrieve team information for this channel.`);
          return;
        }
        const teamsDetails = await TeamsInfo.getTeamDetails(context);
        graphEndpoint = `https://graph.microsoft.com/beta/teams/${teamsDetails.aadGroupId}/channels/${conversationInfo.teamId}/completeMigration`;
      }

      const result = await this.callGraphAPI(graphEndpoint);

      if (result.status == 204) {
        await context.sendActivity(`Migration completed successfully for ${conversationInfo.type}!`);
      } else {
        await context.sendActivity(`Failed to complete migration: ${result.error?.message || 'Unknown error'}`);
      }
    } catch (error) {
      console.error('CompleteMigration command failed:', error);
      await context.sendActivity(`Failed to complete migration: ${error.message}`);
    }
  }

  async handleHelp(context) {
    await context.sendActivity(
      `Migration Bot Commands:

• startMigration - Initiates the chat migration process
• postMessage - Shows a form to post a message with a specific timestamp
• completeMigration - Completes the migration process
• help - Shows this help message

Type any of these commands to get started!`
    );
  }

  async handleMembersAdded(context) {
    const welcomeText = `Welcome to the Migration Bot! 

I can help you start the chat migration process. Type "startMigration" to begin, or "help" to see all available commands.`;

    for (const member of context.activity.membersAdded) {
      if (member.id !== context.activity.recipient.id) {
        await context.sendActivity(welcomeText);

        // Get conversation information when bot is added
        try {
          const conversationInfo = this.getConversationInfo(context);

          if (conversationInfo.type !== 'unknown' && conversationInfo.conversationId) {
            let graphEndpoint;

            if (conversationInfo.type === 'groupchat') {
              // Use chats API for group chats
              graphEndpoint = `https://graph.microsoft.com/beta/chats/${conversationInfo.chatId}`;
            } else if (conversationInfo.type === 'channel') {
              const teamsDetails = await TeamsInfo.getTeamDetails(context);
              graphEndpoint = `https://graph.microsoft.com/beta/teams/${teamsDetails.aadGroupId}/channels/${conversationInfo.teamId}`;
              return;
            }

            // Call Microsoft Graph API to get conversation details (for group chats)
            if (graphEndpoint) {
              const conversationDetails = await this.callGraphAPI(graphEndpoint, 'GET');

              if (conversationDetails && conversationDetails.data && conversationDetails.data.createdDateTime) {
                const createdDate = new Date(conversationDetails.data.createdDateTime);
                await context.sendActivity(`This ${conversationInfo.type} was created on ${createdDate.toLocaleString()}.`);
              } else {
                await context.sendActivity('Creation timestamp is not available, but this information will be useful for the migration process.');
              }
            }
          } else {
            await context.sendActivity('Unable to retrieve conversation information at this time.');
          }
        } catch (error) {
          console.error('Failed to get conversation information:', error);
          await context.sendActivity('Unable to retrieve conversation information at this time.');
        }
      }
    }
  }
}

module.exports.MigrationBot = MigrationBot;
