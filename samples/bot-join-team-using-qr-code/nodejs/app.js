const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const { SimpleGraphClient } = require('./simpleGraphClient');
const adaptiveCards = require('./adaptiveCard');
const config = require('./config');
const path = require('path');
const QRCode = require('qrcode');
const cors = require('cors');

// Create storage for conversation history
const storage = new LocalStorage();
const teamData = {};

// Create the app with storage and OAuth configuration
const app = new App({
  storage,
  oauth: {
    defaultConnectionName: process.env.CONNECTION_NAME || "oauthbotsetting",
  },
});

// Handle install event
app.on("install.add", async ({ send }) => {
  await send("Thanks for installing the bot! Please type **/signin** to sign in and then use the bot to generate QR codes for teams.");
});

// Handle sign out command
app.message("/signout", async ({ send, signout, isSignedIn }) => {
  if (!isSignedIn) {
    await send("You are not signed in.");
    return;
  }
  await signout();
  await send("You have been signed out!");
});

// Handle signin command
app.message('/signin', async ({ send, signin, isSignedIn }) => {
  if (isSignedIn) {
    send('You are already signed in! Please type **/generate** to generate QR codes or **/signout** to sign out.');
  } else {
    await signin();
  }
});

// Handle successful sign-in event
app.event('signin', async ({ send, token, activity }) => {
  const conversationId = activity.conversation.id;
  await storage.set(conversationId, {
    token: token.token
  });
  
  await send(`Signed in using OAuth connection ${token.connectionName}. Please type **/generate** to generate QR codes or **/signout** to sign out.`);
});

// Handle message with /generate command
app.message('/generate', async ({ send, isSignedIn, activity }) => {
  if (!isSignedIn) {
    await send('You are not signed in! Please type **/signin** to sign in.');
    return;
  }
  // Only send adaptive card in personal conversation
  if (activity.conversation.conversationType === 'personal') {
    const card = adaptiveCards.getAdaptiveCardUserDetails();
    await send(card);
  } else {
    await send('Please use this command in personal chat to generate QR codes for teams.');
  }
});

// Handle any other message
app.on('message', async ({ send, activity, isSignedIn }) => {
  if (isSignedIn) {
    await send(`You said: "${activity.text}". Please type **/generate** to generate QR codes or **/signout** to sign out.`);
  } else {
    await send(`You said: "${activity.text}". Please type **/signin** to sign in.`);
  }
});

// Handle task module fetch - when user clicks "Generate QR code" button
app.on('dialog.open', async ({ send, activity, context }) => {
  const conversationId = activity.conversation.id;
  const tokenData = await storage.get(conversationId);
  
  if (!tokenData || !tokenData.token) {
    return {
      task: {
        type: 'continue',
        value: {
          title: 'Authentication Required',
          height: 200,
          width: 400,
          card: {
            contentType: 'application/vnd.microsoft.card.adaptive',
            content: {
              type: 'AdaptiveCard',
              version: '1.4',
              body: [{
                type: 'TextBlock',
                text: 'Please sign in first to generate QR codes.',
                wrap: true
              }]
            }
          }
        }
      }
    };
  }
  
  try {
    const client = new SimpleGraphClient(tokenData.token);
    const response = await client.getAllTeams();
    const teamsData = response.value.slice(0, 5).map(team => ({
      id: team.id,
      name: team.displayName
    }));
    teamData.data = teamsData;
    const baseUrl = process.env.BASE_URL || `https://${process.env.BOT_DOMAIN}`;
    return {
      task: {
        type: 'continue',
        value: {
          title: 'Join Team',
          height: 350,
          width: 350,
          url: baseUrl + '/teamDetails'
        }
      }
    };
  } catch (error) {
    console.error('Error fetching teams:', error);
    return {
      task: {
        type: 'continue',
        value: {
          title: 'Error',
          height: 200,
          width: 400,
          card: {
            contentType: 'application/vnd.microsoft.card.adaptive',
            content: {
              type: 'AdaptiveCard',
              version: '1.4',
              body: [{
                type: 'TextBlock',
                text: 'Error fetching teams. Please try again.',
                wrap: true
              }]
            }
          }
        }
      }
    };
  }
});

// Handle task module submit - when user submits team selection
app.on('dialog.submit', async ({ send, activity, context }) => {
  const taskData = activity.value.data;
  const teamId = taskData.teamId;
  const userId = taskData.userId;
  const conversationId = activity.conversation.id;
  const tokenData = await storage.get(conversationId); 
  if (!tokenData || !tokenData.token) {
    await send('Authentication required. Please sign in again.');
    return null;
  }  
  try {
    const client = new SimpleGraphClient(tokenData.token);
    await client.joinTeam(userId, teamId);
    await send('You are successfully added into the team.');
  } catch (error) {
    console.error('Error joining team:', error);
    await send('Error adding you to the team. Please try again.');
  }
  return null;
});

setupCustomRoutes();

function setupCustomRoutes() {
  const expressApp = app.http.express;
  if (expressApp) {
    expressApp.use(cors());
    expressApp.engine('html', require('ejs').renderFile);
    expressApp.set('view engine', 'ejs');
    expressApp.set('views', __dirname);
    expressApp.use("/Images", require('express').static(path.resolve(__dirname, 'Images')));
    expressApp.use("/node_modules", require('express').static(path.resolve(__dirname, 'node_modules'), {
      setHeaders: (res, filePath) => {
        if (filePath.endsWith('.js')) {
          res.setHeader('Content-Type', 'text/javascript');
        }
      }
    }));
    
    // Route to generate QR code page
    expressApp.get('/generate', (req, res) => {
      res.render('./views/generate');
    });
    
    // Route to generate QR code image.
    expressApp.get('/qrcode', async (req, res) => {
      const teamId = req.query.teamId;
      if (!teamId) return res.status(400).send('Missing teamId query parameter');     
      try {
        const qrData = await QRCode.toDataURL(teamId);
        res.render('./views/qrcode', { qrData: JSON.stringify(qrData) });
      } catch (err) {
        console.error(err);
        res.status(500).send('Error generating QR code');
      }
    });
    
    // Route to get team details for task module
    expressApp.get('/teamDetails', (req, res) => {
      const teamDetails = teamData.data || [];
      res.render('./views/generate', { teamDetails: JSON.stringify(teamDetails) });
    });
    
    // Catch-all route
    expressApp.get('*', (req, res, next) => {
      if (req.path === '/api/messages') {
        return next();
      }
      res.json({ error: 'Route not found' });
    });
  }
}

module.exports = app;
