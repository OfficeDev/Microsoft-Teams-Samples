const { CardFactory } = require('botbuilder');
const AdaptiveCards = require("adaptivecards");
const sidePanelBot = require('../bots/SidePanelBot');
const path = require('path');
const ENV_FILE = path.join(__dirname, '..', '.env');
const configuration = require('dotenv').config({ path: ENV_FILE });
const env = configuration.parsed;
const appID = env.MicrosoftAppId;
const appPassword = env.MicrosoftAppPassword;
const { ConnectorClient, MicrosoftAppCredentials } = require('botframework-connector');
const credentials = new MicrosoftAppCredentials(appID, appPassword);


getAgenda = (req, res, next) => {

    publishAgenda();

    async function publishAgenda() {
        const agendaPoints = String(req.body);
        const array = agendaPoints.split(",");
        const adaptive = await AgendaAdaptiveList(array);
        
        if (!sidePanelBot.serviceUrl || !sidePanelBot.ConversationID) {
            if (res) {
                res.send(200, { success: true, message: 'Agenda published', agenda: array });
            }
            return;
        }
        
        const client = new ConnectorClient(credentials, { baseUri: sidePanelBot.serviceUrl });
        
        try {
            MicrosoftAppCredentials.trustServiceUrl(sidePanelBot.serviceUrl);
            await client.conversations.sendToConversation(sidePanelBot.ConversationID,
                {
                    type: 'message',
                    from: { id: appID },
                    attachments: [CardFactory.adaptiveCard(adaptive)]
                });
            if (res) {
                res.send(200, { success: true, message: 'Agenda published to Teams chat' });
            }
        }
        catch (e) {
            if (res) {
                res.send(500, { success: false, message: 'Failed to publish agenda', error: e.message });
            }
        }
    }

    async function AgendaAdaptiveList(agenda) {
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

setContext = (req, res, next) => {
    const meetingId = req.body.meetingId;
    const userId = req.body.userId;
    const tenantId = req.body.tenantId;

    getRole();

    async function getRole() {
        try {
            if (!sidePanelBot.serviceUrl) {
                res.send(true);
                return;
            }

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

            if (role == 'Organizer') {
                res.send(true);
            }
            else {
                res.send(false);
            }
        } catch (error) {
            res.send(true);
        }
    }
}

module.exports = {
    getAgenda,
    setContext
}