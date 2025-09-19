const { MicrosoftAppCredentials, ConnectorClient } = require('botframework-connector');
const querystring = require("querystring");
const storeService = require('./storageService');
const fetch = require('node-fetch');

getAccessToken = async(req)=> {
    return new Promise((resolve, reject)=>{
        // console.log("==============================getAccessToken Service==============================");
        let meetingId = req.query.meetingId;
        let userId = req.query.userId;
        let tenantId = req.query.tenantId;
        const authHeader = req.headers.authorization;
        const token = authHeader.split(' ')[1];
        if(!storeService.storeCheck("scopes")){
            const scopes = ["https://graph.microsoft.com/User.Read", "https://graph.microsoft.com/email", "https://graph.microsoft.com/offline_access", "https://graph.microsoft.com/openid", "https://graph.microsoft.com/profile"] //, "https://graph.microsoft.com/Calendars.Read"] //,"https://graph.microsoft.com/OnlineMeetings.Read.All"] // "https://graph.microsoft.com/OnlineMeetings.Read", "https://graph.microsoft.com/OnlineMeetings.ReadWrite"] //, "https://graph.microsoft.com/Calendars.Read", "https://graph.microsoft.com/Calendars.ReadWrite"];
            storeService.storeSave("scopes", scopes);
        }
        // console.log(storeService.storeCheck("scopes"));
        let scopes = storeService.storeFetch("scopes");

        const url = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`;
        const params = {
            client_id: process.env.clientId,
            client_secret: process.env.clientSecret,
            grant_type: "urn:ietf:params:oauth:grant-type:jwt-bearer",
            assertion: token,
            requested_token_use: "on_behalf_of",
            scope: scopes.join(" ")
        };
    
        fetch(url, {
          method: "POST",
          body: querystring.stringify(params),
          headers: {
            Accept: "application/json",
            "Content-Type": "application/x-www-form-urlencoded"
        }
        }).then(result => {
          if (result.status !== 200) {
            result.json().then(json => {
                console.log(json);
              // TODO: Check explicitly for invalid_grant or interaction_required
              reject({"error":json.error});
            }).catch(error => {
                console.error("Error parsing JSON:", error);
                reject(error);
            });
          } else {
            result.json().then(async json => {
              // console.log(json);
              resolve(json.access_token);
            }).catch(error => {
                console.error("Error parsing JSON:", error);
                reject(error);
            });
          }
        })
        .catch((error) => {
          console.error("Fetch error:", error);
          reject(error);
        });
    });
}

getAccessToken1 = async(req)=> {
    return new Promise((resolve, reject) => {
        console.log("==============================getAccessToken Service==============================");
        // console.log(req);
        let meetingId = req.query.meetingId;
        let userId = req.query.userId;
        let tenantId = req.query.tenantId;
        const authHeader = req.headers.authorization;
        const token = authHeader.split(' ')[1];
        console.log(meetingId);
        console.log(userId);
        console.log(tenantId);
        console.log(token);
        // tenantId ="0d9b645f-597b-41f0-a2a3-ef103fbd91bb"
        if(!storeService.storeCheck("scopes")){
            const scopes = ["https://graph.microsoft.com/User.Read", "https://graph.microsoft.com/email", "https://graph.microsoft.com/offline_access", "https://graph.microsoft.com/openid", "https://graph.microsoft.com/profile"] //, "https://graph.microsoft.com/Calendars.Read"] //,"https://graph.microsoft.com/OnlineMeetings.Read.All"] // "https://graph.microsoft.com/OnlineMeetings.Read", "https://graph.microsoft.com/OnlineMeetings.ReadWrite"] //, "https://graph.microsoft.com/Calendars.Read", "https://graph.microsoft.com/Calendars.ReadWrite"];
            storeService.storeSave("scopes", scopes);
        }
        // console.log(storeService.storeCheck("scopes"));
        let scopes = storeService.storeFetch("scopes");
        console.log(scopes);

        const url = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`;
        const params = {
            client_id: process.env.clientId,
            client_secret: process.env.clientSecret,
            grant_type: "urn:ietf:params:oauth:grant-type:jwt-bearer",
            assertion: token,
            requested_token_use: "on_behalf_of",
            scope: scopes.join(" ")
        };
        
        fetch(url, {
            method: "POST",
            body: querystring.stringify(params),
            headers: {
                Accept: "application/json",
                "Content-Type": "application/x-www-form-urlencoded"
            }
        }).then(result => {
            if (result.status !== 200) {
                result.json().then(json => {
                    console.log(json);
                    // TODO: Check explicitly for invalid_grant or interaction_required
                    reject({"error":json.error});
                }).catch(error => {
                    console.error("Error parsing JSON:", error);
                    reject(error);
                });
            } else {
                result.json().then(async json => {
                    try {
                        // console.log(json);
                        //   resolve(json.access_token);
                        let accessToken = json.access_token;
                        console.log("----------------------------------------------------------------------------");
                        console.log(accessToken);
                        // let serviceURI = `https://graph.microsoft.com`
                        let serviceURI = "https://smba.trafficmanager.net/amer"
                        //
                        const credentials = new MicrosoftAppCredentials(process.env.BotId, process.env.BotPassword);
                        const connector = new ConnectorClient(credentials, { baseUri: serviceURI });
                        let botToken = await credentials.getToken()
                        MicrosoftAppCredentials.trustServiceUrl(serviceURI);
                        let url = `${serviceURI}/v1/meetings/${meetingId}/participants/${userId}?tenantId=${tenantId}`
                        fetch(url, {
                            method: "GET",
                            headers: {
                                "Content-Type": "application/json",
                                "Authorization": `Bearer ${botToken}`
                            }
                        }).then(result => {
                            if (result.status !== 200) {
                                result.json().then(json => {
                                    console.log(json);
                                    // TODO: Check explicitly for invalid_grant or interaction_required
                                    reject({"error":json.error});
                                }).catch(error => {
                                    console.error("Error parsing JSON:", error);
                                    reject(error);
                                });
                            } else {
                                result.json().then(json => {
                                    console.log(json);
                                    resolve(json);
                                }).catch(error => {
                                    console.error("Error parsing JSON:", error);
                                    reject(error);
                                });
                            }
                        }).catch(error => {
                            console.error("Fetch error:", error);
                            reject(error);
                        });
                    } catch (error) {
                        console.error("Error in access token processing:", error);
                        reject(error);
                    }
                }).catch(error => {
                    console.error("Error parsing JSON:", error);
                    reject(error);
                });
            }
        }).catch(error => {
            console.error("Fetch error:", error);
            reject(error);
        });
    });
}

