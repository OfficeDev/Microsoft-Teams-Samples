// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    TeamsActivityHandler,
    MemoryStorage,
    TurnContext,
    UserState,
    ActionTypes,
    MessageFactory,
    CardFactory,
} = require('botbuilder');

const msal = require("@azure/msal-node");
const axios = require("axios");

class EchoBot extends TeamsActivityHandler {

    userState = null;
    tokenAccessor = null;
    baseUrl = null;

    constructor(baseUrl) {
        super();

        const memoryStorage = new MemoryStorage();
        this.userState = new UserState(memoryStorage);
        this.tokenAccessor = this.userState.createProperty("accessToken");
        this.baseUrl = baseUrl;

        // Whenever the app is installed, show the user a welcome message
        this.onInstallationUpdateAdd(async (context, next) => {
            await context.sendActivity(
                MessageFactory.attachment(
                    CardFactory.heroCard(
                        "Hello, I'm the consent bot", 
                        "Ask me to 'get profile' and I'll try and make a connection to Graph to get some information about you. If I need consent, I'll walk you through the process, using a sign-in card, and a modal window.", 
                        null, 
                        [
                            {
                                type: ActionTypes.ImBack,
                                title: "Help",
                                value: "help"
                            }
                        ])));
            await next();
        });

        // When a text message is received...
        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);

            // Convert the text to it's canonical form
            const text = context.activity.text.trim().toLocaleLowerCase().replace(/[\s-]/g, "");

            // Get a token (if sign in has taken place)
            const token = await this.tokenAccessor.get(context);

            switch (text) {
                // Display a card with some helpful info
                case "hello": case "help":       

                    const card = CardFactory.heroCard(
                        "Hi, I'm the Azure AD Consent Bot!",
                        "I can get information about you from Graph API, and display it in a hero card. To do this, click Get Profile... Or if you want to provide Consent, click Provide Consent. If you click Get Profile, and consent isn't already in place, I'm smart enough to deal with this and I'll walk you through that process!",
                        null,
                        [
                            this.provideAdminConsentAction(),
                            this.provideUserConsentAction(),
                            this.getProfileActionApp(),
                            this.getProfileActionDelegated()
                        ]);

                    await context.sendActivity(MessageFactory.attachment(card));
                    break;

                // Show the user their access token
                case "showaccesstoken":
                    if (!token) {
                        await this.signIn(context);
                        break;
                    }
                    
                    await context.sendActivity(`Here is the Delegated Permissions Access Token I have stored in User State for this user, that will be swapped for a Graph API access token when required: ${token}`)
                
                    break;

                // Perform a sign in
                case "signin":
                    await this.signIn(context);
                    break;
            
                // We have two ways to get a profile - via app tokens and via delegated tokens so display a card to allow the user to choose
                case "getprofile":
                    await context.sendActivity(
                        MessageFactory.attachment(
                            CardFactory.heroCard(
                                "I can help with that!", 
                                "Do you want me to get your profile using an app permissions or a delegated permission token?", 
                                null, 
                                [
                                    this.getProfileActionApp(),
                                    this.getProfileActionDelegated()
                                ])));
                    break;

                // Get the users profile using an app token
                case "getprofileapp":
                    
                    const userAadId = context.activity.from.aadObjectId;
                    const tenantId = context.activity.channelData.tenant.id;

                    const appToken = await this.getTokenForApp(tenantId);

                    // All graph requests require the token to exist in the Authorization header
                    const requestOptions = {
                        headers: {
                            Authorization: `Bearer ${appToken.accessToken}`
                        }
                    };

                    try{
                        // Get the users profile
                        const meResponse = await axios.default.get(`https://graph.microsoft.com/v1.0/users/${userAadId}?$select=displayName,department,jobTitle,state,officeLocation`, requestOptions);
                        // Get the users profile image
                        const image = await this.getImageUrl(appToken.accessToken, userAadId);
                        
                        // Return the profile to the client
                        await context.sendActivity(
                            MessageFactory.attachment(
                                this.getProfileAdaptiveCard(image, meResponse.data.displayName, "Cannot get presence with app token", meResponse.data.officeLocation, meResponse.data.jobTitle, meResponse.data.department)));
                    }
                    catch(e) {
                        // Oh oh! Something went wrong here... let's see if we know what the issue is...
                        switch (e.response.data.error.code) {
                            // RequestDenied means that there are no 'roles' (scopes) on the access token, therefore we need to go through a Consent Process to get the customer to consent to these roles being available on your access token...
                            case "Authorization_RequestDenied":
                            // IdentityNotFound means that there is no Enterprise App that has been established in the customer tenant, therefore we need to go through a Consent Process to get this setup...
                            case "Authorization_IdentityNotFound":
                                await context.sendActivity(`Here is the token I used for authentication with Graph API: ${appToken.accessToken}`);
                                await context.sendActivity("Unfortunately I couldn't pull information on your user via Graph API, as you need to provide consent. Please decode this token by visiting https://jwt.ms/ and review the roles claim. Let's use a sign-in card to get consent, and then try sending 'get profile' to me again please.");

                                // Send down a card to being the app consent process
                                await context.sendActivity(
                                    MessageFactory.attachment(
                                        CardFactory.signinCard("Consent Required!", `${this.baseUrl}/LoginStart.html?appId=${process.env.MicrosoftAppId}`, "Provide Consent")));

                                break;
                        }
                    }

                    break;

                // Get the users profile using a delegated token
                case "getprofiledelegated":
                    if (!token) {
                        await this.signIn(context);
                        break;
                    }
                    
                    try {
                        // Swap the initial token for a delegated token...
                        const tokenResult = await this.getOnBehalfOf(token);

                        // All graph requests require the token to exist in the Authorization header
                        const requestOptions = {
                            headers: {
                                Authorization: `Bearer ${tokenResult.accessToken}`
                            }
                        };

                        // Get the users profile
                        const meResponse = await axios.default.get("https://graph.microsoft.com/v1.0/me?$select=displayName,department,jobTitle,state,officeLocation", requestOptions);
                        // Get the users availability
                        const presenceResponse = await axios.default.get("https://graph.microsoft.com/v1.0/me/presence", requestOptions);

                        // Get the users profile image
                        const image = await this.getImageUrl(tokenResult.accessToken);

                        // Return the profile as a card to the client
                        await context.sendActivity(
                            MessageFactory.attachment(
                                this.getProfileAdaptiveCard(image, meResponse.data.displayName, presenceResponse.data.availability, meResponse.data.officeLocation, meResponse.data.jobTitle, meResponse.data.department)));

                    }
                    catch(e) {
                        // Oh oh! Something went wrong here... let's see if we know what the issue is...

                        // This means that the scopes we are asking for have not been consented to. therefore we need to walk through an interactive consent process
                        // to do this, we'll send a card with a sign-in action. That then walks the end-user through the process of consenting to the scopes we need
                        if (e.errorCode === "invalid_grant" || e.errorCode === "invalid_resource") {
                            await context.sendActivity(
                                MessageFactory.attachment(
                                    CardFactory.heroCard(
                                        "User Consent Required", 
                                        `ERROR: ${e.errorCode}. I need you to provide Consent so I can make some calls to the Graph API to find out some information about you. Please click the button below to complete the Consent process...`,
                                        null,
                                        [
                                            this.provideUserConsentAction()
                                        ])));
                        }
                        // This means that the token in user state has expired, or is not valid. In that case, we need to send down the oAuth card to get a new access token in userState...
                        else if (e.errorCode === "invalid_assertion" || e.errorCode === "invalid_jwt_token") {
                            await this.signIn(context);
                        }
                    }

                    break;

                // Another command... we don't support that!
                default:
                    await context.sendActivity("Sorry, I didn't understand that, try sending 'hello' or 'help' to get started :)");
                    break;
            }

