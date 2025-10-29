const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");

// Use built-in fetch (Node.js 18+) or polyfill
let fetch;
if (typeof globalThis.fetch === 'undefined') {
  // Fallback for older Node.js versions
  fetch = (...args) => import('node-fetch').then(({default: fetch}) => fetch(...args));
} else {
  fetch = globalThis.fetch;
}

// Create storage for conversation history
const storage = new LocalStorage();

// Conversation references store (used for proactive messaging)
const conversationReferences = {};

// Store the last message from each conversation for proactive messaging
const lastMessageStore = {};

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

// Create proper credentials for Teams AI v2
let appOptions = { storage };

if (config.MicrosoftAppType === "UserAssignedMsi") {
  // Production: Use managed identity token factory
  appOptions = {
    ...appOptions,
    clientId: process.env.CLIENT_ID || "",
    token: createTokenFactory(),
  };
} else {
  // Local development: Use client credentials directly
  appOptions = {
    ...appOptions,
    clientId: process.env.CLIENT_ID || "",
    clientSecret: process.env.CLIENT_SECRET || "",
  };
}

console.log('🔑 Environment check:', {
  CLIENT_ID: process.env.CLIENT_ID ? `${process.env.CLIENT_ID.substring(0, 8)}...` : 'undefined',
  CLIENT_SECRET: process.env.CLIENT_SECRET ? 'present' : 'undefined',
  BOT_TYPE: process.env.BOT_TYPE || 'undefined',
  TENANT_ID: process.env.TENANT_ID || 'undefined',
  BOT_ENDPOINT: process.env.BOT_ENDPOINT || 'undefined',
  BOT_DOMAIN: process.env.BOT_DOMAIN || 'undefined'
});

console.log('🔑 App configuration:', {
  clientId: appOptions.clientId ? `${appOptions.clientId.substring(0, 8)}...` : 'undefined',
  hasClientSecret: !!appOptions.clientSecret,
  hasToken: !!appOptions.token,
  appType: config.MicrosoftAppType || 'undefined'
});

// Create the app with storage and proper credentials
const app = new App(appOptions);

// Add custom endpoints for proactive messaging after the app is created
console.log('Available app properties:', Object.keys(app));

// Let's try to hook into the HTTP processing after app.start()
let routesAdded = false;

// Add custom HTTP processing function
app.addCustomHttpHandler = function() {
  // Try multiple approaches to add custom routes
  // Approach 1: Use Teams AI's http property if available
  if (this.http && typeof this.http.get === 'function') {
    console.log('✅ Found Teams AI HTTP server via app.http, adding custom routes...');
    addCustomRoutes(this.http);
    routesAdded = true;
  }
  // Approach 2: Use router property
  else if (this.router && typeof this.router.get === 'function') {
    console.log('✅ Found Teams AI router, adding custom routes...');
    addCustomRoutes(this.router);
    routesAdded = true;
  }
  // Approach 3: Check for internal express app
  else if (this._app && typeof this._app.get === 'function') {
    console.log('✅ Found internal express app, adding custom routes...');
    addCustomRoutes(this._app);
    routesAdded = true;
  }
  // Approach 4: Check for container with express
  else if (this.container && this.container.app && typeof this.container.app.get === 'function') {
    console.log('✅ Found container express app, adding custom routes...');
    addCustomRoutes(this.container.app);
    routesAdded = true;
  }
  
  if (!routesAdded) {
    console.log('⚠️ Could not find suitable server to add custom routes');
    console.log('Available methods on app.http:', this.http ? Object.getOwnPropertyNames(this.http) : 'undefined');
    console.log('Available methods on app.router:', this.router ? Object.getOwnPropertyNames(this.router) : 'undefined');
  }
  
  return routesAdded;
};

