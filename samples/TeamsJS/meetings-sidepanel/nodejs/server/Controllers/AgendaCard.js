function createAgendaCard(agenda) {
    return {
        type: "AdaptiveCard",
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        version: "1.0",
        body: [
            { type: "TextBlock", text: "**Here is the Agenda for Today**" },
            ...agenda.map(element => ({ type: "TextBlock", text: "- " + element + "\r" }))
        ]
    };
}

module.exports = createAgendaCard;
