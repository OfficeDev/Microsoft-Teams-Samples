const assert = require("assert");
const { CardFactory } = require("botbuilder");
const createAgendaCard = require("../server/Controllers/AgendaCard");

const card = createAgendaCard(["Introductions", "Q&A", ""]);
const attachment = CardFactory.adaptiveCard(card);
const content = JSON.parse(JSON.stringify(attachment)).content;

assert.strictEqual(attachment.contentType, "application/vnd.microsoft.card.adaptive");
assert.strictEqual(content.type, "AdaptiveCard");
assert.strictEqual(content.version, "1.0");
assert.deepStrictEqual(content.body, [
    { type: "TextBlock", text: "**Here is the Agenda for Today**" },
    { type: "TextBlock", text: "- Introductions\r" },
    { type: "TextBlock", text: "- Q&A\r" },
    { type: "TextBlock", text: "- \r" }
]);
assert.deepStrictEqual(createAgendaCard([]).body, [content.body[0]]);
console.log("Agenda card serialization tests passed.");
