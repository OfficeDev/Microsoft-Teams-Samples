const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");

// Create storage for conversation history
const storage = new LocalStorage();

const createTokenFactory = () => {
  return async (scope, tenantId) => {
    const managedIdentityCredential = new ManagedIdentityCredential({
      clientId: process.env.CLIENT_ID,
    });
    const scopes = Array.isArray(scope) ? scope : [scope];
    const tokenResponse = await managedIdentityCredential.getToken(scopes, {
      tenantId: tenantId,
    });

    return tokenResponse.token;
  };
};

// Configure authentication using TokenCredentials
const tokenCredentials = {
  clientId: process.env.CLIENT_ID || "",
  token: createTokenFactory(),
};

const credentialOptions =
  config.MicrosoftAppType === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

// Create the app with storage
const app = new App({
  ...credentialOptions,
  storage,
});

/**
 * Lists all channels in the current team.
 * @param {*} context - The bot context.
 */
async function listTeamChannels(context) {
  try {
    const activity = context.activity;
    const teamId = activity.channelData?.team?.id;

    if (!teamId) {
      await context.reply("This command can only be used in a team channel.");
      return;
    }

    const response = await context.api.teams.getConversations(teamId);
    
    // getConversations returns an array of ChannelInfo objects directly
    const channels = Array.isArray(response) ? response : (response?.conversations || []);

    if (!channels || channels.length === 0) {
      await context.reply("No channels found in this team.");
      return;
    }

    // Display channels in a formatted list
    let message = '```\n';
    message += 'LIST OF CHANNELS\n';
    message += '═══════════════════════════════════════\n\n';
    channels.forEach((ch, i) => {
      message += `${i + 1}. ${ch.name || 'General'}\n`;
    });
    message += '```';
    await context.reply(message);
  } catch (error) {
    console.error('Error listing channels:', error);
    await context.reply("An error occurred while trying to list the channels. Please check the team ID and your network connection. Error details: " + error.message);
  }
}

/**
 * Starts a new thread in the current Teams channel.
 * @param {*} context - The bot context.
 */
async function startNewThreadInChannel(context) {
  try {
    const activity = context.activity;
    const teamsChannelId = activity.channelData?.channel?.id;

    if (!teamsChannelId) {
      await context.reply("This command can only be used in a team channel.");
      return;
    }

    // Create a conversation reference for the channel
    const channelConversationRef = {
      ...context.ref,
      conversation: {
        id: teamsChannelId,
        isGroup: true,
        conversationType: 'channel',
        tenantId: activity.channelData?.tenant?.id
      }
    };

    // Send initial message to the channel to start a new thread
    const response = await context.send("This will start a new thread in the channel.", channelConversationRef);

    // Send a follow-up message in the same thread
    if (response && response.id) {
      const threadConversationRef = {
        ...channelConversationRef,
        conversation: {
          ...channelConversationRef.conversation,
          id: `${teamsChannelId};messageid=${response.id}`
        }
      };
      await context.send("This will be the first response to the new thread.", threadConversationRef);
    }
  } catch (error) {
    console.error('Error starting new thread:', error);
    await context.reply("Error starting thread: " + error.message);
  }
}

/**
 * Retrieves the details of the user who sent the message.
 * @param {*} context - The bot context.
 */
async function getTeamMember(context) {
  try {
    const activity = context.activity;
    const aadObjectId = activity.from?.aadObjectId;
    const teamId = activity.channelData?.team?.id;

    if (!teamId) {
      await context.reply("This command can only be used in a team channel.");
      return;
    }

    if (!aadObjectId) {
      await context.reply("Unable to retrieve user information.");
      return;
    }

    const teamMember = await context.api.conversations.members(activity.conversation.id).getById(aadObjectId);

    if (!teamMember) {
      await context.reply("Team member not found.");
      return;
    }

    // Display user information in a formatted way
    const userInfo = '```\n' +
      'USER INFORMATION\n' +
      '═══════════════════════════════════════\n\n' +
      `Name:                  ${teamMember.name || 'N/A'}\n` +
      `Email:                 ${teamMember.email || 'N/A'}\n` +
      `Given Name:            ${teamMember.givenName || 'N/A'}\n` +
      `Surname:               ${teamMember.surname || 'N/A'}\n` +
      `Role:                  ${teamMember.role || 'N/A'}\n` +
      `User Principal Name:   ${teamMember.userPrincipalName || 'N/A'}\n` +
      '```';

    await context.reply(userInfo);
  } catch (error) {
    console.error('Error getting team member:', error);
    await context.reply("Error retrieving team member: " + error.message);
  }
}

/**
 * Retrieves all team members in a paginated manner.
 * @param {*} context - The bot context.
 */
async function getPagedTeamMembers(context) {
  try {
    const activity = context.activity;
    const teamId = activity.channelData?.team?.id;

    if (!teamId) {
      await context.reply("This command can only be used in a team channel.");
      return;
    }

    // Get all members in the conversation (no pagination needed for simple get)
    const members = await context.api.conversations.members(activity.conversation.id).get();

    if (members.length === 0) {
      await context.reply("No team members found.");
      return;
    }

    // Display team members in a formatted list
    let membersList = '```\n';
    membersList += `TEAM MEMBERS (${members.length} total)\n`;
    membersList += '═══════════════════════════════════════\n\n';
    
    members.forEach((member, index) => {
      membersList += `${index + 1}. ${(member.name || 'Unknown').toUpperCase()}\n`;
      membersList += `   Email:      ${member.email || 'N/A'}\n`;
      membersList += `   Given Name: ${member.givenName || 'N/A'}\n`;
      membersList += `   Surname:    ${member.surname || 'N/A'}\n`;
      membersList += `   Role:       ${member.role || 'N/A'}\n`;
      membersList += `   UPN:        ${member.userPrincipalName || 'N/A'}\n`;
      if (index < members.length - 1) {
        membersList += '\n───────────────────────────────────────\n\n';
      }
    });
    membersList += '```';

    await context.reply(membersList);
  } catch (error) {
    console.error('Error retrieving team members:', error);
    await context.reply("Error retrieving team members: " + error.message);
  }
}

app.on("message", async (context) => {
  const activity = context.activity;
  const text = (activity.text || '').replace(/<at>.*?<\/at>/g, '').trim().toLowerCase();

  if (text.includes('listchannels')) {
    await listTeamChannels(context);
  } else if (text.includes('threadchannel')) {
    await startNewThreadInChannel(context);
  } else if (text.includes('getteammember')) {
    await getTeamMember(context);
  } else if (text.includes('getpagedteammembers')) {
    await getPagedTeamMembers(context);
  } else {
    await context.reply("I didn't understand that command. Please try again.");
  }
});

module.exports = app;
