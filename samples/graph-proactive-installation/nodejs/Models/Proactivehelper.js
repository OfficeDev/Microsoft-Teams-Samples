const axios = require('axios');
class Proactive_Messages {
    static async GetAceessToken(MicrosoftTenatId) {
        var qs = require('qs')
        var data=qs.stringify({
            'grant_type':'client_credentials',
            'client_id': process.env.MicrosoftAppId,
            'scope': 'https://graph.microsoft.com/.default',
            'client_secret': process.env.MicrosoftAppPassword
        });
        return new Promise(async (resolve)=>{
            var config = {
                method: 'post',
                url: 'https://login.microsoftonline.com/'+MicrosoftTenatId+'/oauth2/v2.0/token',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                data:data
            };
    
            await axios(config)
                .then(function (response) {
                    resolve((response.data).access_token)
                })
                .catch(function (error,data) {
                    resolve(error)
                });
        })
    }


    static async CheckAppInstalledinPersonalScope(MicrosoftTenatId,Userid) {
        return new Promise(async (resolve)=>{
            var accessToken=await Proactive_Messages.GetAceessToken(MicrosoftTenatId);
            var config = {
                method: 'get',
                url: 'https://graph.microsoft.com/v1.0/users/'+Userid+'/teamwork/installedApps?$expand=teamsApp,teamsAppDefinition',
                headers: {
                    'Authorization': accessToken
                }
            };
    
            axios(config)
                .then(async function (response) {
                    var installedApps=response.data;
                    var Map_installedApps = installedApps.value.map(element =>  element.teamsApp.externalId);
                    var Check_AppExists = Map_installedApps.indexOf(process.env.MicrosoftAppId) > -1;
                    if(Check_AppExists)
                    {
                        var statusCode=await Proactive_Messages.InstallApp_PersonalScope(accessToken,Userid);
                        resolve(statusCode);
                    }
                    else{
                        resolve(409);
                    }
                })
                .catch(function (error) {
                    resolve(error);
                });
        })
    }

    static async InstallApp_PersonalScope(accessToken,Userid) {
        return new Promise(async (resolve)=>{
            var data=JSON.stringify({
                'teamsApp@odata.bind':'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/'+process.env.AppCatalogTeamAppId
            });
            var config = {
                method: 'post',
                url:  'https://graph.microsoft.com/v1.0/users/'+Userid+'/teamwork/installedApps',
                headers: {
                    'Content-type' : 'application/json',
                    'Authorization': accessToken
                },
                data:data
            };
    
            await axios(config)
                .then(function (response) {
                   resolve((response.data).status)
                })
                .catch(function (error) {
                    resolve((error.response).status)
                });
        })
    }



    static async CheckAppInstalledinChannelScope(MicrosoftTenatId,TeamId) {
        var accessToken=await Proactive_Messages.GetAceessToken(MicrosoftTenatId);
        return new Promise(async (resolve)=>{
            var config = {
                method: 'get',
                url: 'https://graph.microsoft.com/v1.0/teams/'+TeamId+'/installedApps??$expand=teamsApp,teamsAppDefinition',
                headers: {
                    'Authorization': accessToken
                }
            };
    
            await axios(config)
                .then(async function (response) {
                    var installedApps=response.data;
                    var Map_installedApps = installedApps.value.map(element =>  element.teamsApp.externalId);
                    var Check_AppExists = Map_installedApps.indexOf(process.env.MicrosoftAppId) > -1;
                    if(Check_AppExists)
                    {
                        var statusCode=await Proactive_Messages.InstallApp_ChannelScope(accessToken,TeamId);
                        resolve(statusCode);
                    }
                    else{
                    }
                })
                .catch(function (error) {
                    resolve(error);
                });
        })
    }

    static async InstallApp_ChannelScope(accessToken,TeamId) {
        return new Promise(async (resolve)=>{
            var data=JSON.stringify({
                'teamsApp@odata.bind':'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/'+process.env.AppCatalogTeamAppId
            });
            var config = {
                method: 'post',
                url:  'https://graph.microsoft.com/v1.0/teams/'+TeamId+'/installedApps',
                headers: {
                    'Content-type' : 'application/json',
                    'Authorization': accessToken
                },
                data:data
            };
    
            await axios(config)
                .then(function (response) {
                   resolve(response.status)
                })
                .catch(function (error) {
                    resolve((error.response).status)
                });
        })
    }


    static async CheckAppInstalledinGroupChatScope(MicrosoftTenatId,ChatId) {
        var accessToken=await Proactive_Messages.GetAceessToken(MicrosoftTenatId);
        return new Promise(async (resolve)=>{
            var config = {
                method: 'get',
                url: 'https://graph.microsoft.com/v1.0/chats/'+ChatId+'/installedApps?$expand=teamsApp,teamsAppDefinition',
                headers: {
                    'Authorization': accessToken
                }
            };
    
            await axios(config)
                .then(async function (response) {
                    var installedApps=response.data;
                    var Map_installedApps = installedApps.value.map(element =>  element.teamsApp.externalId);
                    var Check_AppExists = Map_installedApps.indexOf(process.env.MicrosoftAppId) > -1;
                    if(Check_AppExists)
                    {
                        var statusCode=await Proactive_Messages.InstallApp_GroupChatScope(accessToken,ChatId);
                        resolve(statusCode);
                    }
                    else{
                    }
                })
                .catch(function (error) {
                    resolve(error);
                });
        })
    }

    static async InstallApp_GroupChatScope(accessToken,ChatId) {
        return new Promise(async (resolve)=>{
            var data=JSON.stringify({
                'teamsApp@odata.bind':'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/'+process.env.AppCatalogTeamAppId
            });
            var config = {
                method: 'post',
                url: 'https://graph.microsoft.com/v1.0/chats/'+ChatId+'/installedApps',
                headers: {
                    'Content-type' : 'application/json',
                    'Authorization': accessToken
                },
                data:data
            };
    
            await axios(config)
                .then(function (response) {
                   resolve(response.status)
                })
                .catch(function (error) {
                    resolve((error.response).status)
                });
        })
    }

   

}

module.exports=Proactive_Messages;