getParticipantRole = async (accessToken, meetingId, userId, tenantId) => {
    return new Promise(async (resolve, reject)=>{
        try {
            // let accessToken = json.access_token;
            // console.log("----------------------------------------------------------------------------");
            // console.log(accessToken);
            if(!storeService.storeCheck("serviceurl")){
                const serviceURI = "https://smba.trafficmanager.net/amer";
                storeService.storeSave("serviceurl", serviceURI);
            }
            let serviceURI = storeService.storeFetch("serviceurl");
            const credentials = new MicrosoftAppCredentials(process.env.BotId, process.env.BotPassword);
            const connector = new ConnectorClient(credentials, { baseUri: serviceURI });
            let botToken = await credentials.getToken()

            MicrosoftAppCredentials.trustServiceUrl(serviceURI);
            let url = `${serviceURI}/v1/meetings/${meetingId}/participants/${userId}?tenantId=${tenantId}`
            fetch(url, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${botToken}`
                }
            }).then(result => {
                if (result.status !== 200) {
                    result.json().then(json => {
                        console.log(json);
                    // TODO: Check explicitly for invalid_grant or interaction_required
                    reject({"error":json.error});
                    }).catch(error => {
                        console.error("Error parsing JSON:", error);
                        reject(error);
                    });
                } else {
                    result.json().then(json => {
                    // console.log(json);
                    storeService.storeSave("conversationid", json.conversation.id);
                    resolve(json);
                    }).catch(error => {
                        console.error("Error parsing JSON:", error);
                        reject(error);
                    });
                }
            }).catch(error => {
                console.error("Fetch error:", error);
                reject(error);
            });
        } catch (error) {
            console.error("Error in getParticipantRole:", error);
            reject(error);
        }
    });
}

module.exports = {
    getAccessToken,
    getParticipantRole
}