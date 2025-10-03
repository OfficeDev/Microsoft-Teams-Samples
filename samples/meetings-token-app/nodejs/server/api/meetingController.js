const { MicrosoftAppCredentials, ConnectorClient } = require('botframework-connector');
const querystring = require("querystring");
const fetch = require('node-fetch');

const accessTokenService = require('../services/accessTokenService');
const meetingService = require('../services/meetingService')

const getMeetingTokenAsync = async (req, res) => {
  console.log("==============================getMeetingTokenAsync==============================");
  let meetingId = req.query.meetingId;
  let userId = req.query.userId;
  let tenantId = req.query.tenantId;
  const authHeader = req.headers.authorization;
  const token = authHeader.split(' ')[1];
  let access_token = await accessTokenService.getAccessToken(req);
  // console.log(access_token);
  let participant = await accessTokenService.getParticipantRole(access_token, meetingId, userId, tenantId);
  // console.log(participant);
  let user_token = meetingService.generateToken(meetingId, participant)
  res.json(user_token)
}

const getMeetingStatusAsync = async (req, res) => {
    console.log("==============================getMeetingStatusAsync==============================");
    let meetingId = req.query.meetingId;
    let userId = req.query.userId;
    let tenantId = req.query.tenantId;
    const authHeader = req.headers.authorization;
    const token = authHeader.split(' ')[1];
    let access_token = await accessTokenService.getAccessToken(req);
    // console.log(access_token);
    let participant = await accessTokenService.getParticipantRole(access_token, meetingId, userId, tenantId);
    // console.log(participant);
    let meeting_summary = meetingService.getMeetingSummary(meetingId)
    // console.log(meeting_summary);
    res.json(meeting_summary);
    ////

    

    
}

const getUserInfoAsync = async (req, res) => {
  console.log("==============================getUserInfoAsync==============================");
  let meetingId = req.query.meetingId;
  let userId = req.query.userId;
  let tenantId = req.query.tenantId;
  const authHeader = req.headers.authorization;
  const token = authHeader.split(' ')[1];
  let access_token = await accessTokenService.getAccessToken(req);
  // console.log(access_token);
  let participant = await accessTokenService.getParticipantRole(access_token, meetingId, userId, tenantId);
  // console.log(participant);
  let userInfo = meetingService.getUserInfo(participant)
  res.json(userInfo)
}

const ackTokenAsync = async (req, res) => {
  console.log("==============================ackTokenAsync==============================");
  let meetingId = req.query.meetingId;
  let userId = req.query.userId;
  let tenantId = req.query.tenantId;
  const authHeader = req.headers.authorization;
  const token = authHeader.split(' ')[1];
  let access_token = await accessTokenService.getAccessToken(req);
  let participant = await accessTokenService.getParticipantRole(access_token, meetingId, userId, tenantId);
  let user_tokens = meetingService.ackToken(meetingId, participant)
  res.json(user_tokens)
}
const skipTokenAsync = async (req, res) => {
  console.log("==============================skipTokenAsync==============================");
  let meetingId = req.query.meetingId;
  let userId = req.query.userId;
  let tenantId = req.query.tenantId;
  const authHeader = req.headers.authorization;
  const token = authHeader.split(' ')[1];
  let access_token = await accessTokenService.getAccessToken(req);
  let participant = await accessTokenService.getParticipantRole(access_token, meetingId, userId, tenantId);
  let user_tokens = meetingService.skipToken(meetingId, participant)
  res.json(user_tokens)
}

// On-behalf-of token exchange
const authToken = (req, res)=> {
    console.log("---------------------------In Auth Token Controller------------------------------");
    console.log(process.env.clientId);
    console.log(process.env.clientSecret);
    var tid = req.body.tid;
    var token = req.body.token;
    console.log(tid);
    // console.log(token);
    var scopes = ["https://graph.microsoft.com/User.Read", "https://graph.microsoft.com/AppCatalog.ReadWrite.All", "https://graph.microsoft.com/AppCatalog.Submit"];

    // string[] scopes = { "AppCatalog.ReadWrite.All", "AppCatalog.Submit", "User.Read" };
    var oboPromise = new Promise((resolve, reject) => {
        const url = `https://login.microsoftonline.com/${tid}/oauth2/v2.0/token`;
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
            result.json().then(json => {
              // console.log(json);
              // let accessToken = json.access_token;
              resolve(json.access_token);
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

    oboPromise.then(function(result) {
        res.json(result);
    }, function(err) {
        console.log(err); // Error: "It broke"
        res.json(err);
    });
}

const getUserProfile = (req, res)=> {
    console.log(req.body.accessToken);
    var oboPromise = new Promise((resolve, reject) => {
        let url = `https://graph.microsoft.com/v1.0/me/`;
        fetch(url, {
            method: "GET",
            headers: {
              "Content-Type": "application/json",
              "Authorization": `Bearer ${req.body.accessToken}`
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
    })
    oboPromise.then(function(result) {
        res.json(result);
    }, function(err) {
        console.log(err); // Error: "It broke"
        res.json(err);
    });
}

module.exports = {
    getMeetingTokenAsync,
    getMeetingStatusAsync,
    getUserInfoAsync,
    ackTokenAsync,
    skipTokenAsync,
    authToken,
    getUserProfile
};