const configuration = require('dotenv').config();
const store = require('../services/store')
const {createAdaptiveCard }= require('../services/AdaptiveCardService')
const { ConnectorClient, MicrosoftAppCredentials } = require('botframework-connector');
const credentials = new MicrosoftAppCredentials(process.env.BotId,process.env.BotPassword);

 const sendAgenda = async (req) => {
    const data = req.body;
    store.setItem("agendaList", data.taskList);
    const conversationID = store.getItem("conversationId");
    const serviceUrl = store.getItem("serviceUrl");
    const client = new ConnectorClient(credentials, { baseUri: serviceUrl });
    const adaptiveCard = createAdaptiveCard('Poll.json', data.taskInfo)
        try{
              MicrosoftAppCredentials.trustServiceUrl(serviceUrl);
              await client.conversations.sendToConversation(conversationID, 
              {
                type: 'message',
                from: { id: process.env.BotId },
                attachments: [adaptiveCard]
              });
          }
        catch(e){
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