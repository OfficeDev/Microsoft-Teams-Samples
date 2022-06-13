const axios = require('axios');
const { Client } = require('@microsoft/microsoft-graph-client');
require('isomorphic-fetch');

class GraphHelper {
    constructor() {
        this._token = this.GetAccessToken();
        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        this.graphClient = Client.init({
            authProvider: (done) => {
                done(null, this._token); // First parameter takes an error if you can't get an access token.
            }
        });
    }
    
    /**
     * Gets application token.
     * @returns Application token.
     */
    GetAccessToken() {
        let qs = require('qs')
        const data = qs.stringify({
            'grant_type': 'client_credentials',
            'client_id': process.env.MicrosoftAppId,
            'scope': 'https://graph.microsoft.com/.default',
            'client_secret': process.env.MicrosoftAppPassword
        });

        return new Promise(async (resolve) => {
            const config = {
                method: 'post',
                url: 'https://login.microsoftonline.com/' + process.env.MicrosoftAppTenantId + '/oauth2/v2.0/token',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                data: data
            };

            await axios(config)
                .then(function (response) {
                    resolve((response.data).access_token)
                })
                .catch(function (error) {
                    resolve(error)
                });
        })
    }

    /**
     * Install application in user Group chat
     * @param {string} groupId Install application in user Group chat
     * @returns 
     */
    async AppinstallationforGroupAsync(groupId) {
        try {
            const appInternalId = await this.GetApplicationInternalId();

            const data = {
                'teamsApp@odata.bind': 'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/' + appInternalId
            };

            await this.graphClient.api(`/chats/${groupId}/installedApps`).post(data);
        }
        catch (ex) {
            console.log(ex);
        }
    }

    /**
     * Creates group chat.
     * @param {string[]} userMails Members mail to be added in group chat.
     * @param {string} groupTitle Title of the group chat.
     * @returns Created chat details.
     */
    async CreateGroupChatAsync(userMails, groupTitle) {
        try 
        {
            let members = [];
            let distinctMailList = [...new Set(userMails)];
            distinctMailList.forEach(mail => {
                members.push({
                    '@odata.type': '#microsoft.graph.aadUserConversationMember',
                    roles: ['owner'],
                    'user@odata.bind': 'https://graph.microsoft.com/v1.0/users/' + mail
                  });
            });
    
            const data = {
                'chatType': 'group',
                'topic': groupTitle,
                'members': members
            };
    
            return await this.graphClient.api('/chats').post(data);
        }
        catch (ex) {
            console.log(ex);
        }
    }

    /**
     * Gets the user's photo
     * @param {string} userMail User mail.
     * @returns Profile photo in base64 format.
     */ 
    async GetProfilePictureByUserPrincipalNameAsync(userMail) {
        try 
        {
            const userInfo = await this.graphClient.api(`/users/${userMail}`).get();

            const response = await this.graphClient.api(`/users/${userInfo.id}/photo/$value`).get();

            var imageString = await response.arrayBuffer().then(result => {                
                return "data:image/png;base64," + Buffer.from(result).toString('base64');
            }); 

            return imageString;
        }
        catch (ex)  {
            console.log(ex);
        }
    }

    /**
     * Gets the application internal Id.
     * @returns Application internal Id.
     */
    async GetApplicationInternalId() {
        try
        {
            var apps =  await this.graphClient.api(`/appCatalogs/teamsApps`).filter(`externalId eq '${process.env.MicrosoftAppId}'`).get();

            if (apps.value.length > 0) {
                return apps.value[0].id;
            }

            return "";
        }
        catch (ex)
        {
            throw ex;
        }
        
    }
}
module.exports = GraphHelper;