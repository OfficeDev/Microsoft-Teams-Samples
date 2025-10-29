import { ManagedIdentityCredential } from "@azure/identity";
import { cardAttachment, TokenCredentials } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { IAdaptiveCard } from "@microsoft/teams.cards";
import { ConsoleLogger } from "@microsoft/teams.common/logging";
import { DevtoolsPlugin } from "@microsoft/teams.dev";
import * as path from "path";
import * as fs from "fs";

import {
  createCard,
  createConversationMembersCard,
  createLinkUnfurlCard,
  createMessageDetailsCard,
  createWikipediaCard,
  createNpmPackageCard,
  createAdaptiveCardFromJson,
  createConnectorCard,
  createResultGridCards,
} from "./card";

const createTokenFactory = () => {
  return async (scope: string | string[], tenantId?: string): Promise<string> => {
    const managedIdentityCredential = new ManagedIdentityCredential({
      clientId: process.env.CLIENT_ID,
    });
    const scopes = Array.isArray(scope) ? scope : [scope];
    const tokenResponse = await managedIdentityCredential.getToken(scopes, {
      tenantId: tenantId,
    });

    return tokenResponse.token;
  };
};

const tokenCredentials: TokenCredentials = {
  clientId: process.env.CLIENT_ID || "",
  token: createTokenFactory(),
};

const credentialOptions =
  process.env.BOT_TYPE === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

const app = new App({
  ...credentialOptions,
  logger: new ConsoleLogger("msgext-search-teamsai", { level: "debug" }),
  plugins: [new DevtoolsPlugin()],
});

const publicDir = path.join(__dirname, "../public/Images");
if (!fs.existsSync(publicDir)) {
  fs.mkdirSync(publicDir, { recursive: true });
}

app.on("install.add", async ({ send }) => {
  const greeting = `
  Hi this app handles: Message extension commands - handling card creation.
  `;
  await send(greeting);
});

app.on("message.ext.query-link", async ({ activity }) => {
  const { url } = activity.value;

  if (!url) {
    return { status: 400 };
  }

  const { card, thumbnail } = createLinkUnfurlCard(url);
  const attachment = {
    ...cardAttachment("adaptive", card),
    preview: cardAttachment("thumbnail", thumbnail),
  };

  return {
    composeExtension: {
      type: "result",
      attachmentLayout: "list",
      attachments: [attachment],
    },
  };
});

app.on("message.ext.submit", async ({ activity }) => {
  const { commandId } = activity.value;
  let card: IAdaptiveCard;

  if (commandId === "createCard") {
    card = createCard(activity.value.data);
  } else if (commandId === "getMessageDetails" && activity.value.messagePayload) {
    card = createMessageDetailsCard(activity.value.messagePayload);
  } else {
    throw new Error(`Unknown commandId: ${commandId}`);
  }

  return {
    composeExtension: {
      type: "result",
      attachmentLayout: "list",
      attachments: [cardAttachment("adaptive", card)],
    },
  };
});

app.on("message.ext.open", async ({ activity, api }) => {
  const conversationId = activity.conversation.id;
  const members = await api.conversations.members(conversationId).get();
  const card = createConversationMembersCard(members);

  return {
    task: {
      type: "continue",
      value: {
        title: "Conversation members",
        height: "small",
        width: "small",
        card: cardAttachment("adaptive", card),
      },
    },
  };
});

// @ts-expect-error - Return type compatibility issue with MessagingExtensionResponse
app.on("message.ext.query", async ({ activity }) => {
  const { commandId } = activity.value;
  const searchQuery = activity.value.parameters![0].value;

  if (commandId === "wikipediaSearch") {
    const cards = await createWikipediaCard(searchQuery);
    const attachments = cards.map((item: any) => {
      return {
        ...cardAttachment("hero", item.card),
        preview: cardAttachment("hero", item.thumbnail),
      };
    });

    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: attachments,
      },
    };
  } else if (commandId === "searchQuery") {
    switch (searchQuery) {
      case "adaptive card": {
        const { card, thumbnail } = createAdaptiveCardFromJson();
        const attachment = {
          ...cardAttachment("adaptive", card),
          preview: cardAttachment("thumbnail", thumbnail),
        };
        return {
          composeExtension: {
            type: "result",
            attachmentLayout: "list",
            attachments: [attachment],
          },
        };
      }

      case "connector card": {
        const { card, thumbnail } = createConnectorCard();
        const attachment = {
          contentType: "application/vnd.microsoft.teams.card.o365connector",
          content: card,
          preview: cardAttachment("thumbnail", thumbnail),
        };
        return {
          composeExtension: {
            type: "result",
            attachmentLayout: "list",
            attachments: [attachment as any],
          },
        };
      }

      case "result grid": {
        const baseUrl = process.env.BASE_URL || `https://${process.env.WEBSITE_HOSTNAME}` || "http://localhost:3978";
        const cards = await createResultGridCards(baseUrl);
        const attachments = cards.map((thumbnail) => {
          return cardAttachment("thumbnail", thumbnail);
        });
        return {
          composeExtension: {
            type: "result",
            attachmentLayout: "grid",
            attachments: attachments,
          },
        };
      }

      default: {
        // NPM package search
        const cards = await createNpmPackageCard(searchQuery);
        const attachments = cards.map((item: any) => {
          return {
            ...cardAttachment("hero", item.card),
            preview: cardAttachment("hero", item.thumbnail),
          };
        });

        return {
          composeExtension: {
            type: "result",
            attachmentLayout: "list",
            attachments: attachments,
          },
        };
      }
    }
  }

  return { status: 400 };
});

app.on("message.ext.select-item", async ({ activity }) => {
  const selectedItem = activity.value;
  
  const thumbnail = {
    title: selectedItem.name || "Selected Item",
    text: selectedItem.description || "No description available",
  };

  return {
    composeExtension: {
      type: "result",
      attachmentLayout: "list",
      attachments: [cardAttachment("thumbnail", thumbnail)],
    },
  };
});

(async () => {
  await app.start();
})();

