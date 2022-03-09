// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
const express = require('express');
const cors = require('cors');
const { SimpleGraphClient } = require('./simpleGraphClient');
const msal = require('@azure/msal-node');
const axios = require('axios')
const userDetails = {};

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3978;
const server = express();
let multer = require('multer');
let upload = multer({ storage: multer.memoryStorage() });

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
  extended: true
}));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);
server.use("/Images", express.static(path.resolve(__dirname, 'Images')));
// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { CloudAdapter,
  ConfigurationServiceClientCredentialFactory,
  createBotFrameworkAuthenticationFromConfiguration, ConversationState, MemoryStorage, UserState } = require('botbuilder');
const { TeamsBot } = require('./bots/teamsBot');
const { MainDialog } = require('./dialogs/mainDialog');

const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
  MicrosoftAppId: process.env.MicrosoftAppId,
  MicrosoftAppPassword: process.env.MicrosoftAppPassword,
  MicrosoftAppType: process.env.AppType,
  MicrosoftAppTenantId: process.env.TenantId
});

const botFrameworkAuthentication = createBotFrameworkAuthenticationFromConfiguration(null, credentialsFactory);

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
  // This check writes out errors to console log .vs. app insights.
  // NOTE: In production environment, you should consider logging this to Azure
  //       application insights. See https://aka.ms/bottelemetry for telemetry
  //       configuration instructions.
  console.error(`\n [onTurnError] unhandled error: ${error}`);

  // Send a trace activity, which will be displayed in Bot Framework Emulator
  await context.sendTraceActivity(
    'OnTurnError Trace',
    `${error}`,
    'https://www.botframework.com/schemas/error',
    'TurnError'
  );

  // Note: Since this Messaging Extension does not have the messageTeamMembers permission
  // in the manifest, the bot will not be allowed to message users.
};

// Define the state store for your bot.
// See https://aka.ms/about-bot-state to learn more about using MemoryStorage.
// A bot requires a state storage system to persist the dialog and user state between messages.
const memoryStorage = new MemoryStorage();

// Create conversation and user state with in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);
const userState = new UserState(memoryStorage);

// Create the main dialog.
const dialog = new MainDialog();
// Create the bot that will handle incoming messages.
const bot = new TeamsBot(conversationState, userState, dialog);

// Create HTTP server.
server.listen(PORT, () => {
  console.log(`Server listening on http://localhost:${PORT}`);
});

// Endpoint to fetch Auth tab page.
server.get('/tab', (req, res, next) => {
  var clientId = {
    clientIdFb: process.env.FaceBookAppId,
    clientIdGoogle: process.env.GoogleAppId
  };
  res.render('./views/tab', { clientId: JSON.stringify(clientId) })
});

// Pop-up dialog to ask for additional permissions, redirects to AAD page
server.get('/auth-start', function (req, res) {
  var clientId = process.env.MicrosoftAppId;
  res.render('./views/auth-start', { clientId: JSON.stringify(clientId) });
});

// End of the pop-up dialog auth flow, returns the results back to parent window
server.get('/auth-end', function (req, res) {
  res.render('./views/auth-end');
});

server.get('/config', function (req, res) {
  res.render('./views/config');
});

// Endpoint to facebook auth redirect page.
server.get('/fb-auth', function (req, res) {
  res.render('./views/fb-auth');
});

// Endpoint to google auth redirect page.
server.get('/google-auth', function (req, res) {
  res.render('./views/google-auth');
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
  // Route received a request to adapter for processing
  await adapter.process(req, res, (context) => bot.run(context));
});

