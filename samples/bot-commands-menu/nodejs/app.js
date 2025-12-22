const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");

// Import card templates
const FlightsDetailsCard = require('./resources/flightsDetails.json');
const SearchHotelsCard = require('./resources/searchHotels.json');

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

const getConversationState = (conversationId) => {
  let state = storage.get(conversationId);
  if (!state) {
    state = { count: 0 };
    storage.set(conversationId, state);
  }
  return state;
};

app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);

  // Handle different commands
  if (/search flights/i.test(text)) {
    await context.send({
      type: 'message',
      attachments: [{
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: FlightsDetailsCard
      }]
    });
    return;
  }

  if (/search hotels/i.test(text)) {
    await context.send({
      type: 'message',
      attachments: [{
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: SearchHotelsCard
      }]
    });
    return;
  }

  if (/help/i.test(text)) {
    await context.send('Displays this help message.');
    return;
  }

  if (/best time to fly/i.test(text)) {
    await context.send('Best time to fly to London for a 5-day trip is summer.');
    return;
  }

  // Handle hotel search details from activity value (form submission)
  if (activity.value) {
    const { checkinDate, checkoutDate, location, numberOfGuests } = activity.value;
    await context.send(`Hotel search details: 
Check-in Date: ${checkinDate}, 
Checkout Date: ${checkoutDate},
Location: ${location}, 
Number of Guests: ${numberOfGuests}`);
    return;
  }

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

  // Default echo behavior
  const state = getConversationState(activity.conversation.id);
  state.count++;
  await context.send(`[${state.count}] you said: ${text}`);
});

module.exports = app;
