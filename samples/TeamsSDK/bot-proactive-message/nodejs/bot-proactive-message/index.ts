// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { MessageActivityInput } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";

const app = new App();

// This would be some persistent storage in a real app. It maps a user's
// Microsoft Entra object id to the conversation id we can message them on.
const conversationIdStore = new Map<string, string>();

// Saves the conversation id so it can be used for proactive messaging later.
const rememberConversation = (userId: string | undefined, conversationId: string) => {
  if (!userId) {
    return;
  }

  conversationIdStore.set(userId, conversationId);
};

// Retrieve the conversationId from storage and use it to send the message.
const sendProactiveNotification = async (userId: string | undefined) => {
  const conversationId = userId ? conversationIdStore.get(userId) : undefined;
  if (!conversationId) {
    return;
  }

  const activity = new MessageActivityInput("Hey! It's been a while. How are you?");
  await app.send(conversationId, activity);
};

// A stand-in for a real notification queue / background worker.
const scheduleProactiveNotification = (userId: string | undefined, delayMs: number) => {
  if (!userId) {
    return;
  }

  setTimeout(() => {
    sendProactiveNotification(userId).catch((error) =>
      console.error("[PROACTIVE] Failed to deliver notification:", error)
    );
  }, delayMs);
};

// Installation is just one place to get the conversation id. Every activity
// carries the conversation id, so any handler can capture it.
app.on("install.add", async ({ activity, send }) => {
  rememberConversation(activity.from.aadObjectId, activity.conversation.id);

  await send("Hi! I am going to remind you to say something to me soon!");

  // Queue up a proactive notification to be sent in 10 seconds.
  scheduleProactiveNotification(activity.from.aadObjectId, 10_000);
});

app.on("message", async ({ activity, send }) => {
  const userId = activity.from.aadObjectId;
  rememberConversation(userId, activity.conversation.id);

  const text = activity.text?.trim().toLowerCase() ?? "";

  if (text.includes("remind")) {
    await send("Got it. I will send you a proactive message in 10 seconds.");
    scheduleProactiveNotification(userId, 10_000);
  } else if (text.includes("notify")) {
    await sendProactiveNotification(userId);
  } else {
    await send(
      "Welcome to the proactive message bot! Send 'notify' to receive a proactive message right away, " +
        "or 'remind' to receive one in 10 seconds."
    );
  }
});

app.start().catch(console.error);