// On-behalf-of token exchange
server.post('/getProfileOnBehalfOf', function (req, res) {
  var tid = req.body.tid;
  var token = req.body.token;
  var userName = req.body.userName;
  var scopes = ["https://graph.microsoft.com/User.Read"];

  // Creating MSAL client
  const msalClient = new msal.ConfidentialClientApplication({
    auth: {
      clientId: process.env.MicrosoftAppId,
      clientSecret: process.env.MicrosoftAppPassword
    }
  });

  var oboPromise = new Promise((resolve, reject) => {
    msalClient.acquireTokenOnBehalfOf({
      authority: `https://login.microsoftonline.com/${tid}`,
      oboAssertion: token,
      scopes: scopes,
      skipCache: true
    }).then(async result => {
      const client = new SimpleGraphClient(result.accessToken);
      const myDetails = await client.getMeAsync();
      var userImage = await client.getUserPhoto()
      await userImage.arrayBuffer().then(result => {
        imageString = Buffer.from(result).toString('base64');
        img2 = "data:image/png;base64," + imageString;
        var userData = {
          details: myDetails,
          image: img2
        }

        var currentData = userDetails["userDetails"];
        if(currentData == undefined){
          const userDetailsList = new Array();
          userDetailsList.push({"aad_id":userName,"is_aad_signed_in":true});
          currentData = userDetailsList;
          userDetails["userDetails"] = currentData;
        }
        else if (!currentData.find((user) => {
              if(user.aad_id == userName){
                return true;
              }
            }))
          {
            const userDetailsList = currentData;
            userDetailsList.push({"aad_id":userName,"is_aad_signed_in":true});
            currentData = userDetailsList;
            userDetails["userDetails"] = currentData;
          }
        resolve(userData);
      })
    }).catch(error => {
      reject({ "error": error.errorCode });
    });
  });

  oboPromise.then(function (result) {
    res.json(result);
  }, function (err) {// Error: "It broke"
    res.json(err);
  });
});

server.post('/getUserInfo',(req,res) =>{
  var userName = req.body.userName;
  var userData;
  var currentData = userDetails["userDetails"];
  if(currentData == undefined || !currentData.find((user) => {
    if(user.aad_id == userName){
        userData = user;
      return true;
    }
  })){
    res.json("NoDataFound");
  }
  else{
    res.json(userData);
  }
})

server.post('/disconnectFromGoogle',(req,res) =>{
  var userName = req.body.userName;
  var userData;
  var currentData = userDetails["userDetails"];
  let updateindex;
  currentData.map((user, index) => {
    if (user.aad_id == userName) {
      updateindex = index;
      userData = user;
      userData['google_id'] = null;
      userData['google_token'] = null;
      userData['is_google_signed_in'] = false;
    }
  })
    currentData[updateindex] = userData;
    userDetails["userDetails"]=currentData;
    res.json("disconnected from google");
})

server.post('/disconnectFromFb',(req,res) =>{
  var userName = req.body.userName;
  var userData;
  var currentData = userDetails["userDetails"];
  let updateindex;
  currentData.map((user, index) => {
    if (user.aad_id == userName) {
      updateindex = index;
      userData = user;
      userData['facebook_id'] = null;
      userData['facebook_token'] = null;
      userData['is_fb_signed_in'] = false;
    }
  })
    currentData[updateindex] = userData;
    userDetails["userDetails"]=currentData;
    res.json("disconnected from facebook");
})

// Listen for incoming requests.
server.post('/GetUserDetails', async (req, res) => {
  var accessToken = req.body.accessToken;
  var userName = req.body.userName;
  const client = new SimpleGraphClient(accessToken);
  const myDetails = await client.getMeAsync();
  var userImage = await client.getUserPhoto();
  var userData;
  await userImage.arrayBuffer().then(result => {
    imageString = Buffer.from(result).toString('base64');
    img2 = "data:image/png;base64," + imageString;
    userData = {
      details: myDetails,
      image: img2
    }
    var currentData = userDetails["userDetails"];
      if(currentData == undefined){
        const userDetailsList = new Array();
        userDetailsList.push({"aad_id":userName,"is_aad_signed_in":true});
        currentData = userDetailsList;
        userDetails["userDetails"] = currentData;
      }
      else if (!currentData.find((user) => {
            if(user.aad_id == userName){
              return true;
            }
          }))
      {
        const userDetailsList = currentData;
        userDetailsList.push({"aad_id":userName,"is_aad_signed_in":true});
        currentData = userDetailsList;
        userDetails["userDetails"] = currentData;
      }
  });
  var responseMessage = new Promise((resolve, reject) => {
    resolve(userData)
  });
    responseMessage.then(function (result) {
      res.json(result);
    }, function (err) { // Error: "It broke"
      res.json(err);
    });
});

