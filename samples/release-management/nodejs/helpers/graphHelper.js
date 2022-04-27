const axios = require('axios');
class GraphHelper {
    
    /**
     * Gets application token.
     * @returns Application token.
     */
    async GetAccessToken() {
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
        return new Promise(async (resolve) => {
            let accessToken = await this.GetAccessToken();
            const data = JSON.stringify({
                'teamsApp@odata.bind': 'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/' + process.env.AppExternalId
            });
            const config = {
                method: 'post',
                url: 'https://graph.microsoft.com/v1.0/chats/'+groupId+'/installedApps',
                headers: {
                    'Content-type': 'application/json',
                    'Authorization': accessToken
                },
                data: data
            };
            await axios(config)
                .then(function (response) {
                    resolve((response.data).status)
                })
                .catch(async function (error) {
                    throw error;
                });
        })
    }

    /**
     * Creates group chat.
     * @param {string[]} userMails Members mail to be added in group chat.
     * @returns Created chat details.
     */
    async CreateGroupChatAsync(userMails) {
        let members = [];
        let distinctMailList = [...new Set(userMails)];
        distinctMailList.forEach(mail => {
            members.push({
                '@odata.type': '#microsoft.graph.aadUserConversationMember',
                roles: ['owner'],
                'user@odata.bind': 'https://graph.microsoft.com/v1.0/users/' + mail
              });
        });

        return new Promise(async (resolve) => {
            let accessToken = await this.GetAccessToken();
            const data = JSON.stringify({
                'chatType': 'group',
                'members': members
            });
            const config = {
                method: 'post',
                url: 'https://graph.microsoft.com/v1.0/chats/',
                headers: {
                    'Content-type': 'application/json',
                    'Authorization': accessToken
                },
                data: data
            };
            await axios(config)
                .then(function (response) {
                    resolve(response.data)
                })
                .catch(async function (error) {
                    throw error;
                });
        })
    }
}
module.exports = GraphHelper;