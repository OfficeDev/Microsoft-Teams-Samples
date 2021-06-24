const axios = require('axios');
class ProactiveAppIntallationHelper {
     async GetAccessToken(MicrosoftTenantId) {
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
                url: 'https://login.microsoftonline.com/' + MicrosoftTenantId + '/oauth2/v2.0/token',
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

     async InstallAppInPersonalScope(MicrosoftTenantId, Userid) {
        return new Promise(async (resolve) => {
            let accessToken = await this.GetAccessToken(MicrosoftTenantId);
            const data = JSON.stringify({
                'teamsApp@odata.bind': 'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/' + process.env.AppCatalogTeamAppId
            });
            const config = {
                method: 'post',
                url: 'https://graph.microsoft.com/v1.0/users/' + Userid + '/teamwork/installedApps',
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
                    const objProactiveAppIntallationHelper=new ProactiveAppIntallationHelper();
                    await objProactiveAppIntallationHelper.TriggerConversationUpdate(MicrosoftTenantId, Userid);
                    resolve((error.response).status);
                });
        })
    }

     async TriggerConversationUpdate(MicrosoftTenantId, Userid) {
        const objProactiveAppIntallationHelper=new ProactiveAppIntallationHelper();
        return new Promise(async (resolve) => {
            let accessToken = await this.GetAccessToken(MicrosoftTenantId);
            const config = {
                method: 'get',
                url: 'https://graph.microsoft.com/v1.0/users/' + Userid + '/teamwork/installedApps?$expand=teamsApp,teamsAppDefinition&$filter=teamsApp/externalId eq \'' + process.env.MicrosoftAppId + '\'',
                headers: {
                    'Authorization': accessToken
                }
            };

            axios(config)
                .then(async function (response) {
                    const Map_installedApps = response.data.value.map(element => element.teamsApp.externalId);
                    if (Map_installedApps != null) {
                        installedApps.value.forEach(async apps => {
                            let result = await objProactiveAppIntallationHelper.InstallAppInPersonalChatScope(accessToken, Userid, apps.id);
                        });
                    }
                    resolve(result);
                })
                .catch(function (error) {
                    resolve(error);
                });
        })
    }

      async InstallAppInPersonalChatScope(accessToken, Userid, id) {
        return new Promise(async (resolve) => {
            const config = {
                method: 'get',
                url: 'https://graph.microsoft.com/v1.0/users/' + Userid + '/teamwork/installedApps/' + id + '/chat',
                headers: {
                    'Authorization': accessToken
                }
            };
            await axios(config)
                .then(function (response) {
                    resolve(response)
                })
                .catch(function (error) {
                    resolve(error)
                });
        })
    }
}
module.exports = ProactiveAppIntallationHelper;