// Function to add custom routes to any express-like server
function addCustomRoutes(server) {
  console.log('🔍 Debugging server object:', {
    type: typeof server,
    constructor: server.constructor?.name,
    hasGet: typeof server.get === 'function',
    hasUse: typeof server.use === 'function',
    hasPost: typeof server.post === 'function',
    methods: Object.getOwnPropertyNames(server).slice(0, 10) // First 10 methods
  });
  
  // Test if the server actually supports route registration
  try {
    // Add a middleware to log all requests
    server.use((req, res, next) => {
      console.log(`📥 Incoming request: ${req.method} ${req.url}`);
      next();
    });
    
    // Add a simple test route first
    console.log('🧪 Testing route registration...');
    server.get('/api/test', (req, res) => {
      console.log('🎯 Test route hit!');
      res.status(200).send('Test route works!');
    });
    console.log('✅ Test route added successfully');
    
    // Add proactive messaging endpoint
    server.get('/api/notify', async (req, res) => {
      console.log("🎯 /api/notify route hit!");
      console.log("Handling /api/notify request via Teams AI server");
      try {
      const conversationReferences = app.getConversationReferences();
      console.log("Stored conversation references:", JSON.stringify(conversationReferences, null, 2));
      
      if (Object.keys(conversationReferences).length === 0) {
        res.status(200).send("<html><body><h1>No conversation references found. Please message the bot first.</h1></body></html>");
        return;
      }

      let messagesCount = 0;
      const results = [];
      
      // Get the stored messages
      const lastMessages = app.getLastMessages();
      
      for (const conversationReference of Object.values(conversationReferences)) {
        try {
          // Get the last message for this conversation, or use a default
          const lastMessageData = lastMessages[conversationReference.conversation.id];
          const messageToSend = lastMessageData ? 
            `Proactive ${lastMessageData.message} from the bot!` : 
            "Proactive hello from the Teams AI v2 bot!";
          
          const result = await app.sendProactiveMessage(conversationReference, messageToSend);
          results.push(result);
          messagesCount++;
        } catch (error) {
          console.error("Error sending proactive message to conversation:", conversationReference.conversation.id, error);
          results.push({
            success: false,
            conversationId: conversationReference.conversation.id,
            error: error.message
          });
        }
      }

      // Also show proactive message history
      const history = app.getProactiveMessageHistory();
      const internalAdapterInfo = app.getInternalAdapter();
      
      res.status(200).send(`
        <html>
          <body>
            <h1>✅ Proactive Messages have been sent !</h1>
          </body>
        </html>
      `);
    } catch (error) {
      console.error("Error sending proactive messages:", error);
      res.status(500).json({ code: "Internal", message: error.message });
    }
  });
  
  // Add conversations endpoint
  server.get('/api/conversations', async (req, res) => {
    console.log("Handling /api/conversations request via Teams AI server");
    
    try {
      const conversationReferences = app.getConversationReferences();
      const history = app.getProactiveMessageHistory();
      const lastMessages = app.getLastMessages();
      
      res.status(200).send(`
        <html>
          <body>
            <h1>Bot Status Dashboard - Dev Tunnel</h1>
            <h2>Stored Conversations (${Object.keys(conversationReferences).length})</h2>
            <pre>${JSON.stringify(conversationReferences, null, 2)}</pre>
            
            <h2>Last Messages for Proactive Messaging</h2>
            <pre>${JSON.stringify(lastMessages, null, 2)}</pre>
            
            <h2>Proactive Message History (${history.length})</h2>
            <pre>${JSON.stringify(history, null, 2)}</pre>
            
            <h2>Actions</h2>
            <p><a href="/api/notify">Send Proactive Messages</a></p>
            
            <p><em>Teams AI v2 Pure Implementation - Dev Tunnel Access</em></p>
          </body>
        </html>
      `);
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  });
  
  } catch (error) {
    console.error('❌ Error adding custom routes:', error);
  }
}

if (!routesAdded) {
  console.log('⚠️ Teams AI custom routes not available');
}

const getConversationState = (conversationId) => {
  let state = storage.get(conversationId);
  if (!state) {
    state = { count: 0 };
    storage.set(conversationId, state);
  }
  return state;
};

// Helper function to store conversation reference
const addConversationReference = (activity) => {
  const conversationReference = {
    activityId: activity.id,
    bot: activity.recipient,
    channelId: activity.channelId,
    conversation: activity.conversation,
    serviceUrl: activity.serviceUrl,
    user: activity.from
  };
  conversationReferences[conversationReference.conversation.id] = conversationReference;
  console.log(`Stored conversation reference for: ${conversationReference.conversation.id}`);
};

app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);

  console.log(`📨 Received message: "${text}" from ${activity.from.name}`);
  
  // Store conversation reference for proactive messaging
  addConversationReference(activity);
  
  console.log(`📝 Processing message handler for: ${text}`);

  if (text === "/reset") {
    storage.delete(activity.conversation.id);
    await context.send("Ok I've deleted the current conversation state.");
    return;
  }

  if (text === "/count") {
    const state = getConversationState(activity.conversation.id);
    await context.send(`The count is ${state.count}`);
    return;
  }

  if (text === "/diag") {
    await context.send(JSON.stringify(activity));
    return;
  }

  if (text === "/state") {
    const state = getConversationState(activity.conversation.id);
    await context.send(JSON.stringify(state));
    return;
  }

  if (text === "/runtime") {
    const runtime = {
      nodeversion: process.version,
      sdkversion: "2.0.0", // Teams AI v2
    };
    await context.send(JSON.stringify(runtime));
    return;
  }

  if (text === "/notify") {
    const port = process.env.PORT || 3978;
    
    // Always use localhost for development (no dev tunnel needed)
    let baseUrl = `http://localhost:${port}`;
    
    await context.send(`**Proactive Messaging Endpoints (Teams AI v2 Pure):**\n\n• Send Messages: ${baseUrl}/api/notify\n• View Dashboard: ${baseUrl}/api/conversations\n\n*Note: This implementation uses localhost - no dev tunnel needed*`);
    return;
  }

  // Store the message for proactive messaging
  lastMessageStore[activity.conversation.id] = {
    message: text,
    user: activity.from,
    timestamp: new Date().toISOString()
  };

  // Respond like the original sample - use localhost for development
  const port = process.env.PORT || 3978;
  
  // Always use localhost for development (no dev tunnel needed)
  let baseUrl = `http://localhost:${port}`;
  
  console.log(`🔗 Proactive URL: ${baseUrl}/api/notify`);
  
  const state = getConversationState(activity.conversation.id);
  state.count++;
  
  console.log(`📤 Sending response to user for message: "${text}"`);
  await context.send(`You sent '${text}'. Navigate to [${baseUrl}/api/notify](${baseUrl}/api/notify) to proactively message everyone who has previously messaged this bot.`);
  
  console.log(`✅ Completed message handler for: "${text}" from conversation ${activity.conversation.id}`);
});

