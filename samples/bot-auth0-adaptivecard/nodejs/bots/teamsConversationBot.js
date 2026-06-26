const { TeamsActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const TokenStore = require('../services/TokenStore');
const fetch = require('node-fetch');

class TeamsConversationBot extends TeamsActivityHandler {
    constructor() {
        super();
        
        this.onMessage(async (context, next) => {
            const userId = context.activity.from.id;
            const text = context.activity.text?.trim().toLowerCase();

            // Handle logout
            if (text === 'logout') {
                TokenStore.removeToken(userId);
                const logoutUrl = `https://${process.env.AUTH0_DOMAIN}/v2/logout?client_id=${process.env.AUTH0_CLIENT_ID}`;
                
                const logoutCard = {
                    type: "AdaptiveCard",
                    version: "1.3",
                    body: [
                        {
                            type: "TextBlock",
                            text: "You've been logged out.",
                            size: "Medium",
                            weight: "Bolder",
                            wrap: true
                        }
                    ],
                    actions: [
                        {
                            type: "Action.OpenUrl",
                            title: "Logout from Auth0",
                            url: logoutUrl
                        }
                    ]
                };

                await context.sendActivity({
                    attachments: [CardFactory.adaptiveCard(logoutCard)]
                });
                await next();
                return;
            }

            // Handle profile details
            const accessToken = TokenStore.getToken(userId);
            if (accessToken) {
                if (text === 'profile details') {
                    try {
                        const response = await fetch(`https://${process.env.AUTH0_DOMAIN}/userinfo`, {
                            method: 'GET',
                            headers: {
                                Authorization: `Bearer ${accessToken}`,
                            },
                        });

                        if (response.ok) {
                            const profileData = await response.json();

                            const profileCard = {
                                type: "AdaptiveCard",
                                version: "1.3",
                                body: [
                                    {
                                        type: "TextBlock",
                                        text: `Auth0 Profile`,
                                        size: "Large",
                                        weight: "Bolder",
                                        wrap: true
                                    },
                                    {
                                        type: "Image",
                                        url: profileData.picture || "https://via.placeholder.com/150",
                                        size: "Medium",
                                        style: "Person"
                                    },
                                    {
                                        type: "TextBlock",
                                        text: `Name: ${profileData.name}`,
                                        wrap: true
                                    },
                                    {
                                        type: "TextBlock",
                                        text: `Email: ${profileData.email}`,
                                        wrap: true
                                    }
                                ]
                            };

                            await context.sendActivity({
                                attachments: [CardFactory.adaptiveCard(profileCard)]
                            });
                        } else {
                            await context.sendActivity(MessageFactory.text('Failed to fetch profile details.'));
                        }
                    } catch (err) {
                        console.error('Error fetching profile:', err);
                        await context.sendActivity(MessageFactory.text('Error retrieving profile details.'));
                    }
                } else {
                    await context.sendActivity(MessageFactory.text("Say 'profile details' to get your profile or 'logout' to log out."));
                }
            } else {
                // Prompt for login
                const loginUrl = this.generateLoginUrl(userId);

                const loginCard = {
                    type: "AdaptiveCard",
                    version: "1.3",
                    body: [
                        {
                            type: "TextBlock",
                            text: "Login Required",
                            size: "Medium",
                            weight: "Bolder",
                            wrap: true
                        }
                    ],
                    actions: [
                        {
                            type: "Action.OpenUrl",
                            title: "Login",
                            url: loginUrl
                        }
                    ]
                };

                await context.sendActivity({
                    attachments: [CardFactory.adaptiveCard(loginCard)]
                });
            }

            await next();
        });
    }

    generateLoginUrl(userId) {
        return `https://${process.env.AUTH0_DOMAIN}/authorize` +
            `?response_type=code&client_id=${process.env.AUTH0_CLIENT_ID}` +
            `&redirect_uri=${process.env.APP_URL}/api/auth/callback` +
            `&scope=openid profile email` +
            `&state=${encodeURIComponent(userId)}`;
    }
}

module.exports.TeamsConversationBot = TeamsConversationBot;