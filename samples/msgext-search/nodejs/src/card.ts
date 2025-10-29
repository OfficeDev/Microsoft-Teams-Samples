import { Account, Message, ThumbnailCard } from "@microsoft/teams.api";
import {
  ActionSet,
  AdaptiveCard,
  CardElement,
  Image,
  OpenUrlAction,
  TextBlock,
  IAdaptiveCard,
} from "@microsoft/teams.cards";
import axios from "axios";
import * as querystring from "querystring";
import * as fs from "fs";
import * as path from "path";

const IMAGE_URL =
  "https://github.com/microsoft/teams-agent-accelerator-samples/raw/main/python/memory-sample-agent/docs/images/memory-thumbnail.png";

interface IFormData {
  title: string;
  subtitle: string;
  text: string;
}

export function createCard(data: IFormData) {
  return new AdaptiveCard(
    new Image(IMAGE_URL),
    new TextBlock(data.title, {
      size: "Large",
      weight: "Bolder",
      color: "Accent",
      style: "heading",
    }),
    new TextBlock(data.subtitle, {
      size: "Small",
      weight: "Lighter",
      color: "Good",
    }),
    new TextBlock(data.text, {
      wrap: true,
      spacing: "Medium",
    })
  );
}

export function createMessageDetailsCard(messagePayload: Message) {
  const cardElements: CardElement[] = [
    new TextBlock("Message Details", {
      size: "Large",
      weight: "Bolder",
      color: "Accent",
      style: "heading",
    }),
  ];

  if (messagePayload?.body?.content) {
    cardElements.push(
      new TextBlock("Content", {
        size: "Medium",
        weight: "Bolder",
        spacing: "Medium",
      }),
      new TextBlock(messagePayload.body.content)
    );
  }

  if (messagePayload?.attachments?.length) {
    cardElements.push(
      new TextBlock("Attachments", {
        size: "Medium",
        weight: "Bolder",
        spacing: "Medium",
      }),
      new TextBlock(`Number of attachments: ${messagePayload.attachments.length}`, {
        wrap: true,
        spacing: "Small",
      })
    );
  }

  if (messagePayload?.createdDateTime) {
    cardElements.push(
      new TextBlock("Created Date", {
        size: "Medium",
        weight: "Bolder",
        spacing: "Medium",
      }),
      new TextBlock(messagePayload.createdDateTime, {
        wrap: true,
        spacing: "Small",
      })
    );
  }

  if (messagePayload?.linkToMessage) {
    cardElements.push(
      new TextBlock("Message Link", {
        size: "Medium",
        weight: "Bolder",
        spacing: "Medium",
      }),
      new ActionSet(
        new OpenUrlAction(messagePayload.linkToMessage, {
          title: "Go to message",
        })
      )
    );
  }

  return new AdaptiveCard(...cardElements);
}

export function createConversationMembersCard(members: Account[]) {
  const membersList = members.map((member) => member.name).join(", ");

  return new AdaptiveCard(
    new TextBlock("Conversation members", {
      size: "Medium",
      weight: "Bolder",
      color: "Accent",
      style: "heading",
    }),
    new TextBlock(membersList, {
      wrap: true,
      spacing: "Small",
    })
  );
}

export async function createDummyCards(searchQuery: string) {
  const dummyItems = [
    {
      title: "Item 1",
      description: `This is the first item and this is your search query: ${searchQuery}`,
    },
    { title: "Item 2", description: "This is the second item" },
    { title: "Item 3", description: "This is the third item" },
    { title: "Item 4", description: "This is the fourth item" },
    { title: "Item 5", description: "This is the fifth item" },
  ];

  const cards = dummyItems.map((item) => {
    return {
      card: new AdaptiveCard(
        new TextBlock(item.title, {
          size: "Large",
          weight: "Bolder",
          color: "Accent",
          style: "heading",
        }),
        new TextBlock(item.description, {
          wrap: true,
          spacing: "Medium",
        })
      ),
      thumbnail: {
        title: item.title,
        text: item.description,
        // When a user clicks on a list item in Teams:
        // - If the thumbnail has a `tap` property: Teams will trigger the `message.ext.select-item` activity
        // - If no `tap` property: Teams will insert the full adaptive card into the compose box
        // tap: {
        //   type: "invoke",
        //   title: item.title,
        //   value: {
        //     "option": index,
        //   },
        // },
      },
    };
  });

  return cards;
}

export function createLinkUnfurlCard(url: string) {
  const thumbnail = {
    title: "Unfurled Link",
    text: url,
    images: [
      {
        url: IMAGE_URL,
      },
    ],
  } as ThumbnailCard;

  const card = new AdaptiveCard(
    new TextBlock("Unfurled Link", {
      size: "Large",
      weight: "Bolder",
      color: "Accent",
      style: "heading",
    }),
    new TextBlock(url, {
      size: "Small",
      weight: "Lighter",
      color: "Good",
    })
  );

  return {
    card,
    thumbnail,
  };
}

