const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const { ConsoleLogger } = require("@microsoft/teams.common/logging");
const endpoints = require('@microsoft/teams.graph-endpoints');

// Create storage for conversation history
const storage = new LocalStorage();

// Create the app with storage and OAuth configuration
const app = new App({
  storage,
  oauth: {
    defaultConnectionName: process.env.CONNECTION_NAME || "oauthbotsetting",
  },
  logger: new ConsoleLogger("bot-teams-auth", { level: "debug" }),
});

app.on("install.add", async ({ send }) => {
  await send("Thanks for installing the bot! Please type **/signin** to sign in.");
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

app.message('/signin', async ({ send, signin, isSignedIn }) => {
  if (isSignedIn) {
    send('you are already signed in!');
  } else {
    await signin();
  }
});

// Handle successful sign-in event
app.event('signin', async ({ send, token }) => {
  await send(`Signed in using OAuth connection ${token.connectionName}. Please type **/whoami** to see your profile or **/signout** to sign out.`);
});

app.message('/whoami', async ({ send, userGraph, isSignedIn}) => {
  if (!isSignedIn) {
    await send('you are not signed in! please type **/signin** to sign in.');
    return;
  }
  const me = await userGraph.call(endpoints.me.get);
  await send(`you are signed in as "${me.displayName}" and your email is "${me.mail || me.userPrincipalName}"`);
});

app.on('message', async ({ send, activity, isSignedIn }) => {
  if (isSignedIn) {
    await send(`You said: "${activity.text}". Please type **/whoami** to see your profile or **/signout** to sign out.`);
  } else {
    await send(`You said: "${activity.text}". Please type **/signin** to sign in.`);
  }
});

app.message('/signout', async ({ send, signout, isSignedIn }) => {
  if (!isSignedIn) {
    await send('you are not signed in! please type **/signin** to sign in.');
    return;
  }
  await signout(); // call signout for your auth connection...
  await send('you have been signed out!');
});

module.exports = app;
