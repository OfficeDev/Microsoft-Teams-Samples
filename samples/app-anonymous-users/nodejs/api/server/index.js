const express = require('express');
const PORT = process.env.PORT || 3000;
const app = express();
app.use(express.json());
app.use(express.urlencoded({
  extended: true
}));
const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });
const { SimpleGraphClient } = require('../server/simpleGraphClient');
const axios = require('axios')
const msal = require('@azure/msal-node');
const { polyfills } = require('isomorphic-fetch');

io.on('connection', (socket) => {
  socket.on('message', msg => {
    io.emit('message', msg);
  });
});

// Facebook Oauth token axchange
app.post('/getFacebookLoginUserInfo', function (req, res) {
  console.log("ServerFacebook");
  debugger;
  var token = req.body.token;
  var accessToken;
  var scopes = ['name','picture'].join(',');

  var fbPromise = new Promise((resolve, reject) => {
    axios.get('https://graph.facebook.com/v12.0/oauth/access_token', {
      params: {
        client_id: '{{FacebookAppId}}',
        redirect_uri: '{{domain-name}}/facebook-auth-end',
        client_secret: '{{Facebook App Password}}',
        code: token,
      }
    }).then(response => {
      console.log(response);
      accessToken = response.data.access_token;
      axios.get('https://graph.facebook.com/v2.6/me', {
        params: {
          fields: scopes,
          access_token: accessToken,
        }
      }).then(profile => {
        console.log(profile);
        resolve(profile.data);
      })
    }).catch(error => {
      reject({ "error": error.errorCode });
    });
  });

  fbPromise.then(function (result) {
    res.json(result);
    console.log(result);
  }, function (err) {
    console.log(err); // Error: "It broke"
    res.json(err);
  });
});

// On-behalf-of token exchange
app.post('/GetLoginUserInformation', function (req, res) {
  var tid = req.body.tid;
  var token = req.body.token;
  var scopes = ["https://graph.microsoft.com/User.Read", "openid"];

  // Creating MSAL client
  const msalClient = new msal.ConfidentialClientApplication({
    auth: {
      clientId: '{{Microsoft-App-id}}',
      clientSecret: '{{MicrosoftAppPassword}}'
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
      var userData = {
        details: myDetails
      }
      resolve(userData);
    }).catch(error => {
      reject({ "error": error.errorCode });
    });
  });

  oboPromise.then(function (result) {
    res.json(result);
  }, function (err) {
    console.log(err); // Error: "It broke"
    res.json(err);
  });
});

server.listen(PORT, () => {
  console.log(`Server listening on http://localhost:${PORT}`);
});