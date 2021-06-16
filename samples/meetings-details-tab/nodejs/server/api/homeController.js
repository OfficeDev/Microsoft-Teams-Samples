const configuration = require('dotenv').config();
const store = require('../services/store')
const {createAdaptiveCard }= require('../services/AdaptiveCardService')
const env = configuration.parsed;
const appID = env.BotId;
const appPassword = env.BotPassword;
const { ConnectorClient, MicrosoftAppCredentials } = require('botframework-connector');
const credentials = new MicrosoftAppCredentials(appID,appPassword);

 const sendAgenda = async (req) => {
    const data = req.body;
    store.setItem("agendaList", data.taskList);
    const ConversationID = store.getItem("conversationId");
    const ServiceUrl = store.getItem("serviceUrl");
    const client = new ConnectorClient(credentials, { baseUri: ServiceUrl });
    const adaptiveCard = createAdaptiveCard('Poll.json', data.taskInfo)
        try {
          MicrosoftAppCredentials.trustServiceUrl(ServiceUrl);
          await client.conversations.sendToConversation(ConversationID, 
              {
              type: 'message',
              from: { id: appID },
              attachments: [adaptiveCard]
              });
          }
      catch(e) {
          console.log(e.message);
          }
  }
  const getAgendaList = async (req, res) => {
    await res.send(store.getItem("agendaList"));
  }
  const setAgendaList = async (req, res) => {
    store.setItem("agendaList", req.body);
  }
  module.exports ={
    sendAgenda,
    getAgendaList,
    setAgendaList
  }