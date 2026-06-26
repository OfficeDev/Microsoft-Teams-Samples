const axios = require('axios');
class ProactiveAppIntallationHelper {
    async GetAccessToken(MicrosoftTenantId) {
        let qs = require('qs');
        console.log('Getting access token for tenant:', MicrosoftTenantId);
        console.log('Using client ID:', process.env.MicrosoftAppId);
        
        const data = qs.stringify({
            'grant_type': 'client_credentials',
            'client_id': process.env.MicrosoftAppId,
            'scope': 'https://graph.microsoft.com/.default',
            'client_secret': process.env.MicrosoftAppPassword
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
                console.log('Access token obtained successfully');
                if (response.data && response.data.access_token) {
                    resolve(response.data.access_token);
                } else {
                    console.error('Unexpected token response format:', response.data);
                    resolve(null);
                }
            } catch (error) {
                console.error('Error getting access token:', error.message);
                if (error.response) {
                    console.error('Error details:', JSON.stringify(error.response.data, null, 2));
                    console.error('Error status:', error.response.status);
                }
                resolve(null);
            }
        });
    }

    async InstallAppInPersonalScope(MicrosoftTenantId, Userid) {
        return new Promise(async (resolve) => {
            console.log('Installing app for user:', Userid);
            console.log('AppCatalogTeamAppId:', process.env.AppCatalogTeamAppId);
            
            if (!process.env.AppCatalogTeamAppId) {
                console.error('AppCatalogTeamAppId is not defined in environment variables');
                resolve('Error: AppCatalogTeamAppId is not defined');
                return;
            }
            
            let accessToken = await this.GetAccessToken(MicrosoftTenantId);
            if (!accessToken || typeof accessToken !== 'string') {
                console.error('Invalid access token received');
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
            
            console.log('Making Graph API request to:', config.url);
            
            try {
                const response = await axios(config);
                console.log('App installation successful:', response.status);
                resolve(response.data.status);
            } catch (error) {
                console.error('App installation error:', error.message);
                if (error.response) {
                    console.error('Error details:', JSON.stringify(error.response.data, null, 2));
                    console.error('Error status:', error.response.status);
                }
                
                try {
                    const objProactiveAppIntallationHelper = new ProactiveAppIntallationHelper();
                    await objProactiveAppIntallationHelper.TriggerConversationUpdate(MicrosoftTenantId, Userid);
                    
                    if (error.response) {
                        resolve(error.response.status);
                    } else {
                        resolve('Error: ' + error.message);
                    }
                } catch (innerError) {
                    console.error('Error in TriggerConversationUpdate:', innerError.message);
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
                console.error('Invalid access token received in TriggerConversationUpdate');
                resolve('Error: Invalid access token');
                return;
            }
            
            const config = {
                method: 'get',
                url: 'https://graph.microsoft.com/v1.0/users/' + Userid + '/teamwork/installedApps?$expand=teamsApp,teamsAppDefinition&$filter=teamsApp/externalId eq \'' + process.env.MicrosoftAppId + '\'',
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
                            console.error('Error in InstallAppInPersonalChatScope:', chatError.message);
                        }
                    }
                } else {
                    console.log('No installed apps found for the user');
                }
                
                resolve(result);
            } catch (error) {
                console.error('Error in TriggerConversationUpdate:', error.message);
                if (error.response) {
                    console.error('Error details:', JSON.stringify(error.response.data, null, 2));
                }
                resolve(error);
            }
        });
    }

    async InstallAppInPersonalChatScope(accessToken, Userid, id) {
        return new Promise(async (resolve) => {
            console.log('Installing app in personal chat scope for user:', Userid);
            
            if (!accessToken || typeof accessToken !== 'string') {
                console.error('Invalid access token received in InstallAppInPersonalChatScope');
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
                console.log('Chat installation successful:', response.status);
                resolve(response);
            } catch (error) {
                console.error('Chat installation error:', error.message);
                if (error.response) {
                    console.error('Error details:', JSON.stringify(error.response.data, null, 2));
                }
                resolve(error);
            }
        });
    }
}

module.exports = ProactiveAppIntallationHelper;
