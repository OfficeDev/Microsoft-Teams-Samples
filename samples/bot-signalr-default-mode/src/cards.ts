// Adaptive cards for the end-to-end socket demo.
//
// Flow the cards drive:
//   1. User sends any message -> bot replies (over HTTP, per socket-mode design) with invokeDemoCard().
//   2. Each Action.Execute button -> an `adaptiveCard/action` invoke delivered to the bot OVER THE
//      SOCKET. The bot returns an AdaptiveCardInvokeResponse OVER THE SOCKET (client result), and Teams
//      renders the returned card in place — so the socket round-trip is visible right in the chat.
//   3. The "Open dialog" Action.Submit -> a `task/fetch` invoke over the socket; the bot returns a task
//      module (continue) over the socket.

// The main demo card sent as the reply to any user message.
export function invokeDemoCard(statusText?: string): Record<string, unknown> {
  const body: Record<string, unknown>[] = [
    { type: "TextBlock", text: "Prkare Socket Test", weight: "Bolder", size: "Large" },
    {
      type: "TextBlock",
      wrap: true,
      text:
        "Each button sends an **invoke to the bot over the APX socket**; the bot responds **over the socket**. " +
        "The Action.Execute buttons update this card in place so you can see the round-trip.",
    },
  ];
  if (statusText) {
    body.push({ type: "TextBlock", text: statusText, wrap: true, isSubtle: true, spacing: "Medium" });
  }

  return {
    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
    type: "AdaptiveCard",
    version: "1.5",
    body,
    actions: [
      // Action.Execute -> adaptiveCard/action invoke (verb identifies the button). Response over socket.
      { type: "Action.Execute", title: "🔄 Refresh card (socket)", verb: "refreshCard", data: { source: "demoCard" } },
      { type: "Action.Execute", title: "💬 Show message (socket)", verb: "showMessage", data: { source: "demoCard" } },
      { type: "Action.Execute", title: "⛔ Error response (socket)", verb: "errorAction", data: { source: "demoCard" } },
      // Action.Submit with msteams.type=task/fetch -> task/fetch invoke. Response (task module) over socket.
      { type: "Action.Submit", title: "🗔 Open dialog (socket task/fetch)", data: { msteams: { type: "task/fetch" }, source: "demoCard" } },
    ],
  };
}

// Card returned from the refreshCard verb — shows a live counter + timestamp so the socket round-trip
// is obviously fresh each click.
export function refreshedCard(count: number, whenIso: string): Record<string, unknown> {
  return invokeDemoCard(`✅ Refreshed over the socket · click #${count} · ${whenIso}`);
}

// Card shown inside the task module dialog (returned from task/fetch over the socket).
export function taskModuleCard(): Record<string, unknown> {
  return {
    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
    type: "AdaptiveCard",
    version: "1.5",
    body: [
      { type: "TextBlock", text: "Socket task module", weight: "Bolder", size: "Medium" },
      { type: "TextBlock", text: "This dialog was fetched over the APX socket (task/fetch).", wrap: true },
      { type: "Input.Text", id: "note", placeholder: "Type something to submit over the socket…" },
    ],
    actions: [
      { type: "Action.Submit", title: "Submit (socket task/submit)", data: { msteams: { type: "task/submit" } } },
    ],
  };
}
