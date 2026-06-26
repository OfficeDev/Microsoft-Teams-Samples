const fs = require('fs');
const ACData = require("adaptivecards-templating");
const { CardFactory } = require('botbuilder');
require('isomorphic-fetch');
const { Client } = require("@microsoft/microsoft-graph-client");
const config = require('../config/default.json');

getAdaptiveCard = (req, res) => {

  const client = Client.init({
    authProvider: (done) => {
      done(null, req.body.token); // First parameter takes an error if you can't get an access token.
    }
  });
  getUsers();

  async function getUsers() {
    try {
      const users = await client.api("/users").get();
      const jsonContentStr = fs.readFileSync('resources/adaptiveCard.json', 'utf8')
      const templatePayload = JSON.parse(jsonContentStr);
      const template = new ACData.Template(templatePayload);
      const cardPayload = template.expand({
        $root: {
          user1Title: users.value[0].displayName,
          user1Id: users.value[0].id,
          user2Title: users.value[1].displayName,
          user2Id: users.value[1].id,
          user3Title: users.value[2].displayName,
          user3Id: users.value[2].id,
          user4Title: users.value[3].displayName,
          user4Id: users.value[3].id,
          user5Title: users.value[4].displayName,
          user5Id: users.value[4].id,
          user6Title: users.value[5].displayName,
          user6Id: users.value[5].id
        }
      });
      const card = CardFactory.adaptiveCard(cardPayload);
      res.send(card);
    }
    catch (error) {
      console.log(error);
    }
  }
}

createGroupChat = (req, res) => {
  const userID = req.body.users.split(",");
  const title = req.body.title;
  const client = Client.init({
    authProvider: (done) => {
      done(null, req.body.token); // First parameter takes an error if you can't get an access token.
    }
  });
  createChat();

  async function createChat() {
    try {
      const chat = {
        chatType: 'group',
        topic: title,
        members: [
          {
            '@odata.type': '#microsoft.graph.aadUserConversationMember',
            roles: ['owner'],
            'user@odata.bind': 'https://graph.microsoft.com/v1.0/users(' + '\'' + userID[0] + '\'' + ')'
          },
          {
            '@odata.type': '#microsoft.graph.aadUserConversationMember',
            roles: ['owner'],
            'user@odata.bind': 'https://graph.microsoft.com/v1.0/users(' + '\'' + req.body.userId + '\'' + ')'
          }
        ]
      };
      var response = await client.api('/chats')
        .post(chat);

      if (userID.length == 2) {
        await addMembersWithHistory(client, response, userID);
        await deleteMember(client, response, userID);
      }
      else if (userID.length == 3) {
        await addMembersWithHistory(client, response, userID);
        await addMembersWithoutHistory(client, response, userID);
        await deleteMember(client, response, userID);
      }
      else if (userID.length >= 4) {
        await addMembersWithHistory(client, response, userID);
        await addMembersWithoutHistory(client, response, userID);
        await addMembersWithNoOfDays(client, response, userID);
        await deleteMember(client, response);
      }

      //Adding Polly App to chat
      const pollyID = config["pollyID"];
      const teamsAppInstallation = {
        'teamsApp@odata.bind': 'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/' + pollyID
      };

      await client.api('/chats/' + response.id + '/installedApps')
        .post(teamsAppInstallation);


      //Adding Polly app as Tab to chat      
      const teamsTab = {
        displayName: 'Polly',
        'teamsApp@odata.bind': 'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/' + pollyID,
        configuration: {
          entityId: 'pollyapp',
          contentUrl: 'https://teams.polly.ai/msteams/content/meeting/tab?theme={theme}',
          websiteUrl: null,
          removeUrl: 'https://teams.polly.ai/msteams/content/tabdelete?theme={theme}'
        }
      };
      await client.api('/chats/' + response.id + '/tabs')
        .post(teamsTab);
    }
    catch (error) {
      console.error();
    }
  }
  //Adding member with all chat history
  async function addMembersWithHistory(client, response, userID) {
    const conversationMember = {
      '@odata.type': '#microsoft.graph.aadUserConversationMember',
      roles: ['owner'],
      'user@odata.bind': 'https://graph.microsoft.com/v1.0/users/' + userID[1],

      visibleHistoryStartDateTime: '0001-01-01T00:00:00Z'
    };
    await client.api('/chats/' + response.id + '/members')
      .post(conversationMember);
  }

  //Adding member with no chat history
  async function addMembersWithoutHistory(client, response, userID) {
    const conversationMember = {
      '@odata.type': '#microsoft.graph.aadUserConversationMember',
      roles: ['owner'],
      'user@odata.bind': 'https://graph.microsoft.com/v1.0/users/' + userID[2]
    };
    await client.api('/chats/' + response.id + '/members')
      .post(conversationMember);
  }

  //Adding member with no. of days history
  async function addMembersWithNoOfDays(client, response, userID) {
    if (userID.length == 4) {
      const conversationMember = {
        '@odata.type': '#microsoft.graph.aadUserConversationMember',
        roles: ['owner'],
        'user@odata.bind': 'https://graph.microsoft.com/v1.0/users/' + userID[3],
        visibleHistoryStartDateTime: ('2021-05-30T23:51:43.255Z')
      };
      await client.api('/chats/' + response.id + '/members')
        .post(conversationMember);
    }

    else if (userID.length > 4) {
      userID.forEach(user => {
        const conversationMember = {
          '@odata.type': '#microsoft.graph.aadUserConversationMember',
          roles: ['owner'],
          'user@odata.bind': 'https://graph.microsoft.com/v1.0/users/' + user,
          visibleHistoryStartDateTime: (new Date().toISOString())
        };
        client.api('/chats/' + response.id + '/members')
          .post(conversationMember);
      });
    }
  }

  //Deleting the first member added
  async function deleteMember(client, response) {
    const chat = await client.api('/chats/' + response.id)
      .expand('members')
      .get();
    var convMemID = chat.members[0].id;
    await client.api('/chats/' + response.id + '/members/' + convMemID)
      .delete();
  }
  res.send(true);
}
module.exports = { getAdaptiveCard, createGroupChat }