            await next();
        });
    }

    // Gets the profile image for a user
    // if the userId isn't passed, we'll just use "me"
    async getImageUrl(token, userId) {
        let image = "";
        try {

            // Download the image
            const imageResponse = await axios.default.get(`https://graph.microsoft.com/v1.0/${userId ? `users/${userId}` : "me"}/photo/$value`, { headers: {Authorization: `Bearer ${token}`}, responseType: "arraybuffer"});
            
            // Convert it to a data url
            image = "data:" + imageResponse.headers["content-type"] + ";base64," + Buffer.from(imageResponse.data, "binary").toString('base64');

        }
        catch(e) {
            // Couldn't find the image (or something went wrong) - set a default image
            image = `${this.baseUrl}/missing.jpg`;
        }

        return image;
    }

    // Sends the OAuth card to the user to allow them to sign in
    async signIn(context) {
        await context.sendActivity(
            MessageFactory.attachment(
                CardFactory.oauthCard(process.env.OAuthConnectionName, "Sign-in!", "Sign-in!", null, {id:"showAccessToken"})));
    }

    // Gets the card action to display a button that will post a text message in the chat
    getProfileActionApp() {
        return {
            type: ActionTypes.ImBack,
            title: "Get My Profile (app permissions)",
            text: "Get Profile - app",
            value: "Get Profile - app",
            displayText: "Get My Profile (app permissions)"
        }
    }

    // Gets the card action to display a button that will post a text message in the chat
    getProfileActionDelegated() {
        return {
            type: ActionTypes.ImBack,
            title: "Get My Profile (delegated permissions)",
            text: "Get Profile - delegated",
            value: "Get Profile - delegated",
            displayText: "Get My Profile (delegated permissions)"
        }
    }

    // Gets the card action to display a button that will launch a new window to allow them to consent
    provideUserConsentAction() {
        return {
            type: ActionTypes.Signin,
            title: "Provide User Consent",
            text: "Provide User Consent",
            value: `${this.baseUrl}/LoginStart.html?userConsent=true&appId=${process.env.MicrosoftAppId}`
        }
    }

    // Gets the card action to display a button that will launch a new window to allow them to consent
    provideAdminConsentAction() {
        return {
            type: ActionTypes.Signin,
            title: "Provide Admin Consent",
            text: "Provide Admin Consent",
            value: `${this.baseUrl}/LoginStart.html?appId=${process.env.MicrosoftAppId}`
        }
    }

    // Handles the callback from the consent process
    async handleTeamsSigninVerifyState(context, query) {
        let activity = null;
        // Was this admin consent?
        if (context.activity.value.state === "AdminConsent") {
            activity = MessageFactory.attachment(
                CardFactory.heroCard(
                    "Admin Consent Successful", 
                    "Admin Consent was successful! Click Get Profile to test Graph API functionality now consent has been granted for all users in this tenant.",
                    null,
                    [
                        this.getProfileActionApp(),
                        this.getProfileActionDelegated()
                    ]));
        }
        // Or perhaps user consent?
        else if (context.activity.value.state === "UserConsent") {
            activity = MessageFactory.attachment(
                CardFactory.heroCard(
                    "User Consent Successful", 
                    "User Consent was successful! Click Get Profile to test Graph API functionality now consent has been granted for this user!",
                    null,
                    [
                        this.getProfileActionDelegated()
                    ]));
        }
        
        // Update the existing activity to show the new card and actions
        activity.id = context.activity.replyToId;
        await context.updateActivity(activity);
    }

    // Handles the callback from an OAuth card
    async handleTeamsSigninTokenExchange(context, query) {
        // Make sure the activity is "signin/tokenExchange" - this method will also be called for "signin/verifyState" which we've already handled above
        if (context.activity.name === "signin/tokenExchange") {
            // Get the token from the activity value
            const token = context.activity.value.token;
            // Save the token in user state
            await this.tokenAccessor.set(context, token);

            await context.sendActivity(
                MessageFactory.attachment(
                    CardFactory.heroCard(
                        "You have successfully signed in", 
                        "The bot has now saved your access token to user state. Please send your command again to try again.", null, 
                        [
                            this.provideAdminConsentAction(),
                            this.provideUserConsentAction(),
                            this.getProfileActionApp(),
                            this.getProfileActionDelegated()
                        ])));
        }
    }

    // Gets an app token for a given tenant using MSAL
    async getTokenForApp(tenantId) {
        const cca = new msal.ConfidentialClientApplication({
            auth: {
                clientId: process.env.MicrosoftAppId,
                clientSecret: process.env.MicrosoftAppPassword,
                authority: `https://login.microsoftonline.com/${tenantId}`
            }
        });

        return await cca.acquireTokenByClientCredential({
            scopes: [".default"]
        });
    }

    // Gets a delegated token for a given Teams access token (with access_as_user scope)
    async getOnBehalfOf(accessToken) {
        const cca = new msal.ConfidentialClientApplication({
            auth: {
                clientId: process.env.MicrosoftAppId,
                clientSecret: process.env.MicrosoftAppPassword
            }
        });

        return await cca.acquireTokenOnBehalfOf({
            oboAssertion: accessToken,
            // We've provided scopes manually here, but you could use "/.default" for all delegated scopes that have been configured in the App Reg
            scopes: ["https://graph.microsoft.com/User.Read", "https://graph.microsoft.com/Presence.Read"]
        });
    }

    // Gets the adaptive card for a profile
    getProfileAdaptiveCard(imageUrl, profileName, availability, officeLocation, jobTitle, department) {
        return CardFactory.adaptiveCard({
          type: "AdaptiveCard",
          body: [
            {
              type: "TextBlock",
              size: "Medium",
              weight: "Bolder",
              text: "My Profile",
            },
            {
              type: "ColumnSet",
              columns: [
                {
                  type: "Column",
                  items: [
                    {
                      size: "Small",
                      style: "Person",
                      type: "Image",
                      url: imageUrl,
                    },
                  ],
                  width: "auto",
                },
                {
                  type: "Column",
                  items: [
                    {
                      type: "TextBlock",
                      weight: "Bolder",
                      text: profileName,
                      wrap: true,
                    },
                    {
                      type: "TextBlock",
                      spacing: "None",
                      text: availability,
                      isSubtle: true,
                      wrap: true,
                    },
                  ],
                  width: "stretch",
                },
              ],
            },
            {
              type: "TextBlock",
              text: "This Profile Card was generated by making Graph API calls",
              wrap: true,
            },
            {
              type: "FactSet",
              facts: [
                {
                  title: "Office Location: ",
                  value: officeLocation,
                },
                {
                  title: "Job Title:",
                  value: jobTitle,
                },
                {
                  title: "Department:",
                  value: department,
                },
              ],
            },
          ],
          actions: [
            {
              type: "Action.Submit",
              title: "Show User (delegated) Access Token",
              data: {
                msteams: {
                  type: "messageBack",
                  text: "showAccessToken",
                },
              },
            },
          ],
          $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
          version: "1.4",
        });
    }

    // This is the entry point for _all_ messages sent to the bot
    async run(context) {
        // We first allow the base class to handle the processing (and bubble back up to the methods above)
        await super.run(context);
        // Persist any changes that were made to user state during this request
        await this.userState.saveChanges(context, false);
    }
}

module.exports = EchoBot;