// Handle member added events to store conversation reference
app.on("membersAdded", async (context) => {
  const activity = context.activity;
  addConversationReference(activity);
  
  const membersAdded = activity.membersAdded;
  for (const member of membersAdded) {
    if (member.id !== activity.recipient.id) {
      const port = process.env.PORT || 3978;
      
      // Always use localhost for development (no dev tunnel needed)
      let baseUrl = `http://localhost:${port}`;
      
      const welcomeMessage = `Welcome to the **Teams AI v2 Proactive Bot** sample!\n\n• Dashboard: ${baseUrl}/api/conversations\n• Send Proactive Messages: ${baseUrl}/api/notify\n• Use \`/notify\` command for more info\n\n*Pure Teams AI v2 implementation - localhost only*`;
      await context.send(welcomeMessage);
    }
  }
});

// Export functions for proactive messaging
app.getConversationReferences = () => conversationReferences;
app.getLastMessages = () => lastMessageStore;

// Method to access Teams AI v2 internal adapter
app.getInternalAdapter = () => {
  // Try to access the internal adapter from Teams AI v2
  const possibleAdapterPaths = [
    app._adapter,
    app.adapter, 
    app._app?._adapter,
    app._connector,
    app.connector
  ];
  
  for (const adapter of possibleAdapterPaths) {
    if (adapter && typeof adapter.continueConversationAsync === 'function') {
      console.log('Found internal adapter with continueConversationAsync capability');
      return adapter;
    }
  }
  
  console.log('No internal adapter found with proactive messaging capabilities');
  console.log('Available app properties:', Object.keys(app).filter(key => !key.startsWith('_')));
  return null;
};