// Facebook Oauth token axchange
server.post('/getFbAccessToken', function (req, res) {
  var token = req.body.token;
  var userName = req.body.userName;
  var accessToken;
  var scopes = ['name','picture','id'].join(',');

  var fbPromise = new Promise((resolve, reject) => {
    axios.get('https://graph.facebook.com/v12.0/oauth/access_token', {
      params: {
        client_id: process.env.FaceBookAppId,
        redirect_uri: process.env.ApplicationBaseUrl + '/fb-auth',
        client_secret: process.env.FaceBookAppPassword,
        code: token,
      }
    }).then(response => {
      accessToken = response.data.access_token;
      axios.get('https://graph.facebook.com/v2.6/me', {
        params: {
          fields: scopes,
          access_token: accessToken,
        }
      }).then(profile => {
        var userData;
        var currentData = userDetails["userDetails"];
        let updateindex;
        currentData.map((user, index) => {
          if (user.aad_id == userName) {
            updateindex = index;
            userData = user;
            userData['facebook_id'] = profile.data.id;
            userData['facebook_token'] = accessToken;
            userData['is_fb_signed_in'] = true;
          }
        })
        currentData[updateindex] = userData;
        userDetails["userDetails"]=currentData;
        resolve(profile.data);
      })
    }).catch(error => {
      reject({ "error": error.errorCode });
    });
  });

  fbPromise.then(function (result) {
    res.json(result);
  }, function (err) {// Error: "It broke"
    res.json(err);
  });
});

server.post('/getGoogleAccessToken', function (req, res) {
  var token = req.body.token;
  var userName = req.body.userName;
  var accessToken;
  var googlePromise = new Promise((resolve, reject) => {
    axios.post('https://oauth2.googleapis.com/token', {
        client_id: process.env.GoogleAppId,
        redirect_uri: process.env.ApplicationBaseUrl + '/google-auth',
        client_secret: process.env.GoogleAppPassword,
        code: token,
        grant_type:"authorization_code"
    }).then(response => {
      accessToken = response.data.access_token;
      axios.get('https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls', {
        headers: {
          "Authorization": `Bearer ${accessToken}`,
        }
      }).then(profile => {
        var userData;
        var currentData = userDetails["userDetails"];
        let updateindex;
        currentData.map((user, index) => {
          if (user.aad_id == userName) {
            updateindex = index;
            userData = user;
            userData["google_id"] = profile.data.emailAddresses[0].value;
            userData["google_token"] = accessToken;
            userData["is_google_signed_in"] = true;
          }
        })
        currentData[updateindex] = userData;
        userDetails["userDetails"]=currentData;
        resolve(profile.data);
      })
    }).catch(error => {
      reject({ "error": error.errorCode });
    });
  });

  googlePromise.then(function (result) {
    res.json(result);
  }, function (err) { // Error: "It broke"
    res.json(err);
  });
});

server.post('/getGoogleDetails', function (req,res){
  var token = req.body.token;
  var googlePromise = new Promise((resolve, reject) => {
    axios.get('https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls', {
      headers: {
        "Authorization": `Bearer ${token}`,
      }
    }).then(profile => {
      resolve(profile.data);
    }).catch((err)=>{
      reject(err);
    })
  })

  googlePromise.then(function (result) {
    res.json(result);
  }, function (err) { // Error: "It broke"
    res.json(err);
  });
})

server.post('/getFbDetails',function (req,res){
  var token = req.body.token;
  var scopes = ['name','picture','id'].join(',');
  var fbPromise = new Promise((resolve, reject) => {  
    axios.get('https://graph.facebook.com/v2.6/me', {
      params: {
        fields: scopes,
        access_token: token,
      }
    }).then(profile => {
      resolve(profile.data);
    }).catch(error => {
      reject(error);
    });
  })

  fbPromise.then(function (result) {
    res.json(result);
  }, function (err) { // Error: "It broke"
    res.json(err);
  });
})