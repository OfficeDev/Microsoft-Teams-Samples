const configuration = require('dotenv').config();
const store = require('../services/store')
const {createAdaptiveCard }= require('../services/AdaptiveCardService')
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

 const sendAgenda = async (req) => {
    const data = req.body;
    store.setItem("agendaList", data.taskList);
    const conversationID = store.getItem("conversationId");
    const serviceUrl = store.getItem("serviceUrl");
    const adaptiveCard = createAdaptiveCard('Poll.json', data.taskInfo)
        try{
              const botAppId = process.env.BotId || process.env.AAD_APP_CLIENT_ID || '';
              const conversationReference = {
                  bot: { id: botAppId },
                  conversation: { id: conversationID },
                  serviceUrl: serviceUrl
              };
              
              await adapter.continueConversationAsync(botAppId, conversationReference, async (turnContext) => {
                  await turnContext.sendActivity({
                      type: 'message',
                      attachments: [adaptiveCard]
                  });
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