// Wikipedia search card creation
export async function createWikipediaCard(searchQuery: string) {
  const wikipediaUrl = `https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&srsearch=${searchQuery}&utf8=1`;

  try {
    const response = await axios.get(wikipediaUrl);
    const cards = response.data.query.search.slice(0, 8).map((result: any) => {
      const card = {
        title: result.title,
        text: result.snippet.replace(/<[^>]*>/g, ""),
        buttons: [
          {
            type: "openUrl",
            title: "Read More",
            value: `https://en.wikipedia.org/wiki/${result.title}`,
          },
        ],
      };

      const thumbnail = {
        title: result.title,
        text: result.snippet.replace(/<[^>]*>/g, ""),
      };

      return { card, thumbnail };
    });

    return cards;
  } catch (error) {
    console.error("Error fetching Wikipedia data:", error);
    return [];
  }
}

// NPM package search card creation
export async function createNpmPackageCard(searchQuery: string) {
  try {
    const response = await axios.get(
      `http://registry.npmjs.com/-/v1/search?${querystring.stringify({
        text: searchQuery,
        size: 8,
      })}`
    );

    const cards = response.data.objects.map((obj: any) => {
      const card = {
        title: obj.package.name,
        text: obj.package.description || "No description available",
      };

      const thumbnail = {
        title: obj.package.name,
        text: obj.package.description || "No description available",
        tap: {
          type: "invoke",
          value: {
            name: obj.package.name,
            description: obj.package.description,
          },
        },
      };

      return { card, thumbnail };
    });

    return cards;
  } catch (error) {
    console.error("Error fetching NPM data:", error);
    return [];
  }
}

// Create adaptive card from JSON (Restaurant card)
export function createAdaptiveCardFromJson() {
  const adaptiveCardJson = {
    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
    type: "AdaptiveCard",
    version: "1.0",
    body: [
      {
        type: "ColumnSet",
        columns: [
          {
            type: "Column",
            width: 2,
            items: [
              {
                type: "TextBlock",
                text: "**PIZZA**",
              },
              {
                type: "TextBlock",
                text: "Tom's Pie",
                weight: "bolder",
                size: "extraLarge",
                spacing: "none",
              },
              {
                type: "TextBlock",
                text: "4.2 $",
                isSubtle: true,
                spacing: "none",
              },
              {
                type: "TextBlock",
                text: '**Matt H. said** "I\'m compelled to give this place 5 stars due to the number of times I\'ve chosen to eat here this past year!"',
                size: "small",
                wrap: true,
              },
            ],
          },
        ],
      },
    ],
  };

  const thumbnail = {
    title: "Adaptive Card",
    text: "Please select to get the card",
  };

  return {
    card: adaptiveCardJson as IAdaptiveCard,
    thumbnail,
  };
}

// Create connector card
export function createConnectorCard() {
  const connectorCardJson = {
    "@context": "https://schema.org/extensions",
    "@type": "MessageCard",
    sections: [
      {
        activityTitle: "Steve Tweeted",
        activitySubtitle: "With #MicrosoftTeams",
      },
      {
        facts: [
          {
            name: "Posted By:",
            value: "Steve",
          },
          {
            name: "Posted At:",
            value: "12:00 PM",
          },
          {
            name: "Tweet:",
            value: "Hello Everyone!! Good Afternoon :-)",
          },
        ],
        text: "A tweet with #MicrosoftTeams has been posted:",
      },
    ],
    potentialAction: [
      {
        "@context": "http://schema.org",
        "@type": "ViewAction",
        name: "View all Tweets",
        target: ["https://twitter.com/search?q=%23MicrosoftTeams"],
      },
    ],
  };

  const thumbnail = {
    title: "Connector Card",
    text: "Please select to get the card",
  };

  return {
    card: connectorCardJson,
    thumbnail,
  };
}

// Create result grid cards from images
export async function createResultGridCards(baseUrl: string) {
  const publicDir = path.join(__dirname, "../public/Images");
  
  // Create directory if it doesn't exist
  if (!fs.existsSync(publicDir)) {
    fs.mkdirSync(publicDir, { recursive: true });
  }

  try {
    const files = fs.readdirSync(publicDir);
    const cards = files.map((file) => {
      return {
        title: "",
        images: [
          {
            url: `${baseUrl}/public/Images/${file}`,
          },
        ],
      } as ThumbnailCard;
    });

    return cards;
  } catch (error) {
    console.error("Error reading images directory:", error);
    return [];
  }
}
