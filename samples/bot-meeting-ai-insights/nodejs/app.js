// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { App } = require('@microsoft/teams.apps');
const config = require('./config');
const axios = require('axios');

// Create Teams AI v2 App instance
const app = new App({
    clientId: config.MicrosoftAppId,
    clientSecret: config.MicrosoftAppPassword,
    tenantId: config.MicrosoftAppTenantId,
    oauth: {
        defaultConnectionName: config.connectionName
    }
});

// Error handling
app.event('error', async (client) => {
    if (client.activity) {
        await app.send(
            client.activity.conversation.id,
            'An error occurred while processing your message.'
        );
    }
});

// Helper function to get meeting ID from join URL
async function getMeetingId(accessToken, meetingUrl) {
    try {
        const url = `https://graph.microsoft.com/v1.0/me/onlineMeetings?$filter=JoinWebUrl eq '${meetingUrl}'`;
        const response = await axios.get(url, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        });
        return response.data.value[0]?.id || null;
    } catch (error) {
        return null;
    }
}

// Helper function to get AI Insight ID
async function getAiInsightId(accessToken, userId, onlineMeetingId) {
    try {
        const url = `https://graph.microsoft.com/beta/copilot/users/${userId}/onlineMeetings/${onlineMeetingId}/aiInsights`;
        const response = await axios.get(url, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        });
        return response.data.value[0]?.id || null;
    } catch (error) {
        return { error: error.response?.data?.error };
    }
}

// Helper function to get AI Insight details
async function getAiInsightDetails(accessToken, userId, onlineMeetingId, aiInsightId) {
    try {
        const url = `https://graph.microsoft.com/beta/copilot/users/${userId}/onlineMeetings/${onlineMeetingId}/aiInsights/${aiInsightId}`;
        const response = await axios.get(url, {
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        });
        return response.data.meetingNotes || null;
    } catch (error) {
        return null;
    }
}

// Listen for members added events
app.event('conversationUpdate', async (client) => {
    const membersAdded = client.activity.membersAdded;
    if (membersAdded && membersAdded.length > 0) {
        for (const member of membersAdded) {
            if (member.id !== client.activity.recipient.id) {
                await client.send('Welcome to TeamsBot. Type anything to get logged in. Type \'/signout\' to sign-out.');
            }
        }
    }
});

// Handle sign-out command
app.message('/signout', async (client) => {
    if (!client.isSignedIn) {
        await client.send('You are not signed in!');
        return;
    }
    await client.signout();
    await client.send('You have been signed out.');
});

// Handle all other messages
app.on('message', async (client) => {
    const text = client.activity.text?.trim().toLowerCase();
    
    // Skip if it's the signout command
    if (text === '/signout') {
        return;
    }

    // Check if user is signed in, if not trigger sign-in
    if (!client.isSignedIn) {
        await client.signin({
            oauthCardText: 'Please sign in to access meeting insights',
            signInButtonText: 'Sign In'
        });
        return;
    }

    // Get the access token from the authenticated client
    const accessToken = client.userToken;
    
    if (!accessToken) {
        await client.send('Failed to retrieve access token. Please try signing in again.');
        return;
    }

    // Get meeting insights
    try {
        const userId = config.userId;
        const meetingUrl = config.meetingJoinUrl;

        if (!userId || !meetingUrl) {
            await client.send('Please configure userId and meetingJoinUrl in your .env file.');
            return;
        }

        await client.send('Retrieving AI insights for your meeting...');

        // Get meeting ID
        const onlineMeetingId = await getMeetingId(accessToken, meetingUrl);
        if (!onlineMeetingId) {
            await client.send('Failed to retrieve meeting ID. Please check the meeting join URL.');
            return;
        }

        // Get AI Insight ID
        const aiInsightResult = await getAiInsightId(accessToken, userId, onlineMeetingId);
        if (!aiInsightResult) {
            await client.send('Failed to retrieve AI Insight ID. Please ensure the meeting has been recorded and Copilot has generated insights.');
            return;
        }
        
        // Check if there was an error
        if (aiInsightResult.error) {
            const errorMsg = aiInsightResult.error.message || 'Unknown error';
            if (errorMsg.includes('Copilot license')) {
                await client.send(`**Copilot License Required**\n\nThe user does not have a valid Microsoft 365 Copilot license. Please ensure:\n\n1. User has Copilot license assigned\n2. Copilot is enabled in your tenant\n3. Admin consent granted for required permissions`);
            } else if (errorMsg.includes('Forbidden') || errorMsg.includes('Unauthorized')) {
                await client.send(`**Permission Error**\n\n${errorMsg}\n\nPlease ensure admin consent has been granted for the OnlineMeetingAiInsight.Read.All permission.`);
            } else {
                await client.send(`**Error**: ${errorMsg}`);
            }
            return;
        }
        
        const aiInsightId = aiInsightResult;

        // Get AI Insight details
        const aiInsight = await getAiInsightDetails(accessToken, userId, onlineMeetingId, aiInsightId);
        if (aiInsight) {
            let formattedMessage = '';
            if (Array.isArray(aiInsight)) {
                formattedMessage = aiInsight.map(insight => {
                    return `## ${insight.title}\n${insight.text}\n`;
                }).join('\n');
            }
            await client.send(formattedMessage || 'No insights found in the expected format.');
        } else {
            await client.send('Failed to retrieve AI Insight details.');
        }
    } catch (error) {
        await client.send('An error occurred while retrieving meeting insights. Please try again.');
    }
});

// Handle successful sign-in
app.event('signin', async (client) => {
    await client.send('You have been signed in successfully!');
});

module.exports = app;
