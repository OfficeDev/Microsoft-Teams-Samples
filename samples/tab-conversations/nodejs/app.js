const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const path = require('path');

// Create storage for conversation history
const storage = new LocalStorage();

const createTokenFactory = () => {
  return async (scope, tenantId) => {
    const managedIdentityCredential = new ManagedIdentityCredential({
      clientId: process.env.CLIENT_ID,
    });
    const scopes = Array.isArray(scope) ? scope : [scope];
    const tokenResponse = await managedIdentityCredential.getToken(scopes, {
      tenantId: tenantId,
    });

    return tokenResponse.token;
  };
};

// Configure authentication using TokenCredentials
const tokenCredentials = {
  clientId: process.env.CLIENT_ID || "",
  token: createTokenFactory(),
};

const credentialOptions =
  config.MicrosoftAppType === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

// Create the app with storage
const app = new App({
  ...credentialOptions,
  storage,
});

app.tab('configure', path.join(__dirname, 'tabs/configure'));
app.tab('conversationTab', path.join(__dirname, 'tabs/conversationTab'));
app.tab('static', path.join(__dirname, 'static'));
app.tab('images', path.join(__dirname, 'images'));

app.tab('root', __dirname); 

app.event('start', async () => {
  console.log('App started - attempting to add legacy routes directly to internal server...');

  try {
    if (app._server) {
      console.log('Found internal server, adding legacy routes...');
      app._server.get('/configure', (req, res) => {
        res.redirect('/tabs/configure');
      });
      app._server.get('/conversationTab', (req, res) => {
        res.redirect('/tabs/conversationTab');
      });
      console.log('Legacy routes added successfully!');
    } else {
      console.log('Internal server not accessible through app._server');
    }
  } catch (error) {
    console.log('Could not add legacy routes:', error.message);
    console.log('Legacy redirect files available at /tabs/root/configure.html and /tabs/root/conversationTab.html');
  }
});


module.exports = app;
