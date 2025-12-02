const axios = require('axios');

class ProactiveAppIntallationHelper {
    async GetAccessToken(MicrosoftTenantId) {
        let qs = require('querystring');
        
        const data = qs.stringify({
            'grant_type': 'client_credentials',
            'client_id': process.env.CLIENT_ID,
            'scope': 'https://graph.microsoft.com/.default',
            'client_secret': process.env.CLIENT_PASSWORD
        });
        
        return new Promise(async (resolve) => {
            const config = {
                method: 'post',
                url: 'https://login.microsoftonline.com/' + MicrosoftTenantId + '/oauth2/v2.0/token',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                data: data
            };
            
            try {
                const response = await axios(config);
                if (response.data && response.data.access_token) {
                    resolve(response.data.access_token);
                } else {
                    resolve(null);
                }
            } catch (error) {
                console.error('Error getting access token:', error.response?.data || error.message);
                resolve(null);
            }
        });
    }

    async InstallAppInPersonalScope(MicrosoftTenantId, Userid) {
        return new Promise(async (resolve) => {
            if (!process.env.AppCatalogTeamAppId) {
                console.error('AppCatalogTeamAppId is not defined in environment variables');
                resolve('Error: AppCatalogTeamAppId is not defined');
                return;
            }
            
            let accessToken = await this.GetAccessToken(MicrosoftTenantId);
            if (!accessToken || typeof accessToken !== 'string') {
                resolve('Error: Invalid access token');
                return;
            }
            
            const data = JSON.stringify({
                'teamsApp@odata.bind': 'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/' + process.env.AppCatalogTeamAppId
            });
            
            const config = {
                method: 'post',
                url: 'https://graph.microsoft.com/v1.0/users/' + Userid + '/teamwork/installedApps',
                headers: {
                    'Content-type': 'application/json',
                    'Authorization': 'Bearer ' + accessToken
                },
                data: data
            };
            
            try {
                const response = await axios(config);
                resolve(response.status);
            } catch (error) {
                try {
                    const objProactiveAppIntallationHelper = new ProactiveAppIntallationHelper();
                    await objProactiveAppIntallationHelper.TriggerConversationUpdate(MicrosoftTenantId, Userid);
                    
                    if (error.response) {
                        resolve(error.response.status);
                    } else {
                        resolve('Error: ' + error.message);
                    }
                } catch (innerError) {
                    resolve('Error: ' + error.message);
                }
            }
        });
    }

    async TriggerConversationUpdate(MicrosoftTenantId, Userid) {
        const objProactiveAppIntallationHelper = new ProactiveAppIntallationHelper();
        return new Promise(async (resolve) => {
            let accessToken = await this.GetAccessToken(MicrosoftTenantId);
            
            if (!accessToken || typeof accessToken !== 'string') {
                resolve('Error: Invalid access token');
                return;
            }
            
            const config = {
                method: 'get',
                url: 'https://graph.microsoft.com/v1.0/users/' + Userid + '/teamwork/installedApps?$expand=teamsApp,teamsAppDefinition&$filter=teamsApp/externalId eq \'' + process.env.CLIENT_ID + '\'',
                headers: {
                    'Authorization': 'Bearer ' + accessToken
                }
            };

            try {
                const response = await axios(config);
                const Map_installedApps = response.data.value.map(element => element.teamsApp.externalId);
                let result = true;
                
                if (Map_installedApps && Map_installedApps.length > 0) {
                    for (const apps of response.data.value) {
                        try {
                            result = await objProactiveAppIntallationHelper.InstallAppInPersonalChatScope('Bearer ' + accessToken, Userid, apps.id);
                        } catch (chatError) {
                            // Silently handle chat installation errors
                        }
                    }
                }
                
                resolve(result);
            } catch (error) {
                console.error('Error in TriggerConversationUpdate:', error.response?.data || error.message);
                resolve(error);
            }
        });
    }

    async InstallAppInPersonalChatScope(accessToken, Userid, id) {
        return new Promise(async (resolve) => {
            if (!accessToken || typeof accessToken !== 'string') {
                resolve('Error: Invalid access token');
                return;
            }
            
            const config = {
                method: 'get',
                url: 'https://graph.microsoft.com/v1.0/users/' + Userid + '/teamwork/installedApps/' + id + '/chat',
                headers: {
                    'Authorization': accessToken
                }
            };
            
            try {
                const response = await axios(config);
                resolve(response);
            } catch (error) {
                resolve(error);
            }
        });
    }
}

module.exports = ProactiveAppIntallationHelper;