// Method to send proactive messages - Direct Bot Framework API approach
app.sendProactiveMessage = async (conversationReference, message) => {
  try {
    console.log(`Sending proactive message to ${conversationReference.conversation.id}: ${message}`);
    
    // Direct Bot Framework API call - completely bypass Teams AI v2 internal mechanisms
    const serviceUrl = conversationReference.serviceUrl;
    const conversationId = conversationReference.conversation.id;
    
    const activityPayload = {
      type: 'message',
      text: message,
      from: conversationReference.bot,
      recipient: conversationReference.user,
      conversation: conversationReference.conversation,
      channelId: conversationReference.channelId,
      timestamp: new Date().toISOString(),
      localTimestamp: new Date().toISOString(),
      id: `proactive-${Date.now()}-${Math.random().toString(36).substring(2, 15)}`
    };
    
    console.log('Activity payload:', JSON.stringify(activityPayload, null, 2));
    
    // Get access token for Bot Framework API
    const token = await getAccessToken();
    console.log('Token obtained successfully');
    
    const apiUrl = `${serviceUrl}/v3/conversations/${conversationId}/activities`;
    console.log('API URL:', apiUrl);
    
    const response = await fetch(apiUrl, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
        'User-Agent': 'Microsoft-BotFramework/3.1 (BotFramework-Node.js)',
        'Accept': 'application/json'
      },
      body: JSON.stringify(activityPayload)
    });
    
    const responseText = await response.text();
    console.log('Response status:', response.status);
    console.log('Response text:', responseText);
    
    if (response.ok) {
      console.log(`✅ Proactive message sent successfully to ${conversationReference.conversation.id}`);
      
      if (!app.proactiveMessages) {
        app.proactiveMessages = [];
      }
      
      app.proactiveMessages.push({
        timestamp: new Date().toISOString(),
        conversationId: conversationReference.conversation.id,
        userId: conversationReference.user.id,
        userName: conversationReference.user.name,
        message: message,
        status: 'delivered_direct_api',
        responseStatus: response.status
      });
      
      return {
        success: true,
        conversationId: conversationReference.conversation.id,
        message: message,
        status: 'delivered_direct_api',
        responseStatus: response.status,
        responseBody: responseText
      };
    } else {
      console.error(`❌ API call failed with status: ${response.status}`);
      throw new Error(`API call failed with status: ${response.status} - ${responseText}`);
    }
    
  } catch (error) {
    console.error("❌ Error sending proactive message:", error);
    
    // Store failed attempt
    if (!app.proactiveMessages) {
      app.proactiveMessages = [];
    }
    
    app.proactiveMessages.push({
      timestamp: new Date().toISOString(),
      conversationId: conversationReference.conversation.id,
      userId: conversationReference.user?.id || 'unknown',
      userName: conversationReference.user?.name || 'unknown',
      message: message,
      status: 'failed',
      error: error.message
    });
    
    throw error;
  }
};

// Helper function to get access token for Bot Framework API
async function getAccessToken() {
  try {
    const clientId = process.env.CLIENT_ID;
    const clientSecret = process.env.CLIENT_SECRET;
    
    console.log('Getting access token with clientId:', clientId ? `${clientId.substring(0, 8)}...` : 'undefined');
    
    if (!clientId || !clientSecret) {
      throw new Error('CLIENT_ID and CLIENT_SECRET are required for proactive messaging');
    }
    
    // Use client credentials flow for proactive messaging
    const tokenUrl = 'https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token';
    const params = new URLSearchParams();
    params.append('grant_type', 'client_credentials');
    params.append('client_id', clientId);
    params.append('client_secret', clientSecret);
    params.append('scope', 'https://api.botframework.com/.default');
    
    console.log('Requesting token from:', tokenUrl);
    
    const response = await fetch(tokenUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      },
      body: params
    });
    
    const responseText = await response.text();
    console.log('Token response status:', response.status);
    
    if (!response.ok) {
      console.error('Token request failed:', responseText);
      throw new Error(`Token request failed: ${response.status} - ${responseText}`);
    }
    
    const tokenData = JSON.parse(responseText);
    console.log('Token acquired successfully, expires in:', tokenData.expires_in, 'seconds');
    return tokenData.access_token;
    
  } catch (error) {
    console.error('❌ Error getting access token:', error);
    throw error;
  }
}

// Method to get proactive message history
app.getProactiveMessageHistory = () => {
  return app.proactiveMessages || [];
};

// Method to setup custom routes after app starts
app.setupCustomRoutes = () => {
  return app.addCustomHttpHandler();
};

module.exports = app;
