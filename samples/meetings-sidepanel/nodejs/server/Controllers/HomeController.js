const {CardFactory } = require('botbuilder');
const AdaptiveCards = require("adaptivecards");
const sidePanelBot = require('../bots/SidePanelBot');
const configuration = require('dotenv').config();
const env = configuration.parsed;
const appID = env.MicrosoftAppId;
const appPassword = env.MicrosoftAppPassword;
const { ConnectorClient, MicrosoftAppCredentials } = require('botframework-connector');
const credentials = new MicrosoftAppCredentials(appID,appPassword);

getAgenda = (req) =>{
       
    publishAgenda();
    
async function publishAgenda() {
    const agendaPoints = String(req.body.context);
    const array = agendaPoints.split(",");
    const adaptive = await AgendaAdaptiveList(array);
    const client = new ConnectorClient(credentials, { baseUri: sidePanelBot.serviceUrl });

    try {
        MicrosoftAppCredentials.trustServiceUrl(sidePanelBot.serviceUrl);
        await client.conversations.sendToConversation(sidePanelBot.ConversationID, 
            {
            type: 'message',
            from: { id: appID },
            attachments: [CardFactory.adaptiveCard(adaptive)]
            });
        }
    catch(e) {
        console.log(e.message);
        }
    }

    async function AgendaAdaptiveList(agenda)
        {
            const aCard = new AdaptiveCards.AdaptiveCard();       
            aCard.addItem(new AdaptiveCards.TextBlock("**Here is the Agenda for Today**"));     
            agenda.forEach(element => {
                aCard.addItem(new AdaptiveCards.TextBlock(                                      
                "- " + element + "\r"                    
                ));
            }); 
            return aCard;        
        }
    }   

    setContext = (req, res) => {

    const meetingId = req.body.meetingId;
    const userId = req.body.userId;
    const tenantId = req.body.tenantId;
    getRole();

    async function getRole()
    {
         const token = await credentials.getToken();
         const getRoleRequest = await fetch(`${sidePanelBot.serviceUrl}/v1/meetings/${meetingId}/participants/${userId}?tenantId=${tenantId}`,         
            {
             method: 'GET',
             headers: {              
               'Authorization': 'Bearer ' + token
             }
           });
           
           const response = await getRoleRequest.json();
           const role = response.meeting.role;
           if(role=='Organizer')
           {
              res.send(true);
          }
           else{
               res.send(false);
           }
    }
}

module.exports = {
    getAgenda,
    setContext
}